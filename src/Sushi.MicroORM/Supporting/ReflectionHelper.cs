using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Supporting
{
    // Is this class based on something? Please add source (URL)
    public static class ReflectionHelper
    {
        /// <summary>
        /// Sets <paramref name="value"/> on the property defined by <paramref name="info"/> on <paramref name="entity"/>.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="value"></param>
        /// <param name="entity"></param>
        public static void SetPropertyValue(PropertyInfo info, object value, object entity)
        {
            if (value == DBNull.Value)
            {
                value = null;
            }
            else
            {
                var type = info.PropertyType;
                value = ConvertValueToEnum(value, type);
            }
            try
            {
                //set the value on the entity's correct property
                info.SetValue(entity, value, null);
            }
            catch (Exception innerException)
            {
                string message = string.Format("Error while setting the {1} property of type {0} with type {2}"
                    , info.PropertyType.ToString() //0
                    , info.Name //1
                    , value == null ? "unknown (=NULL)" : value.GetType().ToString() //2
                    );
                throw new Exception(message, innerException);
            }
        }

        /// <summary>
        /// Converts <paramref name="value"/> to an enumeration member if <paramref name="type"/> or its underlying <see cref="Type"/> is an <see cref="Enum"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ConvertValueToEnum(object value, Type type)
        {
            //if this is a nullable type, we need to get the underlying type (ie. int?, float?, guid?, etc.)            
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
                type = underlyingType;

            //if the type is an enum, we need to convert the value to the enum's type
            if (type.IsEnum)
            {
                value = Enum.ToObject(type, value);
            }

            return value;
        }

        public static PropertyInfo GetMember(Expression expression)
        {
            if (IsIndexedPropertyAccess(expression))
                return GetDynamicComponentProperty(expression);

            if (IsMethodExpression(expression))
            {
                var method = ((MethodCallExpression)expression).Method;
                return null;
            }
            var member = GetMemberExpression(expression, true);
            return (PropertyInfo)member.Member;
        }

        private static MemberExpression GetMemberExpression(Expression expression, bool enforceCheck)
        {
            MemberExpression memberExpression = null;
            if (expression.NodeType == ExpressionType.Convert)
            {
                var body = (UnaryExpression)expression;
                memberExpression = body.Operand as MemberExpression;
            }
            else if (expression.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = expression as MemberExpression;
            }

            if (enforceCheck && memberExpression == null)
            {
                throw new ArgumentException("Does not access a property or field.", nameof(expression));
            }

            return memberExpression;
        }

        private static PropertyInfo GetDynamicComponentProperty(Expression expression)
        {
            Type desiredConversionType = null;
            MethodCallExpression methodCallExpression = null;
            var nextOperand = expression;

            while (nextOperand != null)
            {
                if (nextOperand.NodeType == ExpressionType.Call)
                {
                    methodCallExpression = nextOperand as MethodCallExpression;
                    desiredConversionType = desiredConversionType ?? methodCallExpression.Method.ReturnType;
                    break;
                }

                if (nextOperand.NodeType != ExpressionType.Convert)
                    throw new ArgumentException("Expression not supported", nameof(expression));

                var unaryExpression = (UnaryExpression)nextOperand;
                desiredConversionType = unaryExpression.Type;
                nextOperand = unaryExpression.Operand;
            }

            var constExpression = methodCallExpression.Arguments[0] as ConstantExpression;

            return new DummyPropertyInfo((string)constExpression.Value, desiredConversionType);
        }

        private static bool IsIndexedPropertyAccess(Expression expression)
        {
            return IsMethodExpression(expression) && expression.ToString().Contains("get_Item");
        }

        private static bool IsMethodExpression(Expression expression)
        {
            return expression is MethodCallExpression || (expression is UnaryExpression && IsMethodExpression((expression as UnaryExpression).Operand));
        }
    }
}
