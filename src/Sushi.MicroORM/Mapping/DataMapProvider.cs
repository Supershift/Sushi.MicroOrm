using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Mapping
{
    /// <summary>
    /// Provides methods to map class types to DataMaps
    /// </summary>
    public class DataMapProvider
    {
        /// <summary>
        /// Gets a collection of key value pairs with default relations between <see cref="DataMap"/> classes and mapped classes.
        /// </summary>
        protected ConcurrentDictionary<Type, Type> DataMapTypes { get; } = new ConcurrentDictionary<Type, Type>();

        /// <summary>
        /// Sets the DataMap<typeparamref name="T"/> to use when resolving queries for <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Y"></typeparam>
        public void AddMapping<T, Y>() where T : new() where Y : DataMap<T>, new()
        {
            DataMapTypes[typeof(T)] = typeof(Y);            
        }

        /// <summary>
        /// Sets the type to use when resolving queries for another type.
        /// </summary>
        /// <param name="classToMap"></param>
        /// <param name="dataMap"></param>
        public void AddMapping(Type classToMap, Type dataMap)
        {
            //check if class to map has a default, parameterless constructor
            if (classToMap.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException($"{classToMap} does not have a parameterless constructor", nameof(classToMap));

            //check if dataMap is of type DataMap<classToMap>
            if (dataMap.IsSubclassOf(typeof(DataMap)) == false)
                throw new ArgumentException($"{dataMap} is not of type DataMap<{classToMap}>");

            //TODO: how to check if dataMap is DataMap<ClassToMap> ???

            DataMapTypes[classToMap] = dataMap;
        }

        /// <summary>
        /// Returns an instance of DataMap<typeparamref name="T"/> for <typeparamref name="T"/> if declared. If not, null is returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public DataMap GetMapForType<T>() where T : new()
        {
            var objectType = typeof(T);
            return GetMapForType(objectType);
        }
        /// <summary>
        /// Returns an instance of DataMap for <param name="type"></param> if declared. If not, null is returned
        /// </summary>        
        /// <returns></returns>
        public DataMap GetMapForType(System.Type type)
        {
            Type dataMapType = null;
            if (DataMapTypes.ContainsKey(type))
            {
                dataMapType = DataMapTypes[type];
            }

            if (dataMapType == null)
            {
                //check if the mapping is declared as a DataMap attribute on T
                dataMapType = RetrieveMapFromAttributeOnType(type);
                if (dataMapType != null)
                {
                    //add the map to collection of datamaptypes
                    this.AddMapping(type, dataMapType);
                }
            }

            if (dataMapType == null)
            {
                //check if the class has a nested class of type DataMap mapped against type T
                var nestedTypes = type.GetNestedTypes();
                for (int i = 0; i < nestedTypes.Length; i++)
                {
                    var nestedType = nestedTypes[i];
                    if (nestedType.IsSubclassOf(typeof(DataMap)))
                    {
                        dataMapType = nestedType;
                        //do we need to check if it is a generic type for T?
                        //add the map to collection of datamaptypes
                        this.AddMapping(type, dataMapType);
                        break;
                    }
                }
            }

            if (dataMapType != null)
            {
                //MV: I think this instance should be singleton, so there is only one datamap object for each DataMap. Maybe that singleton container should be backed by MemoryCache to avoid memory issues
                var dataMap = (DataMap)System.Activator.CreateInstance(dataMapType);
                return dataMap;
            }
            else
                return null;
        }
        /// <summary>
        /// Checks if type<param name="type"/> has a DataMapAttribute defining. Returns null if no attribute found
        /// </summary>
        /// <returns></returns>
        public static Type RetrieveMapFromAttributeOnType(System.Type type)
        {
            var dataMapAttribute = Attribute.GetCustomAttribute(type, typeof(DataMapAttribute)) as DataMapAttribute;
            if (dataMapAttribute != null)
                return dataMapAttribute.DataMapType;
            else
                return null;
        }
    }
}
