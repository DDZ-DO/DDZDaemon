using DDZManager.CronService;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace DDZManager.SmapOneImporter
{
    public class SmapOneImporterCronJob : ICronJob
    {
        private ILogger<SmapOneImporterCronJob> _logger;
        private SmapOneImporterSettings _settings;

        public Guid Id { get; }

        public DateTimeOffset NextExecution { get; private set; }
        public DateTimeOffset LastExecution { get; private set; }

        public TimeSpan Interval { get; } = new TimeSpan(0, 15, 0);

        public bool IsEnabled { get; set; }

        private Process _process;

        private bool IsProcessRunning = false;

        public SmapOneImporterCronJob(ILogger<SmapOneImporterCronJob> logger,
            IOptions<SmapOneImporterSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
            Id = Guid.NewGuid();
            IsEnabled = true;
            _process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _settings.Command,
                    Arguments = _settings.CommandArguments
                }
            };
        }

        public void CalculateNextExecution()
        {
            NextExecution = DateTime.Now.RoundUp(Interval);
            _logger.LogDebug($"Next execution: {NextExecution}");
        }

        public async Task Start()
        {
            _logger.LogInformation("cronjob started");
            await RunSmapOneImporter();
        }

        public async Task ForceStart()
        {
            _logger.LogInformation("forced start");
            await RunSmapOneImporter();
        }

        private async Task RunSmapOneImporter()
        {
            if (IsProcessRunning)
            {
                _logger.LogInformation($"importer is running, will be killed");
                _process.Kill(true);
                await _process.WaitForExitAsync();
            }

            _logger.LogInformation($"starting importer");
            LastExecution = DateTimeOffset.Now;
            _process.Start();
            IsProcessRunning = true;
            _logger.LogInformation($"waiting for finishing");
            _process.WaitForExit(30000);
            IsProcessRunning = false;
            _logger.LogInformation($"finished");
        }
    }
}