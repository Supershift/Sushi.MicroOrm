using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sushi.MicroORM.Mapping;
using Sushi.MicroORM.Supporting;
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
        public static IServiceCollection AddMicroORM(this IServiceCollection services, string defaultConnectionString, Action<MicroOrmConfigurationBuilder> config = null) 
        {   
            services.TryAddTransient(typeof(Connector<>));

            services.TryAddSingleton<DataMapProvider>();

            services.TryAddTransient<SqlExecuter>();
            services.TryAddTransient<ResultMapper>();
            services.TryAddTransient<SqlStatementGenerator>();

            var connectionStringProvider = new ConnectionStringProvider();
            connectionStringProvider.DefaultConnectionString = defaultConnectionString;
            services.TryAddSingleton(connectionStringProvider);

            // perform configuration action for caller
            if(config != null)
            {
                var builder = new MicroOrmConfigurationBuilder(connectionStringProvider);

                config(builder);
            }

            return services;
        }
    }
}
