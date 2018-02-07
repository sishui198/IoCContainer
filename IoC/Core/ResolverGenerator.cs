﻿namespace IoC.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;

    internal class ResolverGenerator: IResolverGenerator
    {
        public static readonly IResolverGenerator Shared = new ResolverGenerator();
        public static readonly ParameterExpression ContainerParameter = Expression.Parameter(typeof(IContainer), nameof(Context.Container));
        public static readonly ParameterExpression ArgsParameter = Expression.Parameter(typeof(object[]), nameof(Context.Args));
        public static readonly ParameterExpression[] Parameters = { ContainerParameter, ArgsParameter };

        public IResolverHolder<T> Generate<T>(Key key, IContainer container, IDependency dependency, ILifetime lifetime = null)
        {
            var typesMap = new Dictionary<Type, Type>();
            var dependencyExpression = ExpressionBuilder.Shared.PrepareExpression(dependency.Expression, key, typesMap);
            var expressionVisitor = new InjectingExpressionVisitor(key, container, null);
            var injectedExpression = expressionVisitor.Visit(dependencyExpression) ?? throw new InvalidOperationException();
            switch (dependency)
            {
                case Autowring autowring:
                    if (autowring.Statements.Length == 0)
                    {
                        break;
                    }

                    var vars = new Collection<ParameterExpression>();
                    var statements = CreateAutowringStatements(key, container, autowring, injectedExpression, vars, typesMap).ToList();
                    injectedExpression = Expression.Block(vars, statements);
                    break;
            }

            injectedExpression = ExpressionBuilder.Shared.Convert(injectedExpression, key.Type);
            injectedExpression = ExpressionBuilder.Shared.AddLifetime(injectedExpression, lifetime, key.Type, expressionVisitor);
            var resolverExpression = Expression.Lambda<Resolver<T>>(injectedExpression, true, Parameters);
            return new ResolverHolder<T>(resolverExpression.Compile());
        }

        private IEnumerable<Expression> CreateAutowringStatements(
            Key key,
            IContainer container,
            Autowring autowring,
            Expression newExpression,
            ICollection<ParameterExpression> vars,
            IDictionary<Type, Type> typesMap)
        {
            if (autowring.Statements.Length == 0)
            {
                throw new ArgumentException($"{nameof(autowring.Statements)} should not be empty.");
            }

            var instanceExpression = Expression.Variable(newExpression.Type);
            vars.Add(instanceExpression);
            yield return instanceExpression;
            yield return Expression.Assign(instanceExpression, newExpression);
            var expressionVisitor = new InjectingExpressionVisitor(key, container, instanceExpression);
            foreach (var statement in autowring.Statements)
            {
                var statementExpression = ExpressionBuilder.Shared.PrepareExpression(statement, key, typesMap);
                yield return expressionVisitor.Visit(statementExpression);
            }

            yield return instanceExpression;
        }
    }
}
