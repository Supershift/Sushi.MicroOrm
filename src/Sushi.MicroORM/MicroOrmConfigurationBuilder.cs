using System;
using System.Collections.Generic;

namespace Sushi.MicroORM
{      
    /// <summary>
    /// Provides methods to configure Sushi MicroORM.
    /// </summary>
    public class MicroOrmConfigurationBuilder
    {
        private List<Mapping.DataMapProfile> _profiles = new();
        
        /// <summary>
        /// Creates an instance of <see cref="MicroOrmConfigurationBuilder"/>
        /// </summary>
        /// <param name="connectionStringProvider"></param>
        public MicroOrmConfigurationBuilder(ConnectionStringProvider connectionStringProvider)
        {
            ConnectionStringProvider = connectionStringProvider;
        }

        internal IReadOnlyList<Mapping.DataMapProfile> Profiles => _profiles;

        /// <summary>
        /// Gets the <see cref="ConnectionStringProvider"/>.
        /// </summary>
        public ConnectionStringProvider ConnectionStringProvider { get; private set; } 

        /// <summary>
        /// Gets or sets the callback used to configure <see cref="MicroOrmOptions"/>.
        /// </summary>
        public Action<MicroOrmOptions>? Options { get; set; }

        /// <summary>
        /// Add an existing profile type. Profile will be instantiated and added to the configuration.
        /// </summary>
        /// <typeparam name="T">Profile type</typeparam>
        public void AddDataMapProfile<T>() where T : Mapping.DataMapProfile, new()
        {
            var profile = new T();
            _profiles.Add(profile);
        }
    }
}
