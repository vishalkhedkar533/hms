using Quartz;
using System.Reflection;
using System.Text.Json;
using Tasks.Models;
using Tasks.Repository;

namespace Jobs
{
    public sealed class ReflectionJob : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReflectionJob> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ReflectionJob(IServiceProvider serviceProvider, ILogger<ReflectionJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var data = context.JobDetail.JobDataMap;

            var typeName = data.GetString("TargetType");
            var methodName = data.GetString("TargetMethod") ?? "Execute";
            var argsJson = data.GetString("Args") ?? string.Empty;

            if (string.IsNullOrWhiteSpace(typeName))
            {
                _logger.LogError("ReflectionJob missing TargetType in JobDataMap.");
                return;
            }

            // Resolve Type
            var targetType = Type.GetType(typeName)
                ?? AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName == typeName || t.Name == typeName);

            if (targetType == null)
            {
                _logger.LogError("ReflectionJob could not find type '{TypeName}'.", typeName);
                return;
            }

            // Security: optionally check a whitelist here
            // if (!IsAllowed(targetType)) { log & return; }

            // Create a scope so that Scoped services (like IConnectionScope) can be resolved
            using (var scope = _serviceProvider.CreateScope())
            {
                var scopedProvider = scope.ServiceProvider;
                var repo = scope.ServiceProvider.GetRequiredService<IJobTriggerRepository>();
                JobExeHist hist = await repo.CreateJobTriggerDetails(context);
                // Try resolve from DI within the scope; fallback to ActivatorUtilities
                object? instance = scopedProvider.GetService(targetType);
                if (instance == null)
                {
                    try
                    {
                        // We pass scopedProvider here instead of the root _serviceProvider
                        instance = ActivatorUtilities.CreateInstance(scopedProvider, targetType, context);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create instance of {Type}.", targetType.FullName);
                        return;
                    }
                }

                // Find method
                var method = targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                if (method == null)
                {
                    _logger.LogError("Method '{Method}' not found on type {Type}.", methodName, targetType.FullName);
                    return;
                }

                // Build invocation arguments (support 0 or 1 parameter; extend for more)
                var parameters = method.GetParameters();
                object?[] invokeArgs;
                if (parameters.Length == 0)
                {
                    invokeArgs = Array.Empty<object?>();
                }
                else if (parameters.Length == 1)
                {
                    var paramType = parameters[0].ParameterType;
                    object? deserialized = string.IsNullOrWhiteSpace(argsJson)
                        ? (paramType.IsValueType ? Activator.CreateInstance(paramType) : null)
                        : JsonSerializer.Deserialize(argsJson, paramType, _jsonOptions);
                    invokeArgs = new object?[] { deserialized };
                }
                else
                {
                    _logger.LogError("ReflectionJob target method must take 0 or 1 parameters. Found {Count}.", parameters.Length);
                    return;
                }

                try
                {
                    var result = method.Invoke(method.IsStatic ? null : instance, invokeArgs);

                    if (result is Task task)
                    {
                        await task.ConfigureAwait(false);
                    }
                }
                catch (TargetInvocationException tie)
                {
                    _logger.LogError(tie.InnerException ?? tie, "Invocation of {Type}.{Method} failed.", targetType.FullName, methodName);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Invocation of {Type}.{Method} failed.", targetType.FullName, methodName);
                    throw;
                }
            }
        }
    }
}