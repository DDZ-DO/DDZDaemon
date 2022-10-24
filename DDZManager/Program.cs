using DDZManager.Airleader;
using DDZManager.CronService;
using DDZManager.SmapOneImporter;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Events;

const string outputTemplate =
    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] [{ThreadId}] [{SourceContext}] {Message}{NewLine}{Exception}";

Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
var builder = WebApplication.CreateBuilder(args);
// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<SmapOneImporterSettings>(builder.Configuration.GetSection(nameof(SmapOneImporterSettings)));
builder.Services.Configure<AirleaderEmailSettings>(builder.Configuration.GetSection(nameof(AirleaderEmailSettings)));
// *** Cronjobs
builder.Services.AddSingleton<AirleaderEmailCronJob>();
builder.Services.AddSingleton<ICronJob>(svcCollection => svcCollection.GetRequiredService<AirleaderEmailCronJob>());
builder.Services.AddSingleton<SmapOneImporterCronJob>();
builder.Services.AddSingleton<ICronJob>(svcCollection => svcCollection.GetRequiredService<SmapOneImporterCronJob>());

builder.Services.AddHostedService<CronService>();

builder.Host.UseWindowsService();
builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(hostingContext.Configuration)
            .Enrich.WithThreadId()
            .Enrich.FromLogContext()
            .WriteTo.Console(LogEventLevel.Information,outputTemplate)
            .WriteTo.File(@"./logs/service.log", LogEventLevel.Information, outputTemplate, rollingInterval: RollingInterval.Month, retainedFileCountLimit: 3));

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(
                 options => options.WithOrigins("http://localhost:4200", "https://localhost:4200").AllowAnyMethod().AllowAnyHeader()
             );
}
else
{    
    app.UseFileServer(new FileServerOptions() { FileProvider = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "../wwwroot")) });    
}

//app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
