using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Sushi.MicroORM.Supporting
{
    /// <summary>
    /// Provides methods to work with properties and fields of mapped objects.
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// Determines the <see cref="Type"/> accessed by <paramref name="memberInfo"/>.
        /// <paramref name="memberInfo"/> must be of type <see cref="MemberTypes.Field"/> or <see cref="MemberTypes.Property"/>.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static Type GetMemberType(MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).FieldType;

                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).PropertyType;

                default:
                    throw new ArgumentException($"Only {MemberTypes.Field} and {MemberTypes.Property} are supported.", nameof(memberInfo));
            }
        }

        /// <summary>
        /// Determines the <see cref="Type"/> accessed by <paramref name="memberInfoTree"/>.
        /// <paramref name="memberInfoTree"/> items must be of type <see cref="MemberTypes.Field"/> or <see cref="MemberTypes.Property"/>.
        /// </summary>
        /// <param name="memberInfoTree"></param>
        /// <returns></returns>
        public static Type GetMemberType(List<MemberInfo> memberInfoTree)
        {
            return GetMemberType(memberInfoTree.LastOrDefault());
        }

        /// <summary>
        /// Gets the value of the deepest level member defined by <paramref name="memberInfoTree"/> on <paramref name="entity"/>.
        /// </summary>
        /// <param name="memberInfoTree"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static object GetMemberValue(List<MemberInfo> memberInfoTree, object entity)
        {
            if (memberInfoTree == null)
                throw new ArgumentNullException(nameof(memberInfoTree));
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (memberInfoTree.Count == 0)
                throw new ArgumentException("cannot contain zero items", nameof(memberInfoTree));

            foreach (var memberInfo in memberInfoTree)
            {
                entity = GetMemberValue(memberInfo, entity);
                if (entity == null)
                    return null;
            }
            return entity;
        }

        /// <summary>
        /// Gets the value of the member defined by <paramref name="memberInfo"/> on <paramref name="entity"/>.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static object GetMemberValue(MemberInfo memberInfo, object entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).GetValue(entity);

                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).GetValue(entity);

                default:
                    throw new ArgumentException($"Only {MemberTypes.Field} and {MemberTypes.Property} are supported.", nameof(memberInfo));
            }
        }

        /// <summary>
        /// Sets <paramref name="value"/> on the property defined by <paramref name="memberInfo"/> on <paramref name="entity"/>.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="value"></param>
        /// <param name="entity"></param>
        public static void SetMemberValue(MemberInfo memberInfo, object value, object entity)
        {
            // if this is a nullable type, we need to get the underlying type (ie. int?, float?, guid?, etc.)
            var type = GetMemberType(memberInfo);
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }

            if (value == DBNull.Value)
            {
                value = null;
            }
            else
            {
                value = ConvertValueToEnum(value, type);
            }

            // custom support for converting to DateOnly and TimeOnly
            if (type == typeof(DateOnly) && value is DateTime dt1)
            {
                value = DateOnly.FromDateTime(dt1);
            }
            else if (type == typeof(TimeOnly))
            {
                switch (value)
                {
                    case DateTime dt2:
                        value = TimeOnly.FromDateTime(dt2);
                        break;

                    case TimeSpan ts:
                        value = TimeOnly.FromTimeSpan(ts);
                        break;
                }
            }

            try
            {
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Field:
                        ((FieldInfo)memberInfo).SetValue(entity, value);
                        break;

                    case MemberTypes.Property:
                        ((PropertyInfo)memberInfo).SetValue(entity, value, null);
                        break;

                    default:
                        throw new ArgumentException($"Only {MemberTypes.Field} and {MemberTypes.Property} are supported.", nameof(memberInfo));
                }
            }
            catch (Exception innerException)
            {
                string valueType = value == null ? "unknown (=NULL)" : value.GetType().ToString();
                string message = $"Error while setting the {memberInfo.Name} member with an object of type {value}";

                throw new Exception(message, innerException);
            }
        }

        /// <summary>
        /// Sets <paramref name="value"/> on the property defined by <paramref name="memberInfoTree"/> on <paramref name="entity"/>.
        /// </summary>
        /// <param name="memberInfoTree"></param>
        /// <param name="value"></param>
        /// <param name="entity"></param>
        public static void SetMemberValue(List<MemberInfo> memberInfoTree, object value, object entity)
        {
            if (memberInfoTree == null)
                throw new ArgumentNullException(nameof(memberInfoTree));
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (memberInfoTree.Count == 0)
                throw new ArgumentException("cannot contain zero items", nameof(memberInfoTree));

            if (memberInfoTree.Count > 1)
            {
                foreach (var memberInfo in memberInfoTree.GetRange(0, memberInfoTree.Count - 1))
                {
                    //get current node value, if null, create a new object
                    var instance = GetMemberValue(memberInfo, entity);
                    if (instance == null)
                    {
                        var type = GetMemberType(memberInfo);
                        instance = Activator.CreateInstance(type);
                        SetMemberValue(memberInfo, instance, entity);
                    }
                    entity = instance;
                }
            }
            //now set the db value on the final member
            var lastMemberInfo = memberInfoTree.Last();
            SetMemberValue(lastMemberInfo, value, entity);
        }

        /// <summary>
        /// Converts <paramref name="value"/> to an enumeration member if <paramref name="type"/> or its underlying <see cref="Type"/> is an <see cref="Enum"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ConvertValueToEnum(object value, Type type)
        {
            //if the type is an enum, we need to convert the value to the enum's type
            if (type.IsEnum)
            {
                value = Enum.ToObject(type, value);
            }

            return value;
        }

        /// <summary>
        /// Gets a collection of <see cref="MemberInfo"/> objects represented by the expression. The expression needs to be a <see cref="MemberExpression"/> or a <see cref="UnaryExpression"/> wrapping a <see cref="MemberExpression"/>.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static List<MemberInfo> GetMemberTree<T>(Expression<Func<T, object>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            //get body of the expression
            var body = expression.Body;

            var current = body;
            var result = new List<MemberInfo>();
            do
            {
                //unwrap the expression contained by the unary expression
                if (current is UnaryExpression unaryExpression)
                {
                    current = unaryExpression.Operand;
                }

                switch (current)
                {
                    case MemberExpression memberExpression:
                        result.Add(memberExpression.Member);
                        current = ((MemberExpression)current).Expression;
                        break;

                    case ParameterExpression parameterExpression:
                        //this is the root node
                        current = null;
                        break;

                    default:
                        //unsupported expression type
                        throw new Exception($"Unsupported expression type {current.GetType()} in: {body}, only MemberExpressions are supported.");
                }
            }
            while (current != null);

            //reverse the result, so it starts with the lowest level property
            if (result.Count > 1)
                result.Reverse();

            return result;
        }
    }
}