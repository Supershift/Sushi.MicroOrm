using System;

namespace Sushi.MicroORM
{      
    /// <summary>
    /// Provides methods to configure Sushi MicroORM.
    /// </summary>
    public class MicroOrmConfigurationBuilder
    {
        /// <summary>
        /// Creates an instance of <see cref="MicroOrmConfigurationBuilder"/>
        /// </summary>
        /// <param name="connectionStringProvider"></param>
        public MicroOrmConfigurationBuilder(ConnectionStringProvider connectionStringProvider)
        {
            ConnectionStringProvider = connectionStringProvider;
        }

        /// <summary>
        /// Gets the <see cref="ConnectionStringProvider"/>.
        /// </summary>
        public ConnectionStringProvider ConnectionStringProvider { get; private set; } 

        /// <summary>
        /// Gets or sets the callback used to configure <see cref="MicroOrmOptions"/>.
        /// </summary>
        public Action<MicroOrmOptions> Options { get; set; }
    }
}
