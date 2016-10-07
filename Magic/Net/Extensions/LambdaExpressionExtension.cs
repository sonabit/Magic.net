using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Magic.Net.Data;

namespace Magic.Net
{
    public static class LambdaExpressionExtension
    {
        public static INetCommand ToNetCommand([NotNull]this LambdaExpression expression)
        {
            var methodCallExpression = (MethodCallExpression)expression.Body;

            MethodInfo method = methodCallExpression.Method;
            object[] parameterValues = methodCallExpression.Arguments.Select(a => a.GetArgumentValue()).ToArray();
            ParameterInfo[] parameterInfos = methodCallExpression.Method.GetParameters().ToArray();
            
            var serviceType = methodCallExpression.Method.DeclaringType;

            return new NetCommand(serviceType, method, parameterInfos, parameterValues);
        }

        private static object GetArgumentValue(this Expression argumentExpression)
        {
            if (argumentExpression == null)
                return null;

            object result = argumentExpression;
            do
            {
                ConstantExpression constantExpression = result as ConstantExpression;
                if (constantExpression != null)
                {
                    result = constantExpression.Value;
                }

                MemberExpression memberExp = result as MemberExpression;
                if (memberExp != null)
                {
                    ConstantExpression constcExpression = (ConstantExpression)memberExp.Expression;
                    if (memberExp.Member is FieldInfo)
                    {
                        result = ((FieldInfo)memberExp.Member).GetValue(constcExpression.Value);
                    }

                }
            } while (result is Expression);

            return result;
            
        }
    }
}