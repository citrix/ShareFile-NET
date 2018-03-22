using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ShareFile.Api.Client;
using ShareFile.Api.Client.Requests;
using System.Reflection;
using ShareFile.Api.Client.Models;
using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Extensions
{
    public static class QueryExtensions
    {
        public static IQuery<Target> Project<Model, Target>(this IQuery<Model> modelQuery, Expression<Func<Model, Target>> mapExpr) 
            where Model : ODataObject
            where Target : class
        {
            IQuery<Model> modifiedQuery = ApplySelectsAndExpands(modelQuery, mapExpr);
            Query<Target> mappedQuery = new MappedQuery<Model, Target>(modifiedQuery as Query<Model>, mapExpr.Compile());
            return mappedQuery;
        }

        public static IQuery<ICollection<Target>> Project<Model, Target>(this IQuery<ODataFeed<Model>> modelQuery, Expression<Func<Model, Target>> mapExpr)
            where Model : ODataObject
        {
            IQuery<ODataFeed<Model>> modifiedQuery = ApplySelectsAndExpands(modelQuery, mapExpr);
            Query<ICollection<Target>> mappedQuery = new MappedQuery<ODataFeed<Model>, ICollection<Target>>(modifiedQuery as Query<ODataFeed<Model>>, feed => feed.Feed.Select(mapExpr.Compile()).ToList());
            return mappedQuery;
        }

        public static IQuery<Model> Select<Model, Property>(this IQuery<Model> modelQuery, Expression<Func<Model, Property>> selectExpr)
            where Model : ODataObject
        {
            IQuery<Model> modifiedQuery = ApplySelectsAndExpands(modelQuery, selectExpr);
            return modifiedQuery;
        }

        public static IQuery<ODataFeed<Model>> Select<Model, Property>(this IQuery<ODataFeed<Model>> modelQuery, Expression<Func<Model, Property>> selectExpr)
            where Model : ODataObject
        {
            IQuery<ODataFeed<Model>> modifiedQuery = ApplySelectsAndExpands(modelQuery, selectExpr);
            return modifiedQuery;
        }

        public static IQuery<Model> Expand<Model, SubModel>(this IQuery<Model> modelQuery, Expression<Func<Model, SubModel>> expandExpr)
            where Model : ODataObject
        {
            IQuery<Model> modifiedQuery = ApplySelectsAndExpands(modelQuery, expandExpr);
            return modifiedQuery;
        }

        public static IQuery<ODataFeed<Model>> Expand<Model, SubModel>(this IQuery<ODataFeed<Model>> modelQuery, Expression<Func<Model, SubModel>> expandExpr)
            where Model : ODataObject
        {
            IQuery<ODataFeed<Model>> modifiedQuery = ApplySelectsAndExpands(modelQuery, expandExpr);
            return modifiedQuery;
        }

        public static IQuery<ODataFeed<Model>> FilterBy<Model>(this IQuery<ODataFeed<Model>> query, 
            Expression<Func<Model, object>> property,
            Requests.Filters.Filter.Operator @operator,
            string value)
        {
            return query.FilterBy(property, @operator, new Requests.Filters.Filter.Value(value));
        }

        public static IQuery<ODataFeed<Model>> FilterBy<Model>(this IQuery<ODataFeed<Model>> query,
            Expression<Func<Model, object>> property, 
            Requests.Filters.Filter.Operator @operator, 
            Requests.Filters.Filter.Value value)
        {
            string propertyName = property.ResolveProperty().Name;
            var filter = @operator.Op(propertyName, value);
            return query.Filter(filter);
        }

        private static MemberInfo ResolveProperty<Model>(this Expression<Func<Model, object>> lambda)
        {
            var expr = lambda.Body;
            if (expr.NodeType == ExpressionType.Convert
                    || expr.NodeType == ExpressionType.ConvertChecked
                    || expr.NodeType == ExpressionType.TypeAs
                    || expr.NodeType == ExpressionType.Coalesce)
            {
                expr = ((UnaryExpression)expr).Operand;
            }

            if (expr.NodeType == ExpressionType.MemberAccess)
            {
                return ((MemberExpression)expr).Member;
            }

            throw new Exception("Unrecognized property expression");
        }


        #region old stuff
        public static Target SelectAndExecute<Model, Target>(this IQuery<Model> modelQuery, Expression<Func<Model, Target>> mapExpr) where Model : ODataObject
        {
            IQuery<Model> modifiedQuery = ApplySelectsAndExpands(modelQuery, mapExpr);
            Model result = modifiedQuery.Execute();
            return mapExpr.Compile()(result);
        }

        public static ICollection<Target> SelectAndExecute<Model, Target>(this IQuery<ODataFeed<Model>> modelQuery, Expression<Func<Model, Target>> mapExpr) where Model : ODataObject
        {
            IQuery<ODataFeed<Model>> modifiedQuery = ApplySelectsAndExpands(modelQuery, mapExpr);
            ODataFeed<Model> result = modifiedQuery.Execute();
            return result.Feed.Select(mapExpr.Compile()).ToList();
        }
		
        public static async Task<Target> SelectAndExecute<Model, Target>(this IQuery<Model> modelQuery, Expression<Func<Model, Target>> mapExpr, CancellationToken cancelToken = default(CancellationToken)) where Model : ODataObject
        {
            IQuery<Model> modifiedQuery = ApplySelectsAndExpands(modelQuery, mapExpr);
            Model result = await modifiedQuery.ExecuteAsync(cancelToken).ConfigureAwait(false);
            return mapExpr.Compile()(result);
        }

        public static async Task<ICollection<Target>> SelectAndExecute<Model, Target>(this IQuery<ODataFeed<Model>> modelQuery, Expression<Func<Model, Target>> mapExpr, CancellationToken cancelToken = default(CancellationToken)) where Model : ODataObject
        {
            IQuery<ODataFeed<Model>> modifiedQuery = ApplySelectsAndExpands(modelQuery, mapExpr);
            ODataFeed<Model> result = await modifiedQuery.ExecuteAsync(cancelToken).ConfigureAwait(false);
            return result.Feed.Select(mapExpr.Compile()).ToList();
        }
        #endregion

        public static IQuery<T> ApplySelectsAndExpands<T>(IQuery<T> query, LambdaExpression lambda)
            where T : class
        {
            var queryModifiers = ExpressionUtils.ExpandLambdaExpression(lambda).SelectMany(z => ExpressionUtils.ParseToQuery(lambda.Parameters[0], z)).ToList();

            IQuery<T> modifiedQuery = query;

            IEnumerable<string> selects = queryModifiers.Where(mod => mod.ModType == QueryModifierType.Select).Select(mod => mod.Property).Distinct().OrderBy(s => s.Length);
            foreach (string select in selects)
            {
                modifiedQuery = modifiedQuery.Select(select);
            }

            IEnumerable<string> expands = queryModifiers.Where(mod => mod.ModType == QueryModifierType.Expand).Select(mod => mod.Property).Distinct().OrderBy(s => s.Length);
            foreach (string expand in expands)
            {
                modifiedQuery = modifiedQuery.Expand(expand);
            }

            return modifiedQuery;
        }

        enum QueryModifierType { Expand, Select }
        private class QueryModifier
        {
            public QueryModifierType ModType { get; set; }
            public string Property { get; set; }

            public override string ToString()
            {
                return String.Format("{0}: {1}", Enum.GetName(typeof(QueryModifierType), ModType), Property);
            }
        }

        #region collection class
        private class ImmutableList<T> : IEnumerable<T>
        {
            public T Value { get; set; }
            public ImmutableList<T> Next { get; set; }

            public IEnumerator<T> GetEnumerator()
            {
                return new ImmutableListEnumerator { List = new ImmutableList<T> { Next = this } };
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private class ImmutableListEnumerator : IEnumerator<T>
            {
                public ImmutableList<T> List { get; set; }

                public T Current
                {
                    get { return List.Value; }
                }

                public void Dispose()
                {
                    return;
                }

                object System.Collections.IEnumerator.Current
                {
                    get { return Current; }
                }

                public bool MoveNext()
                {
                    if (List.Next != null)
                    {
                        List = List.Next;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                public void Reset()
                {
                    throw new NotImplementedException();
                }
            }
        }
        #endregion

        #region expression parsing
        private static class ExpressionUtils
        {
            public static IEnumerable<QueryModifier> ParseToQuery(ParameterExpression baseParam, IEnumerable<Expression> propertyChain)
            {
                //paramexpr.a.b.c
                string prefix = "";
                foreach (var propExpr in propertyChain)
                {
                    if (propExpr.NodeType == ExpressionType.MemberAccess)
                    {
                        MemberExpression m = (MemberExpression)propExpr;
                        PropertyInfo prop = (PropertyInfo)m.Member;

                        Type propInnerType = prop.PropertyType;
                        if (prop.PropertyType.IsGenericTypeOf(typeof(IEnumerable<>)))
                        {
                            if (prop.PropertyType == typeof(string))
                            {
                                //don't treat strings as collections
                            }
                            else if (prop.PropertyType.IsArray)
                            {
                                propInnerType = prop.PropertyType.GetElementType();
                            }
                            else
                            {
                                propInnerType = prop.PropertyType.GetGenericArguments()[0];
                            }
                        }

                        if (typeof(ODataObject).IsAssignableFrom(propInnerType))
                        {
                            yield return new QueryModifier { ModType = QueryModifierType.Expand, Property = prefix + prop.Name };
                            prefix += prop.Name + "/";
                        }
                        else
                        {
                            yield return new QueryModifier { ModType = QueryModifierType.Select, Property = prefix + prop.Name };
                            break;
                        }
                    }
                    else if (propExpr.NodeType == ExpressionType.Convert
                        || propExpr.NodeType == ExpressionType.ConvertChecked
                        || propExpr.NodeType == ExpressionType.TypeAs
                        || propExpr.NodeType == ExpressionType.Coalesce)
                    {
                        //assume you know what you're doing..
                        continue;
                    }
                    else if (propExpr.NodeType == ExpressionType.Call)
                    {
                        MethodCallExpression call = (MethodCallExpression)propExpr;
                        bool hasLambda = false;
                        foreach (Expression arg in call.Arguments)
                        {
                            if (arg.NodeType != ExpressionType.Lambda)
                                continue;

                            LambdaExpression lambda = (LambdaExpression)arg;
                            //confirm that an arg to the lambda matches property type on the odataobj before this?
                            ParameterExpression lambdaBaseParam = lambda.Parameters.FirstOrDefault(param => typeof(ODataObject).IsAssignableFrom(param.Type));
                            if (lambdaBaseParam != null)
                            {
                                hasLambda = true;
                                foreach (var subModifier in ExpandLambdaExpression(lambda).SelectMany(z => ParseToQuery(lambdaBaseParam, z)))
                                {
                                    yield return new QueryModifier { ModType = subModifier.ModType, Property = prefix + subModifier.Property };
                                }
                            }
                        }
                        if (!hasLambda)
                        {
                            break;
                        }
                    }
                    else if (propExpr.NodeType == ExpressionType.Parameter && ((ParameterExpression)propExpr) == baseParam)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                yield break;
            }

            public static IEnumerable<ImmutableList<Expression>> ExpandLambdaExpression(LambdaExpression lambda)
            {
                return ExpandExpressionTree(lambda.Body);
            }

            private static IEnumerable<ImmutableList<Expression>> ExpandExpressionTree(Expression expr, ImmutableList<Expression> parents = null)
            {
                if (expr is ParameterExpression)
                {
                    yield return new ImmutableList<Expression> { Value = expr, Next = parents };
                }
                else
                {
                    foreach (var child in Expand(expr))
                    {
                        foreach (var result in ExpandExpressionTree(child, new ImmutableList<Expression> { Value = expr, Next = parents }))
                        {
                            yield return result;
                        }
                    }
                }
            }

            #region expand
            private static IEnumerable<Expression> Expand(Expression expr)
            {
                switch (expr.NodeType)
                {
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Not:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.ArrayLength:
                    case ExpressionType.Quote:
                    case ExpressionType.TypeAs:
                        return ExpandUnary((UnaryExpression)expr);
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Divide:
                    case ExpressionType.Modulo:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Coalesce:
                    case ExpressionType.ArrayIndex:
                    case ExpressionType.RightShift:
                    case ExpressionType.LeftShift:
                    case ExpressionType.ExclusiveOr:
                        return ExpandBinary((BinaryExpression)expr);
                    case ExpressionType.TypeIs:
                        return ExpandTypeIs((TypeBinaryExpression)expr);
                    case ExpressionType.Conditional:
                        return ExpandConditional((ConditionalExpression)expr);
                    case ExpressionType.Constant:
                        return ExpandConstant((ConstantExpression)expr);
                    case ExpressionType.Parameter:
                        return ExpandParameter((ParameterExpression)expr);
                    case ExpressionType.MemberAccess:
                        return ExpandMemberAccess((MemberExpression)expr);
                    case ExpressionType.Call:
                        return ExpandMethodCall((MethodCallExpression)expr);
                    case ExpressionType.Lambda:
                        return ExpandLambda((LambdaExpression)expr);
                    case ExpressionType.New:
                        return ExpandNew((NewExpression)expr);
                    case ExpressionType.NewArrayInit:
                    case ExpressionType.NewArrayBounds:
                        return ExpandNewArray((NewArrayExpression)expr);
                    case ExpressionType.Invoke:
                        return ExpandInvocation((InvocationExpression)expr);
                    case ExpressionType.MemberInit:
                        return ExpandMemberInit((MemberInitExpression)expr);
                    case ExpressionType.ListInit:
                        return ExpandListInit((ListInitExpression)expr);
                    default:
                        throw new Exception(string.Format("Unhandled expression type: '{0}'", expr.NodeType));
                }
            }

            private static IEnumerable<Expression> ExpandListInit(ListInitExpression listInitExpression)
            {
                yield return listInitExpression.NewExpression;
                foreach (var initArg in ExpandCollectionInitializers(listInitExpression.Initializers))
                {
                    yield return initArg;
                }
            }

            private static IEnumerable<Expression> ExpandCollectionInitializers(IEnumerable<ElementInit> initializers)
            {
                foreach (var init in initializers)
                {
                    foreach (var initArg in init.Arguments)
                    {
                        yield return initArg;
                    }
                }
            }

            private static IEnumerable<Expression> ExpandMemberBindings(IEnumerable<MemberBinding> bindings)
            {
                foreach (var binding in bindings)
                {
                    switch (binding.BindingType)
                    {
                        case MemberBindingType.Assignment:
                            yield return ((MemberAssignment)binding).Expression;
                            break;
                        case MemberBindingType.ListBinding:
                            foreach (var initArg in ExpandCollectionInitializers(((MemberListBinding)binding).Initializers))
                            {
                                yield return initArg;
                            }
                            break;
                        case MemberBindingType.MemberBinding:
                            foreach(var initArg in ExpandMemberBindings(((MemberMemberBinding)binding).Bindings))
                            {
                                yield return initArg;
                                //yield return balrog;
                            }
                            break;
                    }
                }
            }

            private static IEnumerable<Expression> ExpandMemberInit(MemberInitExpression memberInitExpression)
            {
                yield return memberInitExpression.NewExpression;
                foreach(var initArg in ExpandMemberBindings(memberInitExpression.Bindings))
                {
                    yield return initArg;
                }
            }

            private static IEnumerable<Expression> ExpandInvocation(InvocationExpression invocationExpression)
            {
                yield return invocationExpression.Expression;
                foreach (var arg in invocationExpression.Arguments)
                {
                    yield return arg;
                }
            }

            private static IEnumerable<Expression> ExpandNewArray(NewArrayExpression newArrayExpression)
            {
                foreach (var arg in newArrayExpression.Expressions)
                {
                    yield return arg;
                }
            }

            private static IEnumerable<Expression> ExpandNew(NewExpression newExpression)
            {
                foreach (var arg in newExpression.Arguments)
                {
                    yield return arg;
                }
            }

            private static IEnumerable<Expression> ExpandLambda(LambdaExpression lambdaExpression)
            {
                foreach (var param in lambdaExpression.Parameters)
                {
                    yield return param;
                }
                yield return lambdaExpression.Body;
            }

            private static IEnumerable<Expression> ExpandMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Object != null)
                    yield return methodCallExpression.Object;
                foreach (var arg in methodCallExpression.Arguments)
                {
                    yield return arg;
                }
            }

            private static IEnumerable<Expression> ExpandMemberAccess(MemberExpression memberExpression)
            {
                if (memberExpression.Expression != null)
                    yield return memberExpression.Expression;
            }

            private static IEnumerable<Expression> ExpandParameter(ParameterExpression parameterExpression)
            {
                yield break;
            }

            private static IEnumerable<Expression> ExpandConstant(ConstantExpression constantExpression)
            {
                yield break;
            }

            private static IEnumerable<Expression> ExpandConditional(ConditionalExpression conditionalExpression)
            {
                yield return conditionalExpression.Test;
                yield return conditionalExpression.IfTrue;
                yield return conditionalExpression.IfFalse;
            }

            private static IEnumerable<Expression> ExpandTypeIs(TypeBinaryExpression typeBinaryExpression)
            {
                yield return typeBinaryExpression.Expression;
            }

            private static IEnumerable<Expression> ExpandBinary(BinaryExpression binaryExpression)
            {
                yield return binaryExpression.Left;
                yield return binaryExpression.Right;
            }

            private static IEnumerable<Expression> ExpandUnary(UnaryExpression unaryExpression)
            {
                yield return unaryExpression.Operand;
            }

            #endregion
        }
        #endregion
    }
}
