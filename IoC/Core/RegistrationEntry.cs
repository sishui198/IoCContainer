﻿namespace IoC.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Collections;
    using Extensibility;

    internal sealed class RegistrationEntry : IDisposable
    {
        [NotNull] internal readonly IDependency Dependency;
        [CanBeNull] private readonly ILifetime _lifetime;
        [NotNull] private readonly List<IDisposable> _resources = new List<IDisposable>();
        [NotNull] public readonly List<Key> Keys;
        private readonly object _lockObject = new object();
        private readonly Dictionary<LifetimeKey, ILifetime> _lifetimes = new Dictionary<LifetimeKey, ILifetime>();

        public RegistrationEntry(
            [NotNull] IDependency dependency,
            [CanBeNull] ILifetime lifetime,
            [NotNull] IDisposable resource,
            [NotNull] List<Key> keys)
        {
            Dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
            _lifetime = lifetime;
            _resources.Add(resource ?? throw new ArgumentNullException(nameof(resource)));
            Keys = keys ?? throw new ArgumentNullException(nameof(keys));
            if (lifetime is IDisposable disposableLifetime)
            {
                _resources.Add(disposableLifetime);
            }
        }

        public bool TryCreateResolver(Key key, [NotNull] IContainer container, out Delegate resolver)
        {
            var typeInfo = key.Type.Info();
            var compiler = container.GetExpressionCompiler();
            var buildContext = new BuildContext(compiler, key, container, _resources);
            if (!Dependency.TryBuildExpression(buildContext, GetLifetime(typeInfo), out var expression))
            {
                resolver = default(Delegate);
                return false;
            }

            var resolverExpression = Expression.Lambda(buildContext.Key.Type.ToResolverType(), expression, false, WellknownExpressions.ResolverParameters);
            resolver = compiler.Compile(resolverExpression);
            return true;
        }

        [CanBeNull]
        public ILifetime GetLifetime([NotNull] Type type)
        {
            return GetLifetime(type.Info());
        }

        [CanBeNull]
        private ILifetime GetLifetime(ITypeInfo typeInfo)
        {
            if (!typeInfo.IsConstructedGenericType)
            {
                return _lifetime;
            }

            var lifetimeKey = new LifetimeKey(typeInfo.GenericTypeArguments);
            ILifetime lifetime;
            lock (_lockObject)
            {
                if (!_lifetimes.TryGetValue(lifetimeKey, out lifetime))
                {
                    lifetime = _lifetime?.Clone();
                    if (lifetime is IDisposable disposableLifetime)
                    {
                        _resources.Add(disposableLifetime);
                    }

                    _lifetimes.Add(lifetimeKey, lifetime);
                }
            }

            return lifetime;
        }

        public void Dispose()
        {
            foreach (var resource in _resources)
            {
                resource.Dispose();
            }

            _resources.Clear();
        }

        public override string ToString()
            => $"{string.Join(", ", Keys.Select(i => i.ToString()))} as {_lifetime?.ToString() ?? Lifetime.Transient.ToString()}";

        private struct LifetimeKey
        {
            private readonly Type[] _genericTypes;

            public LifetimeKey(Type[] genericTypes) => _genericTypes = genericTypes;

            public override bool Equals(object obj) => obj is LifetimeKey key && Equals(key);

            public override int GetHashCode() => _genericTypes != null ? _genericTypes.GetHash() : 0;

            private bool Equals(LifetimeKey other) => Extensions.SequenceEqual(_genericTypes, other._genericTypes);
        }
    }
}
