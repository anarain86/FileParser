using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace FileParser
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ///Generate Host Builder and Register the Services for DI
            var builder = new HostBuilder()
               .ConfigureServices((hostContext, services) =>
               {
                   services.AddScoped<Form1>();
                   //services.AddScoped<IBusinessLayer, BusinessLayer>();
                   //services.AddSingleton<IDataAccessLayer, CDataAccessLayer>();


                   //Add Serilog
                   var serilogLogger = new LoggerConfiguration()
                       .WriteTo.RollingFile("AcedsNoticeFileParser.txt")
                       .CreateLogger();

                   services.AddLogging(x =>
                   {
                       x.SetMinimumLevel(LogLevel.Information);
                       x.AddSerilog(logger: serilogLogger, dispose: true);
                   });

               });

            var host = builder.Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                try
                {
                    var form1 = services.GetRequiredService<Form1>();
                    Application.Run(form1);

                    Console.WriteLine("Success");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Occured");
                }
            }
        }
    }
}