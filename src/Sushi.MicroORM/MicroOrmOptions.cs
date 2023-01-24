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
    }
}
