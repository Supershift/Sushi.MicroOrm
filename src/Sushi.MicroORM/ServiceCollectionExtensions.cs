using Microsoft.Extensions.DependencyInjection;

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
        public static IServiceCollection AddMicroORM(this IServiceCollection services, string defaultConnectionString)
        {
            // todo: use options or builder pattern for configuration
            DatabaseConfiguration.SetDefaultConnectionString(defaultConnectionString);

            services.AddTransient(typeof(Connector<>));

            return services;
        }
    }
}