using DDZManager.Models;
using DDZManager.SmapOneImporter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DDZManager.Controllers
{
    [ApiController]
    [Route("smapone/[controller]/[action]")]
    public class SmapOneImporterController : ControllerBase
    {
        private ILogger<SmapOneImporterController> _logger;
        private readonly SmapOneImporterSettings _settings;
        private readonly SmapOneImporterCronJob _smapOneImporterCronJob;
        private List<string> _controllerProtocol = new();
        public SmapOneImporterController(ILogger<SmapOneImporterController> logger, SmapOneImporterCronJob smapOneImporterCronJob, IOptions<SmapOneImporterSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
            this._smapOneImporterCronJob = smapOneImporterCronJob;
        }

        [HttpGet()]
        public string[] GetControllerProtocol()
        {
            return _controllerProtocol.ToArray();
        }

        [HttpGet()]
        public SmapOneImportStatusModel GetImporterStatus()
        {
            var result = new SmapOneImportStatusModel()
            {
                Status = _smapOneImporterCronJob.IsEnabled ? SmapOneImporterStatus.Run : SmapOneImporterStatus.Stop,
                NextExecutionTimePoint = _smapOneImporterCronJob.NextExecution.ToUnixTimeMilliseconds(),
                LastExecutionTimePoint = _smapOneImporterCronJob.LastExecution.ToUnixTimeMilliseconds()

            };            
            return result;
        }

        [HttpGet()]
        public string[] GetImporterLog()
        {
            var automatorLogFile = _settings.LogFilePath;
            if (System.IO.File.Exists(_settings.LogFilePath))
            {
                var readLines = System.IO.File.ReadLines(automatorLogFile).ToList();
                readLines.Reverse();
                return readLines.GetRange(0, 100).ToArray();
            }
            throw new Exception($"{_settings.LogFilePath} does not exists.");
        }

        [HttpPost()]
        public SmapOneImportStatusModel SetStatus(SmapOneImporterStatus newStatus)
        {
            _logger.LogInformation(newStatus.ToString());
            _smapOneImporterCronJob.IsEnabled = (newStatus == SmapOneImporterStatus.Run);
            var result = new SmapOneImportStatusModel()
            {
                Status = _smapOneImporterCronJob.IsEnabled ? SmapOneImporterStatus.Run : SmapOneImporterStatus.Stop,
                NextExecutionTimePoint = _smapOneImporterCronJob.NextExecution.ToUnixTimeMilliseconds(),
                LastExecutionTimePoint = _smapOneImporterCronJob.LastExecution.ToUnixTimeMilliseconds()
            };
            AddToProtocol($"Status des Service auf '{newStatus}' gesetzt");
            return result;
        }

        [HttpPost()]
        public void ForceStart()
        {
            _ = _smapOneImporterCronJob.Start();
            AddToProtocol($"Importer manuell ausgelöst");
        }

        private void AddToProtocol(string item)
        {
            if(_controllerProtocol.Count > 10)
            {
                _controllerProtocol.RemoveAt(0);
            }
            _controllerProtocol.Add($"{DateTime.Now}: {item}");
        }
    }
}
