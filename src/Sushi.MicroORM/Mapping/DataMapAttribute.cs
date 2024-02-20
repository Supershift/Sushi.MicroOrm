using System;

namespace Sushi.MicroORM.Mapping
{
    /// <summary>
    /// Defines the connection between a class and a <see cref="DataMap"/>. Set the attribute on the class you want to map the <see cref="DataMap"/> against.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DataMapAttribute : Attribute
    {
        /// <summary>
        /// Initialiezs a new instance of the <see cref="DataMapAttribute"/> class.
        /// </summary>
        /// <param name="dataMapType"></param>
        public DataMapAttribute(Type dataMapType)
        {
            DataMapType = dataMapType;
        }

        /// <summary>
        /// Gets the type of the class of the <see cref="DataMap"/>
        /// </summary>
        public Type DataMapType { get; protected set; }
    }
}