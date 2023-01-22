using Serilog;
using WebSockets.API;

// Bootstrapping Serilog Logger
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
        true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateBootstrapLogger();
// ---------------------------------------------

Log.Information("Application is starting up");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();

// Inject serilog into asp.net core ILogger implementation
builder.Logging.ClearProviders();
builder.Host.UseSerilog((hostContext, services, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(hostContext.Configuration);
    loggerConfiguration.ReadFrom.Services(services);
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IWebSocketConnectionManager, WebSocketConnectionManager>();
builder.Services.AddTransient<INotificationMessageHandler, NotificationMessageHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UsePathBase("/websockets-service-dev");
app.UsePathBase("/websockets-service-qa");
app.UsePathBase("/websockets-service-prod");
app.MapHealthChecks("/");
app.UseSerilogRequestLogging();
//app.UseHttpLogging();
app.UseRouting();
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval =
        TimeSpan.FromMinutes(Convert.ToDouble(builder.Configuration.GetSection("WebSockets")["KeepAliveInterval"]))
};
app.UseWebSockets(webSocketOptions);
app.UseAuthorization();

app.MapControllers();

app.Run();