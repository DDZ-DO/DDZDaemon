namespace DDZManager.Airleader;

public class AirleaderEmailSettings
{
    public bool CronJobEnabled { get; set; } = false;
    public string AttachmentTargeDirectory { get; set; }
    
    public string EmailAddress { get; set; }
    
    public string DestinationMailFolderAfterProcessing { get; set; }
}