﻿namespace IoC.Lifetimes
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Core;
    using Extensibility;
    using TypeExtensions = Core.TypeExtensions;

    /// <summary>
    /// Represents singleton lifetime.
    /// </summary>
    [PublicAPI]
    public sealed class SingletonLifetime : ILifetime, IDisposable, IExpressionBuilder<object>
    {
        [NotNull] private object _lockObject = new object();
        private volatile object _instance;

        /// <inheritdoc />
        [MethodImpl((MethodImplOptions)256)]
        public T GetOrCreate<T>(IContainer container, object[] args, Resolver<T> resolver)
        {
            if (_instance != null)
            {
                return (T) _instance;
            }

            return CreateInstance(container, args, resolver);
        }

        [MethodImpl((MethodImplOptions)256)]
        internal T CreateInstance<T>(IContainer container, object[] args, Resolver<T> resolver)
        {
            lock (_lockObject)
            {
                if (_instance == null)
                {
                    _instance = resolver(container, args);
                }
            }

            return (T)_instance;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <inheritdoc />
        public ILifetime Clone()
        {
            return new SingletonLifetime();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Lifetime.Singleton.ToString();
        }

        private static readonly MethodInfo CreateInstanceMethodInfo = TypeExtensions.Info<SingletonLifetime>().DeclaredMethods.Single(i => i.Name == nameof(CreateInstance));
        private static readonly Expression NullConst = Expression.Constant(null);
        private static readonly ITypeInfo ResolverTypeInfo = typeof(Resolver<>).Info();

        /// <inheritdoc />
        public Expression Build(Expression expression, Key key, IContainer container, object context)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (container == null) throw new ArgumentNullException(nameof(container));
            var instanceField = Expression.Field(Expression.Constant(this), nameof(_instance));
            var typedInstance = instanceField.Convert(expression.Type);
            var methodInfo = CreateInstanceMethodInfo.MakeGenericMethod(expression.Type);
            var resolverType = ResolverTypeInfo.MakeGenericType(expression.Type);
            var resolverExpression = Expression.Lambda(resolverType, expression, false, ExpressionExtensions.Parameters);
            var resolver = container.GetExpressionCompiler().Compile(resolverExpression);

            var lifetimeBody = Expression.Condition(
                Expression.NotEqual(instanceField, NullConst),
                typedInstance,
                Expression.Call(Expression.Constant(this), methodInfo, ExpressionExtensions.ContainerParameter, ExpressionExtensions.ArgsParameter, Expression.Constant(resolver)));

            return lifetimeBody;
        }
    }
}
