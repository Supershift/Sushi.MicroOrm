using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM
{
    /// <summary>
    /// Extension methods for configuring <see cref="Sushi.MicroORM"/> services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a default implementation for the <see cref="Connector{T}"/> service.
        /// </summary>        
        /// <returns></returns>
        public static IServiceCollection AddMessageLoggerFilter(this IServiceCollection services, string defaultConnectionString) 
        {
            // todo: use options or builder pattern for configuration
            DatabaseConfiguration.SetDefaultConnectionString(defaultConnectionString);
            
            services.AddTransient(typeof(Connector<>));

            return services;
        }
    }
}
