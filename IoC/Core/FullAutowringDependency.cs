﻿namespace IoC.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensibility;

    internal class FullAutowringDependency: IDependency
    {
        [NotNull] private static readonly TypeDescriptor GenericContextTypeDescriptor = typeof(Context<>).Descriptor();
        [NotNull] private static readonly MethodInfo InjectMethodInfo;
        [NotNull] private static Cache<ConstructorInfo, NewExpression> _constructors = new Cache<ConstructorInfo, NewExpression>();
        [NotNull] private static Cache<Type, Expression> _this = new Cache<Type, Expression>();
        [NotNull] private static Cache<Type, MethodCallExpression> _injections = new Cache<Type, MethodCallExpression>();
        [NotNull] private readonly AutowringDependency _autowringDependency;

        static FullAutowringDependency()
        {
            Expression<Func<object>> injectExpression = () => default(IContainer).Inject<object>();
            InjectMethodInfo = ((MethodCallExpression)injectExpression.Body).Method.GetGenericMethodDefinition();
        }

        public FullAutowringDependency([NotNull] IContainer container, [NotNull] Type type, [CanBeNull] IAutowiringStrategy autowiringStrategy = null)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (autowiringStrategy == null)
            {
                autowiringStrategy = container.TryGetResolver<IAutowiringStrategy>(typeof(IAutowiringStrategy), out var contextResolver)
                    ? contextResolver(container)
                    : new DefaultAutowiringStrategy(container);
            }

            var typeDescriptor = type.Descriptor().ToDefinedGenericType().Descriptor();
            var ctor = autowiringStrategy.SelectConstructor(GetDefaultConstructors(typeDescriptor).Select(i => new Method<ConstructorInfo>(i)));
            var methods = autowiringStrategy.GetInitializers(GetDefaultMethods(typeDescriptor).Select(i => new Method<MethodInfo>(i)));

            var newExpression = _constructors.GetOrCreate(ctor.Info, () => Expression.New(ctor.Info, GetParameters(ctor)));
            var thisExpression = _this.GetOrCreate(typeDescriptor.AsType(), () =>
            {
                var contextType = GenericContextTypeDescriptor.MakeGenericType(typeDescriptor.AsType());
                var itFieldInfo = contextType.Descriptor().GetDeclaredFields().Single(i => i.Name == nameof(Context<object>.It));
                return Expression.Field(Expression.Parameter(contextType, "context"), itFieldInfo);
            });

            var methodCallExpressions = (
                from method in methods
                select (Expression)Expression.Call(thisExpression, method.Info, GetParameters(method))).ToArray();

            _autowringDependency = new AutowringDependency(newExpression, methodCallExpressions);
        }

        public bool TryBuildExpression(IBuildContext buildContext, ILifetime lifetime, out Expression baseExpression)
        {
            if (buildContext == null) throw new ArgumentNullException(nameof(buildContext));
            return _autowringDependency.TryBuildExpression(buildContext, lifetime, out baseExpression);
        }

        [NotNull]
        private static IEnumerable<ConstructorInfo> GetDefaultConstructors([NotNull] TypeDescriptor typeDescriptor)
        {
            return typeDescriptor.GetDeclaredConstructors()
                .Where(MethodFilter)
                .OrderBy(ctor => ctor.GetParameters().Length);
        }

        [NotNull]
        private static IEnumerable<MethodInfo> GetDefaultMethods([NotNull] TypeDescriptor typeDescriptor)
        {
            return typeDescriptor.GetDeclaredMethods()
                .Where(MethodFilter)
                .OrderBy(methodInfo => methodInfo.GetParameters().Length);
        }

        private static bool MethodFilter<T>([NotNull] T method)
            where T: MethodBase
        {
            return !method.IsStatic && (method.IsAssembly || method.IsPublic);
        }

        [NotNull]
        private static IEnumerable<Expression> GetParameters<TMethodInfo>(IMethod<TMethodInfo> method)
            where TMethodInfo: MethodBase
        {
            for (var position = 0; position < method.Info.GetParameters().Length; position++)
            {
                yield return method[position];
            }
        }

        private class Method<TMethodInfo>: IMethod<TMethodInfo>
            where TMethodInfo: MethodBase
        {
            private readonly Expression[] _parameters;

            public Method([NotNull] TMethodInfo info)
            {
                Info = info ?? throw new ArgumentNullException(nameof(info));
                var parameters = info.GetParameters();
                _parameters = new Expression[parameters.Length];
                for (var i = 0; i < _parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    var paramType = parameter.ParameterType;
                    _parameters[i] = _injections.GetOrCreate(paramType, () =>
                    {
                        var methodInfo = InjectMethodInfo.MakeGenericMethod(paramType);
                        var containerExpression = Expression.Field(Expression.Constant(null, typeof(Context)), nameof(Context.Container));
                        return Expression.Call(methodInfo, containerExpression);
                    });
                }
            }

            public TMethodInfo Info { get; }

            public Expression this[int position]
            {
                get => _parameters[position];
                set => _parameters[position] = value ?? throw new ArgumentNullException();
            }
        }
    }
}
