﻿using Microsoft.Extensions.DependencyInjection;
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
        public static IServiceCollection AddMicroORM(this IServiceCollection services, string defaultConnectionString, Action<MicroOrmConfigurationBuilder>? config = null) 
        {   
            services.TryAddTransient(typeof(IConnector<>), typeof(Connector<>));

            services.TryAddSingleton<DataMapProvider>();

            services.TryAddTransient<SqlExecuter>();
            services.TryAddTransient<ResultMapper>();
            services.TryAddTransient<SqlStatementGenerator>();

            var connectionStringProvider = new ConnectionStringProvider(defaultConnectionString);            
            services.TryAddSingleton(connectionStringProvider);

            // create config builder
            var microOrmBuilder = new MicroOrmConfigurationBuilder(connectionStringProvider);
            var optionsBuilder = services.AddOptions<MicroOrmOptions>();

            // execute configuration callbacks
            if (config != null)
            {
                config(microOrmBuilder);

                if(microOrmBuilder.Options != null)
                {
                    optionsBuilder.Configure(microOrmBuilder.Options);
                }
            }

            return services;
        }
    }
}
