using Microsoft.Extensions.DependencyInjection;
using UUNATRK.Application.Services.Printer;

namespace UUNATRK.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<PrinterService>();

            return services;
        }
    }
}
