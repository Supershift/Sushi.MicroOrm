using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Mapping
{
    public class DataMapScanner
    {

        public void Scan(System.Reflection.Assembly[] assemblyList, DataMapProvider dataMapProvider)
        {
            Type? dataMapType = null;

            foreach (var assembly in assemblyList)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    var nestedTypes = type.GetNestedTypes();

                    foreach (var nestedType in nestedTypes)
                    {
                        if (nestedType.IsSubclassOf(typeof(DataMap)))
                        {
                            dataMapType = nestedType;
                            dataMapProvider.AddMapping(type, dataMapType);
                            break;
                        }
                    }
                }
            }
        }
    }
}
