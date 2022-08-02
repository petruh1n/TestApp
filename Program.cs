using NLog;
using NLog.Web;

//var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllers(); // используем контроллеры без представлений
    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    var app = builder.Build();
    app.UseDeveloperExceptionPage();
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers(); // подключаем маршрутизацию на контроллеры
    });
    app.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "Программа остановлена из-за исключения");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}