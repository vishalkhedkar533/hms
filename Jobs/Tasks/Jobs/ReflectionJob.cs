using Quartz;
using System.Reflection;
using System.Text.Json;
using Tasks.Models;
using Tasks.Repository;

namespace Jobs
{
    [DisallowConcurrentExecution]
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

                // Build invocation arguments (support 0, 1, or 2 parameters; if a parameter type is JobExeHist pass hist)
                var parameters = method.GetParameters();
                object?[] invokeArgs;

                if (parameters.Length == 0)
                {
                    invokeArgs = Array.Empty<object?>();
                }
                else if (parameters.Length == 1)
                {
                    var paramType = parameters[0].ParameterType;

                    // If the single parameter expects JobExeHist (or base/interface) pass hist directly
                    if (paramType.IsAssignableFrom(typeof(JobExeHist)))
                    {
                        invokeArgs = new object?[] { hist };
                    }
                    else
                    {
                        object? deserialized = string.IsNullOrWhiteSpace(argsJson)
                            ? (paramType.IsValueType ? Activator.CreateInstance(paramType) : null)
                            : JsonSerializer.Deserialize(argsJson, paramType, _jsonOptions);
                        invokeArgs = new object?[] { deserialized };
                    }
                }
                else if (parameters.Length == 2)
                {
                    // Support common pattern: (T args, JobExeHist hist) or (JobExeHist hist, T args)
                    var p0 = parameters[0].ParameterType;
                    var p1 = parameters[1].ParameterType;

                    object? arg0 = null;
                    object? arg1 = null;

                    // Determine which parameter should receive hist
                    if (p0.IsAssignableFrom(typeof(JobExeHist)))
                    {
                        arg0 = hist;
                        // deserialize argsJson into p1
                        arg1 = string.IsNullOrWhiteSpace(argsJson)
                            ? (p1.IsValueType ? Activator.CreateInstance(p1) : null)
                            : JsonSerializer.Deserialize(argsJson, p1, _jsonOptions);
                    }
                    else if (p1.IsAssignableFrom(typeof(JobExeHist)))
                    {
                        arg1 = hist;
                        // deserialize argsJson into p0
                        arg0 = string.IsNullOrWhiteSpace(argsJson)
                            ? (p0.IsValueType ? Activator.CreateInstance(p0) : null)
                            : JsonSerializer.Deserialize(argsJson, p0, _jsonOptions);
                    }
                    else
                    {
                        _logger.LogError("ReflectionJob target method with 2 parameters must accept JobExeHist in either position.");
                        return;
                    }

                    invokeArgs = new object?[] { arg0, arg1 };
                }
                else
                {
                    _logger.LogError("ReflectionJob target method must take 0, 1 or 2 parameters. Found {Count}.", parameters.Length);
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