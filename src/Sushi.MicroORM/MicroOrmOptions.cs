using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM
{
    public class MicroOrmOptions
    {
        /// <summary>
        ///  Gets or sets the wait time (in seconds) before terminating the attempt to execute a command and generating an error. 
        ///  If left emtpy, the ADO.NET default command timeout is used.
        /// </summary>
        public int? DefaultCommandTimeOut { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTimeKind"/> applied to <see cref="DateTime"/> values when retrieved from the database. Defaults to <see cref="DateTimeKind.Utc"/>.
        /// </summary>
        public DateTimeKind DateTimeKind { get; set; } = DateTimeKind.Utc;
    }
}
