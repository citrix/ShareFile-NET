using System;
using System.Linq.Expressions;

namespace ShareFile.Api.Client
{
    internal class NotNullAttribute : Attribute
    {
    }

    internal class Constraint
    {
        [NotNull]
        public static T NotNull<T>(Expression<Func<T>> expression) where T : class
        {
            var result = expression.Compile()();
            if (result == null)
            {
                throw new ConstraintViolationException($"NotNull constraint failed: {ExpressionToString(expression)} was null");
            }
            return result;
        }

        private static string ExpressionToString(Expression expression)
        {
            string prefix = "";
            string memberName = "";
            var fieldExpression = expression as MemberExpression;
            if (fieldExpression != null)
            {
                prefix = ExpressionToString(fieldExpression.Expression);
                memberName = fieldExpression.Member.Name;
            }
            var lambda = expression as LambdaExpression;
            if (lambda != null)
            {
                var memberExpression = lambda.Body as MemberExpression;
                if (memberExpression != null)
                {
                    prefix = ExpressionToString(memberExpression.Expression);
                    memberName = memberExpression.Member.Name;
                }
            }

            if (string.IsNullOrEmpty(memberName))
            {
                return "";
            }
            if (string.IsNullOrEmpty(prefix))
            {
                return memberName;
            }
            return $"{prefix}.{memberName}";
        }

        internal class ConstraintViolationException : Exception
        {
            public ConstraintViolationException(string message)
                : base(message)
            {
            }
        }
    }
}
