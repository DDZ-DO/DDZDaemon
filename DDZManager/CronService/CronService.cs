using System.Collections.Concurrent;
using System.Timers;

namespace DDZManager.CronService
{
    public class CronService : IHostedService
    {
        private ConcurrentDictionary<Guid,ICronJob> _cronJobs = new();
        private readonly System.Timers.Timer _cronTimer;
        private readonly ILogger<CronService> _logger;

        public CronService(ILogger<CronService> logger, IEnumerable<ICronJob> cronJobs)
        {
            _cronTimer = new System.Timers.Timer();
            _cronTimer.Elapsed += new ElapsedEventHandler(OnCronTimerEvent);
            _cronTimer.Interval = 5000;
            this._logger = logger;
            foreach (var cronJob in cronJobs)
            {
                _cronJobs.TryAdd(cronJob.Id, cronJob);
            }
        }

        private void OnCronTimerEvent(object? sender, ElapsedEventArgs e)
        {
            _logger.LogDebug("CronTimer triggered");
            foreach(var cronJob in _cronJobs.Values)
            {
                if (cronJob.NextExecution <= DateTime.Now)
                {
                    cronJob.CalculateNextExecution();
                    if(cronJob.IsEnabled)
                        cronJob.Start();
                }                    
            }
        }

        public void AddCronJob(ICronJob cronJob)
        {            
            if (!_cronJobs.TryAdd(cronJob.Id, cronJob))
                throw new Exception("CronJob could not be added");            
        }

        public void RemoveCronJob(Guid id)
        {
            if (!_cronJobs.ContainsKey(id))
                throw new Exception($"CronJob with ID {id} is unknown.");
            _cronJobs.Remove(id, out _);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {                  
            _cronTimer.Enabled = true;
            _logger.LogInformation("CronService started");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cronTimer.Enabled = true;
            _logger.LogInformation("CronService stopped");
            return Task.CompletedTask;
        }
    }
}
