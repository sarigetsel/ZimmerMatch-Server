using Common.Dto;
using Microsoft.Extensions.DependencyInjection;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    // הוספת שירותים-הרחבות
    public static class ExtensionService
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddRepository();
            services.AddScoped<IService<AvailabilityDto>, AvailabilityService>();
            services.AddScoped<IService<UserDto>,UserService>();
            services.AddScoped<IService<ZimmerDto>,ZimmerService>();
            // להוסיף פה גם את ה isExist למחלקה שצריכה
            return services;
        }
    }
}
