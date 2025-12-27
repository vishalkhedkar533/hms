using Quartz;
using Quartz.Spi;

namespace Jobs
{
    /// <summary>
    /// Minimal IJobFactory that resolves job instances from IServiceProvider (DI).
    /// </summary>
    public class ServiceProviderJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobType = bundle.JobDetail.JobType;
            var job = (IJob?)_serviceProvider.GetService(jobType);
            return job ?? throw new InvalidOperationException($"Unable to resolve job type {jobType.FullName} from IServiceProvider.");
        }

        public void ReturnJob(IJob job)
        {
            // Let DI container manage disposal (if applicable).
            (job as IDisposable)?.Dispose();
        }

        IJob IJobFactory.NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            throw new NotImplementedException();
        }

        void IJobFactory.ReturnJob(IJob job)
        {
            throw new NotImplementedException();
        }
    }
}