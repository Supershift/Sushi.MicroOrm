﻿using System;
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
    internal class DataMapProvider
    {        
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
            Type dataMapType = null;
            if (DataMapTypes.ContainsKey(objectType))
            {
                dataMapType = DataMapTypes[objectType];
            }            

            if (dataMapType == null)
            {
                //check if the mapping is declared as a DataMap attribute on T
                dataMapType = this.RetrieveMapFromAttributeOnType<T>();
                if(dataMapType != null)
                {
                    //add the map to collection of datamaptypes
                    this.AddMapping(typeof(T), dataMapType);
                }
            }

            if (dataMapType == null)
            {
                //check if the class has a nested class of type DataMap mapped against type T
                var nestedTypes = objectType.GetNestedTypes();
                for (int i = 0; i < nestedTypes.Length; i++)
                {
                    var nestedType = nestedTypes[i];
                    if(nestedType.IsSubclassOf(typeof(DataMap)))
                    {
                        dataMapType = nestedType;
                        //do we need to check if it is a generic type for T?
                        //add the map to collection of datamaptypes
                        this.AddMapping(typeof(T), dataMapType);
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
        /// Checks if class T has a DataMapAttribute defining the DataMap for T. Returns null if no attribute found
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Type RetrieveMapFromAttributeOnType<T>()
        {
            var dataMapAttribute = Attribute.GetCustomAttribute(typeof(T), typeof(DataMapAttribute)) as DataMapAttribute;
            if (dataMapAttribute != null)
                return dataMapAttribute.DataMapType;
            else
                return null;
        }
    }
}