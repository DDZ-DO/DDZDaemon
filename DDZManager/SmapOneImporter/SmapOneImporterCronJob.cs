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

        public TimeSpan Interval { get; set; } = new TimeSpan(0, 15, 0);

        public bool IsEnabled { get; set; }

        private Process _process;

        private SemaphoreSlim _processRunning = new SemaphoreSlim(1);

        public SmapOneImporterCronJob(ILogger<SmapOneImporterCronJob> logger, IOptions<SmapOneImporterSettings> settings)
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
            _process.Exited += _process_Exited;
        }

        private void _process_Exited(object? sender, EventArgs e)
        {
            _processRunning.Release();
        }

        public void CalculateNextExecution()
        {
            NextExecution = DateTime.Now.RoundUp(Interval);
            _logger.LogDebug($"Next execution: {NextExecution}");
        }       

        public async Task Start()
        {
            _logger.LogDebug($"Started at {DateTime.Now.ToLongTimeString()}");
            if (_processRunning.CurrentCount == 0)
                return;
            await _processRunning.WaitAsync();
            LastExecution = DateTimeOffset.Now;
            _process.Start();            
            _process.WaitForExit(30000);
        }
    }
}
