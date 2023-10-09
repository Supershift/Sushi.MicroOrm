using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Sushi.MicroORM.Mapping
{
    /// <summary>
    /// Provides methods to scan assemblies for <see cref="DataMap"/> implementations.
    /// </summary>
    internal class DataMapScanner
    {
        /// <summary>
        /// Scans the specified assemblies for <see cref="DataMap"/> implementations and adds them to the datamap provider.
        /// </summary>
        /// <param name="assemblyList"></param>
        /// <param name="dataMapProvider"></param>
        public void Scan(System.Reflection.Assembly[] assemblyList, DataMapProvider dataMapProvider)
        {           
            foreach (var assembly in assemblyList)
            {
                var dataMapTypes = assembly.GetExportedTypes().Where(x => x.IsSubclassOf(typeof(DataMap))); 

                foreach (var dataMapType in dataMapTypes)
                {
                    var instance = Activator.CreateInstance(dataMapType);

                    var propertyInfo = dataMapType.GetProperty("MappedType");
                    var mappedTypeValue = propertyInfo!.GetValue(instance, null);

                    dataMapProvider.AddMapping((Type)mappedTypeValue!, dataMapType);
                }
            }
        }
    }
}
