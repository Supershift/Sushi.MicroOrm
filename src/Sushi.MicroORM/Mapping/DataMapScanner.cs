using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Sushi.MicroORM.Mapping
{
    public class DataMapScanner
    {

        public void Scan(System.Reflection.Assembly[] assemblyList, DataMapProvider dataMapProvider)
        {           
            foreach (var assembly in assemblyList)
            {
                var instances = assembly.GetExportedTypes();

                foreach (var instance in instances)
                {
                    if (instance.IsSubclassOf(typeof(DataMap)))
                    {
                        var datamapInstance = Activator.CreateInstance(instance);

                        var propertyInfo = instance.GetProperty("MappedType");
                        var mappedTypeValue = propertyInfo!.GetValue(datamapInstance, null);
                        dataMapProvider.AddMapping((Type)mappedTypeValue!, instance);
                    }
                }
            }
        }
    }
}
