﻿namespace IoC.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class FullAutoWiringDependency: IDependency
    {
        private static readonly Type[] GenericTypeArguments =
        {
            TypeDescriptor<TT>.Type,
            TypeDescriptor<TT1>.Type,
            TypeDescriptor<TT2>.Type,
            TypeDescriptor<TT3>.Type,
            TypeDescriptor<TT4>.Type,
            TypeDescriptor<TT5>.Type,
            TypeDescriptor<TT6>.Type,
            TypeDescriptor<TT7>.Type,
            TypeDescriptor<TT8>.Type,
            TypeDescriptor<TT9>.Type,
            TypeDescriptor<TT10>.Type,
            TypeDescriptor<TT11>.Type,
            TypeDescriptor<TT12>.Type,
            TypeDescriptor<TT13>.Type,
            TypeDescriptor<TT14>.Type,
            TypeDescriptor<TT15>.Type
        };

        private static readonly TypeDescriptor GenericContextTypeDescriptor = typeof(Context<>).Descriptor();
        [NotNull] private readonly Type _type;
        [CanBeNull] private readonly IAutowiringStrategy _autoWiringStrategy;
        private readonly bool _hasGenericParamsWithConstraints;
        private readonly Dictionary<int, TypeDescriptor> _genericParamsWithConstraints = new Dictionary<int, TypeDescriptor>();
        private readonly Type[] _registeredGenericTypeParameters;
        private readonly TypeDescriptor _registeredTypeDescriptor;

        public FullAutoWiringDependency([NotNull] Type type, [CanBeNull] IAutowiringStrategy autoWiringStrategy = null)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _autoWiringStrategy = autoWiringStrategy;
            _registeredTypeDescriptor = type.Descriptor();
            if (_registeredTypeDescriptor.IsInterface())
            {
                throw new ArgumentException($"Type \"{type}\" should not be an interface.", nameof(type));
            }

            if (_registeredTypeDescriptor.IsAbstract())
            {
                throw new ArgumentException($"Type \"{type}\" should not be an abstract class.", nameof(type));
            }

            if (!_registeredTypeDescriptor.IsGenericTypeDefinition())
            {
                return;
            }

            _registeredGenericTypeParameters = _registeredTypeDescriptor.GetGenericTypeParameters();
            var genericTypePos = 0;
            var typesMap = new Dictionary<Type, Type>();
            for (var position = 0; position < _registeredGenericTypeParameters.Length; position++)
            {
                var genericType = _registeredGenericTypeParameters[position];
                if (!genericType.IsGenericParameter)
                {
                    continue;
                }

                var descriptor = genericType.Descriptor();
                if (!descriptor.GetGenericParameterConstraints().Any())
                {
                    if (!typesMap.TryGetValue(genericType, out var curType))
                    {
                        try
                        {
                            curType = GenericTypeArguments[genericTypePos++];
                            typesMap[genericType] = curType;
                        }
                        catch (IndexOutOfRangeException ex)
                        {
                            throw new BuildExpressionException("Too many generic arguments.", ex);
                        }
                    }

                    _registeredGenericTypeParameters[position] = curType;
                }
                else
                {
                    _genericParamsWithConstraints[position] = descriptor;
                }
            }

            if (_genericParamsWithConstraints.Count == 0)
            {
                _type = _registeredTypeDescriptor.MakeGenericType(_registeredGenericTypeParameters);
            }
            else
            {
                _hasGenericParamsWithConstraints = true;
            }
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public bool TryBuildExpression(IBuildContext buildContext, ILifetime lifetime, out Expression baseExpression, out Exception error)
        {
            if (buildContext == null) throw new ArgumentNullException(nameof(buildContext));
            try
            {
                var autoWiringStrategy = _autoWiringStrategy ?? buildContext.AutowiringStrategy;
                var isDefaultAutoWiringStrategy = DefaultAutowiringStrategy.Shared == autoWiringStrategy;
                if (!autoWiringStrategy.TryResolveType(_type, buildContext.Key.Type, out var instanceType))
                {
                    instanceType = _hasGenericParamsWithConstraints
                        ? GetInstanceTypeBasedOnTargetGenericConstrains(buildContext.Key.Type) ?? buildContext.Container.Resolve<IIssueResolver>().CannotResolveType(_type, buildContext.Key.Type)
                        : _type;
                }

                var typeDescriptor = instanceType.Descriptor();
                if (typeDescriptor.IsConstructedGenericType())
                {
                    buildContext.BindTypes(instanceType, buildContext.Key.Type);
                    var genericArgs = typeDescriptor.GetGenericTypeArguments();
                    var isReplaced = false;
                    for (var position = 0; position < genericArgs.Length; position++)
                    {
                        if (buildContext.TryReplaceType(genericArgs[position], out var type))
                        {
                            genericArgs[position] = type;
                            isReplaced = true;
                        }
                    }

                    if (isReplaced)
                    {
                        typeDescriptor = typeDescriptor.GetGenericTypeDefinition().MakeGenericType(genericArgs).Descriptor();
                    }
                }

                var defaultConstructors = CreateMethods(buildContext.Container, typeDescriptor.GetDeclaredConstructors());
                if (!autoWiringStrategy.TryResolveConstructor(defaultConstructors, out var ctor))
                {
                    if (isDefaultAutoWiringStrategy || !DefaultAutowiringStrategy.Shared.TryResolveConstructor(defaultConstructors, out ctor))
                    {
                        ctor = buildContext.Container.Resolve<IIssueResolver>().CannotResolveConstructor(defaultConstructors);
                    }
                }

                var defaultMethods = CreateMethods(buildContext.Container, typeDescriptor.GetDeclaredMethods());
                if (!autoWiringStrategy.TryResolveInitializers(defaultMethods, out var initializers))
                {
                    if (isDefaultAutoWiringStrategy || !DefaultAutowiringStrategy.Shared.TryResolveInitializers(defaultMethods, out initializers))
                    {
                        initializers = Enumerable.Empty<IMethod<MethodInfo>>();
                    }
                }

                baseExpression = Expression.New(ctor.Info, ctor.GetParametersExpressions(buildContext));
                var curInitializers = initializers.ToArray();
                if (curInitializers.Length > 0)
                {
                    var thisVar = Expression.Variable(baseExpression.Type, "this");
                    baseExpression = Expression.Block(
                        new[] {thisVar},
                        Expression.Assign(thisVar, baseExpression),
                        Expression.Block(
                            from initializer in initializers
                            select Expression.Call(thisVar, initializer.Info, initializer.GetParametersExpressions(buildContext))
                        ),
                        thisVar
                    );
                }

                if (!isDefaultAutoWiringStrategy)
                {
                    baseExpression = buildContext.InjectDependencies(baseExpression);
                }
                
                baseExpression = buildContext.AddLifetime(baseExpression, lifetime);
                error = default(Exception);
                return true;
            }
            catch (BuildExpressionException ex)
            {
                error = ex;
                baseExpression = default(Expression);
                return false;
            }
        }

        [CanBeNull]
        internal Type GetInstanceTypeBasedOnTargetGenericConstrains(Type targetType)
        {
            var registeredGenericTypeParameters = new Type[_registeredGenericTypeParameters.Length];
            Array.Copy(_registeredGenericTypeParameters, registeredGenericTypeParameters, _registeredGenericTypeParameters.Length);
            var resolvingTypeDescriptor = targetType.Descriptor();
            var resolvingTypeDefinitionDescriptor = resolvingTypeDescriptor.GetGenericTypeDefinition().Descriptor();
            var resolvingTypeDefinitionGenericTypeParameters = resolvingTypeDefinitionDescriptor.GetGenericTypeParameters();
            var constraintsMap = resolvingTypeDescriptor
                .GetGenericTypeArguments()
                .Zip(resolvingTypeDefinitionGenericTypeParameters, (type, typeDefinition) => Tuple.Create(type, typeDefinition.Descriptor().GetGenericParameterConstraints()))
                .ToArray();

            var canBeResolved = true;
            foreach (var item in _genericParamsWithConstraints)
            {
                var position = item.Key;
                var descriptor = item.Value;
                var constraints = descriptor.GetGenericParameterConstraints();

                var isDefined = false;
                foreach (var constraintsEntry in constraintsMap)
                {
                    if (!CoreExtensions.SequenceEqual(constraints, constraintsEntry.Item2))
                    {
                        continue;
                    }

                    registeredGenericTypeParameters[position] = constraintsEntry.Item1;
                    isDefined = true;
                    break;
                }

                if (!isDefined)
                {
                    canBeResolved = false;
                    break;
                }
            }

            return canBeResolved ? _registeredTypeDescriptor.MakeGenericType(registeredGenericTypeParameters) : null;
        }

        [NotNull]
        [MethodImpl((MethodImplOptions) 256)]
        private static IEnumerable<IMethod<TMethodInfo>> CreateMethods<TMethodInfo>(IContainer container, [NotNull] IEnumerable<TMethodInfo> methodInfos)
            where TMethodInfo: MethodBase
            => methodInfos
                .Where(method => !method.IsStatic && (method.IsAssembly || method.IsPublic))
                .Select(info => new Method<TMethodInfo>(info));
    }
}
