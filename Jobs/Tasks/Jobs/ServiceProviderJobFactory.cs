using Quartz;
using Quartz.Spi;
using Microsoft.Extensions.DependencyInjection;

namespace Jobs
{
    /// <summary>
    /// Minimal IJobFactory that resolves job instances from IServiceProvider (DI).
    /// Falls back to ActivatorUtilities if the job type isn't registered.
    /// </summary>
    public class ServiceProviderJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        IJob IJobFactory.NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobType = bundle.JobDetail.JobType;

            // First try to resolve from DI container
            var job = (IJob?)_serviceProvider.GetService(jobType);
            if (job != null)
            {
                return job;
            }

            // Fallback: create via ActivatorUtilities so job can still be constructed
            try
            {
                var created = (IJob)ActivatorUtilities.CreateInstance(_serviceProvider, jobType);
                return created;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to resolve or create job type {jobType.FullName} from IServiceProvider.", ex);
            }
        }

        void IJobFactory.ReturnJob(IJob job)
        {
            // Let DI container manage disposal (if applicable).
            (job as IDisposable)?.Dispose();
        }
    }
}