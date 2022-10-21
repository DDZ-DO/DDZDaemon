using DDZManager.SmapOneImporter;

namespace DDZManager.Models
{
    public class SmapOneImportStatusModel
    {
        public SmapOneImporterStatus Status { get; set; }

        public long NextExecutionTimePoint { get; set; }

        public long LastExecutionTimePoint { get; set; }

    }
}
