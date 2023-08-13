using System.Net.Http.Headers;
using DDZ.Airleader.CronService;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using File = System.IO.File;

namespace DDZ.Airleader.Email;

public class AirleaderEmailCronJob : ICronJob
{
    private readonly ILogger<AirleaderEmailCronJob> _logger;
    private readonly AirleaderEmailSettings _settings;
    public Guid Id { get; }
    public DateTimeOffset NextExecution { get; private set; }
    public bool IsEnabled { get; set; }

    private TimeSpan Interval { get; set; } = new TimeSpan(0, 20, 0);

    public AirleaderEmailCronJob(ILogger<AirleaderEmailCronJob> logger,IOptions<AirleaderEmailSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
        IsEnabled = _settings.CronJobEnabled;
        Id = Guid.NewGuid();
    }

    public void CalculateNextExecution()
    {
        NextExecution = DateTime.Now.RoundUp(Interval);
        _logger.LogDebug($"Next execution: {NextExecution}");
    }

    public async Task Start()
    {
        _logger.LogInformation("cronjob started");
        try
        {
            var graphClient = await Login();
            var mailFolders = await graphClient.Users[_settings.EmailAddress].MailFolders
                .Request()
                .GetAsync();
            
            string mailFolderId = null;
            foreach (var mailFolder in mailFolders)
            {
                if (mailFolder.DisplayName == _settings.DestinationMailFolderAfterProcessing)
                {
                    mailFolderId = mailFolder.Id;
                    break;
                }
            }
            if (mailFolderId == null)
                throw new Exception($"Mailfolder '{_settings.DestinationMailFolderAfterProcessing}' not found");
            _logger.LogDebug("Mailfolder-ID found: {Id}", mailFolderId);
            
            var messages = await graphClient.Users[_settings.EmailAddress].MailFolders["inbox"].Messages
                .Request()
                .Select("sender,subject")
                .Top(1000)
                .GetAsync();
            _logger.LogInformation("Found {MessagesCount} new messages.", messages.Count);
            foreach (var msg in messages)
            {
                var attachments = await graphClient.Users[_settings.EmailAddress].Messages[msg.Id].Attachments.Request().GetAsync();
                var firstAtt = attachments.FirstOrDefault();
                if (firstAtt != null && firstAtt is FileAttachment fa)
                {
                    var filename = Path.Combine(_settings.AttachmentTargeDirectory,fa.Name);
                    SaveByteArrayToFileWithBinaryWriter(fa.ContentBytes, filename);
                    _logger.LogInformation("Created file {Name}", fa.Name);
                }
                await graphClient.Users[_settings.EmailAddress].Messages[msg.Id].Move(mailFolderId).Request().PostAsync();
                _logger.LogInformation("Moved message to {FolderName}", _settings.DestinationMailFolderAfterProcessing );
            }
            _logger.LogInformation("cronjob finished");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Graphclient");
            throw;
        }
    }

    private static void SaveByteArrayToFileWithBinaryWriter(byte[] data, string filePath)
    {
        using var writer = new BinaryWriter(File.OpenWrite(filePath));
        writer.Write(data);
    }

    private async Task<GraphServiceClient> Login()
    {
        // https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/~/Overview/appId/f625d2d1-fd52-4fed-8653-00e3ab57f33d/objectId/d9513471-288d-464b-b419-cffec1ce2267/isMSAApp~/false/defaultBlade/Overview/appSignInAudience/AzureADMyOrg/servicePrincipalCreated~/true
        // Secret muss nach 24 Monaten erneuert werden 21.10.2022
        // https://learn.microsoft.com/en-us/graph/auth-register-app-v2
        var authentication = new
        {
            Authority = "https://graph.microsoft.com", //fix
            Directory = "7746237a-ff88-4f28-9ced-3eae6358054e", //Verzeichnis-ID (Mandant)
            Application = "f625d2d1-fd52-4fed-8653-00e3ab57f33d", //Anwendungs-ID (Client)
            _settings.ClientSecret
        };

        var app = ConfidentialClientApplicationBuilder.Create(authentication.Application)
            .WithClientSecret(authentication.ClientSecret)
            .WithAuthority(AzureCloudInstance.AzurePublic, authentication.Directory)
            .Build();

        var scopes = new[] { "https://graph.microsoft.com/.default" };

        var authenticationResult = await app.AcquireTokenForClient(scopes)
            .ExecuteAsync();

        var graphServiceClient = new GraphServiceClient(
            new DelegateAuthenticationProvider(x =>
            {
                x.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer", authenticationResult.AccessToken);

                return Task.FromResult(0);
            }));
        return graphServiceClient;
    }
}