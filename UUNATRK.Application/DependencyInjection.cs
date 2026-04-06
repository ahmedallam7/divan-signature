using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UUNATRK.Application.Models;
using UUNATRK.Application.Services.Printer;

namespace UUNATRK.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,
            IConfiguration? configuration = null)
        {
            if (configuration?.GetSection("Printer").Exists() == true)
                services.Configure<PrinterSettings>(configuration.GetSection("Printer"));
            else
                services.Configure<PrinterSettings>(_ => { });

            services.AddSingleton<PrinterService>();

            return services;
        }
    }
}
