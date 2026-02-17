using Microsoft.Extensions.DependencyInjection;
using UUNATRK.Application;
using UUNATRK.Application.Services.Printer;

namespace UUNATEK.WindowsForm
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var services = new ServiceCollection();
            services.AddApplicationServices();
            var serviceProvider = services.BuildServiceProvider();

            var printer = serviceProvider.GetRequiredService<PrinterService>();

            Application.Run(new Form1(printer));
        }
    }
}
