using MediatR;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UMS.Application.Common.Behaviours;

namespace UMS.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // MediatR pipeline: validation before every handler
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

            return services;
        }
    }
}
