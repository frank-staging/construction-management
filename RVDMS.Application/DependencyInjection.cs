using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using MediatR;

using Microsoft.Extensions.DependencyInjection;
using RVDMS.Application.Auth;
using RVDMS.Application.Behaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RVDMS.Application.Mappings;

namespace RVDMS.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
            });
            services.AddValidatorsFromAssembly(assembly);
            services.AddAutoMapper(cfg =>  
            {
                cfg.AddMaps(typeof(MappingProfile).Assembly);
            });
            services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            // Permission Authorization
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            return services;
        }
    }
}
