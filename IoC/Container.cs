﻿namespace IoC
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Core;
    using Core.Collections;
    using Extensibility;
    using Features;

    using FullKey = Key;
    using ShortKey = System.Type;

    /// <summary>
    /// The IoC container implementation.
    /// </summary>
    [PublicAPI]
    [DebuggerDisplay("Name = {" + nameof(ToString) + "()}")]
    public sealed class Container: IContainer, IResourceStore, IObserver<ContainerEvent>
    {
        private const string RootName = "root:/";
        private static long _containerId;

        [NotNull] private static readonly Lazy<Container> DefultRootContainer = new Lazy<Container>(() => CreateRootContainer(Feature.Defaults), true);
        [NotNull] private readonly object _lockObject = new object();
        [NotNull] private readonly IContainer _parent;
        [NotNull] private readonly string _name;
        [NotNull] private readonly Subject<ContainerEvent> _eventSubject = new Subject<ContainerEvent>();
        [NotNull] private readonly List<IDisposable> _resources = new List<IDisposable>();

        [NotNull] private volatile Map<FullKey, RegistrationEntry> _registrationEntries = Map<FullKey, RegistrationEntry>.Empty;
        [NotNull] private volatile Map<ShortKey, RegistrationEntry> _registrationEntriesForTagAny = Map<ShortKey, RegistrationEntry>.Empty;
        [NotNull] private volatile Map<FullKey, object> _resolvers = Map<FullKey, object>.Empty;
        [NotNull] private volatile Map<ShortKey, object> _resolversByType = Map<ShortKey, object>.Empty;

        private volatile IEnumerable<Key>[] _allKeys;
        private volatile bool _hasResolver;
        private bool _isFreezed;

        /// <summary>
        /// Creates a root container with default features.
        /// </summary>
        /// <param name="name">The optional name of the container.</param>
        /// <returns>The roor container.</returns>
        [NotNull]
        public static Container Create([NotNull] string name = "")
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return new Container(CreateContainerName(name), DefultRootContainer.Value, true);
        }

        /// <summary>
        /// Creates a root container with specified features.
        /// </summary>
        /// <param name="configurations">The set of features.</param>
        /// <returns>The roor container.</returns>
        [NotNull]
        public static Container Create([NotNull][ItemNotNull] params IConfiguration[] configurations)
        {
            if (configurations == null) throw new ArgumentNullException(nameof(configurations));
            return Create("", configurations);
        }

        /// <summary>
        /// Creates a root container with specified name and features.
        /// </summary>
        /// <param name="name">The optional name of the container.</param>
        /// <param name="configurations">The set of features.</param>
        /// <returns>The roor container.</returns>
        [NotNull]
        public static Container Create([NotNull] string name, [NotNull][ItemNotNull] params IConfiguration[] configurations)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (configurations == null) throw new ArgumentNullException(nameof(configurations));
            return new Container(CreateContainerName(name), CreateRootContainer(configurations), true);
        }

        private static Container CreateRootContainer([NotNull][ItemNotNull] IEnumerable<IConfiguration> configurations)
        {
            var container = new Container(RootName);
            container.ApplyConfigurations(configurations);
            return container;
        }

        private Container([NotNull] string name = "")
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _parent = new NullContainer();
        }

        internal Container([NotNull] string name, [NotNull] IContainer parent, bool root)
        {
            _name = $"{parent}/{name ?? throw new ArgumentNullException(nameof(name))}";
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));

            if (!root && _parent is IResourceStore resourceStore)
            {
                resourceStore.AddResource(this);
            }

            // Subscribe to events from the parent container
            ((IResourceStore)this).AddResource(_parent.Subscribe(_eventSubject));

            // Subscribe to reset resolvers
            ((IResourceStore)this).AddResource(_eventSubject.Subscribe(this));
        }

        /// <inheritdoc />
        public IContainer Parent => _parent;

        private IIssueResolver IssueResolver => GetResolver<IIssueResolver>(typeof(IIssueResolver))(this);

        /// <inheritdoc />
        public override string ToString()
        {
            return _name;
        }

        /// <inheritdoc />
        public bool TryRegister(IEnumerable<Key> keys, IDependency dependency, ILifetime lifetime, out IDisposable registrationToken)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (dependency == null) throw new ArgumentNullException(nameof(dependency));
            var isRegistered = true;
            var registeredKeys = new List<Key>();
            void UnregisterKeys()
            {
                lock (_lockObject)
                {
                    var registrationAnyEntries = _registrationEntriesForTagAny;
                    var registrationEntries = _registrationEntries;

                    foreach (var key in registeredKeys)
                    {
                        if (key.Tag == Key.AnyTag)
                        {
                            TryUnregister(key, key.Type, ref registrationAnyEntries);
                        }
                        else
                        {
                            TryUnregister(key, key, ref registrationEntries);
                        }
                    }

                    _registrationEntriesForTagAny = registrationAnyEntries;
                    _registrationEntries = registrationEntries;
                }
            }

            var registrationEntry = new RegistrationEntry(
                ResolverExpressionBuilder.Shared,
                dependency,
                lifetime,
                Disposable.Create(UnregisterKeys),
                registeredKeys);

            try
            {
                lock (_lockObject)
                {
                    var registrationAnyEntries = _registrationEntriesForTagAny;
                    var registrationEntries = _registrationEntries;

                    foreach (var key in keys)
                    {
                        if (key.Tag == Key.AnyTag)
                        {
                            isRegistered &= TryRegister(key, key.Type, registrationEntry, ref registrationAnyEntries);
                        }
                        else
                        {
                            isRegistered &= TryRegister(key, key, registrationEntry, ref registrationEntries);
                        }

                        if (isRegistered)
                        {
                            registeredKeys.Add(key);
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (isRegistered)
                    {
                        _registrationEntriesForTagAny = registrationAnyEntries;
                        _registrationEntries = registrationEntries;
                    }
                }
            }
            catch (Exception)
            {
                isRegistered = false;
                throw;
            }
            finally
            {
                if (isRegistered)
                {
                    registrationToken = registrationEntry;
                }
                else
                {
                    registrationEntry.Dispose();
                    registrationToken = default(IDisposable);
                }
            }

            return isRegistered;
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        public bool TryGetResolver<T>(Type type, object tag, out Resolver<T> resolver, IContainer container = null)
        {
            if (_resolversByType.TryGet(type.GetHashCode(), type, out var resolverValue))
            {
                resolver = (Resolver<T>)resolverValue;
                return true;
            }


            var key = new FullKey(type, tag);
            if (_resolvers.TryGet(key.GetHashCode(), key, out resolverValue))
            {
                resolver = (Resolver<T>)resolverValue;
                return true;
            }

            return TryCreateResolver(type, tag, out resolver, container ?? this);
        }

        /// <inheritdoc />
        [MethodImpl((MethodImplOptions)256)]
        [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        public bool TryGetResolver<T>(Type type, out Resolver<T> resolver, IContainer container = null)
        {
            if (_resolversByType.TryGet(type.GetHashCode(), type, out var resolverValue))
            {
                resolver = (Resolver<T>)resolverValue;
                return true;
            }

            return TryCreateResolver(type, null, out resolver, container ?? this);
        }

        /// <inheritdoc />
        [MethodImpl((MethodImplOptions)256)]
        public Resolver<T> GetResolver<T>(Type type, object tag, IContainer container = null)
        {
            if (!TryGetResolver<T>(type, tag, out var resolver, container))
            {
                return IssueResolver.CannotGetResolver<T>(this, new Key(type, tag));
            }

            return resolver;
        }

        /// <inheritdoc />
        [MethodImpl((MethodImplOptions)256)]
        public Resolver<T> GetResolver<T>(Type type, IContainer container = null)
        {
            if (!TryGetResolver<T>(type, out var resolver, container))
            {
                return IssueResolver.CannotGetResolver<T>(this, new Key(type));
            }

            return resolver;
        }

        [MethodImpl((MethodImplOptions)256)]
        private bool TryCreateResolver<T>(Type type, [CanBeNull] object tag, out Resolver<T> resolver, IContainer container)
        {
            if (TryGetRegistrationEntry(type, tag, out var registrationEntry))
            {
                if (!registrationEntry.TryCreateResolver(type, tag, container, out resolver))
                {
                    return false;
                }

                if (container == this)
                {
                    AddResolver(type, tag, resolver);
                }

                return true;
            }

            if (!_parent.TryGetResolver(type, tag, out resolver, container))
            {
                return false;
            }

            if (container == this)
            {
                AddResolver(type, tag, resolver);
            }

            return true;
        }

        [MethodImpl((MethodImplOptions)256)]
        private void AddResolver<T>(Type type, object tag, Resolver<T> resolver)
        {
            lock (_lockObject)
            {
                _hasResolver = true;
                var key = new FullKey(type, tag);
                _resolvers = _resolvers.Set(key.GetHashCode(), key, resolver);
                if (tag == null)
                {
                    _resolversByType = _resolversByType.Set(type.GetHashCode(), type, resolver);
                }
            }
        }

        /// <inheritdoc />
        public bool TryGetDependency(Key key, out IDependency dependency, out ILifetime lifetime)
        {
            if (TryGetRegistrationEntry(key.Type, key.Tag, out var registrationEntry))
            {
                dependency = registrationEntry.Dependency;
                lifetime = registrationEntry.GetLifetime(key.Type);
                return true;
            }

            return _parent.TryGetDependency(key, out dependency, out lifetime);
        }

        [MethodImpl((MethodImplOptions)256)]
        private bool TryGetRegistrationEntry(Type type, object tag, out RegistrationEntry registrationEntry)
        {
            lock (_lockObject)
            {
                var key = new FullKey(type, tag);
                if (_registrationEntries.TryGet(key.GetHashCode(), key, out registrationEntry))
                {
                    return true;
                }

                var typeInfo = type.Info();
                if (typeInfo.IsConstructedGenericType)
                {
                    var genericType = typeInfo.GetGenericTypeDefinition();
                    key = new FullKey(genericType, tag);
                    if (_registrationEntries.TryGet(key.GetHashCode(), key, out registrationEntry))
                    {
                        return true;
                    }

                    if (_registrationEntriesForTagAny.TryGet(genericType.GetHashCode(), genericType, out registrationEntry))
                    {
                        return true;
                    }
                }

                if (_registrationEntriesForTagAny.TryGet(type.GetHashCode(), type, out registrationEntry))
                {
                    return true;
                }

                return false;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_parent is IResourceStore resourceStore)
            {
                resourceStore.RemoveResource(this);
            }

            IDisposable resource;
            lock (_lockObject)
            {
                _registrationEntries = Map<FullKey, RegistrationEntry>.Empty;
                _registrationEntriesForTagAny = Map<ShortKey, RegistrationEntry>.Empty;
                _resolvers = Map<FullKey, object>.Empty;
                _resolversByType = Map<ShortKey, object>.Empty;
                resource = Disposable.Create(_resources);
                _resources.Clear();
            }

            resource.Dispose();
        }

        void IResourceStore.AddResource(IDisposable resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            lock (_lockObject)
            {
                _resources.Add(resource);
            }
        }

        void IResourceStore.RemoveResource(IDisposable resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            lock (_lockObject)
            {
                _resources.Remove(resource);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public IEnumerator<IEnumerable<Key>> GetEnumerator()
        {
            return GetAllKeys().Concat(_parent).GetEnumerator();
        }

        private IEnumerable<IEnumerable<Key>> GetAllKeys()
        {
            lock (_lockObject)
            {
                if (_allKeys == null)
                {
                    _allKeys = _registrationEntries.Select(i => i.Value).Distinct().Select(i => (IEnumerable<Key>)i.Keys).ToArray();
                }

                return _allKeys;
            }
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<ContainerEvent> observer)
        {
            return _eventSubject.Subscribe(observer);
        }

        private bool TryRegister<TKey>(Key originalKey, TKey key, [NotNull] RegistrationEntry registrationEntry, [NotNull] ref Map<TKey, RegistrationEntry> entries)
        {
            var hashCode = key.GetHashCode();
            var isRegistered = !entries.TryGet(hashCode, key, out var _);
            if (isRegistered)
            {
                entries = entries.Set(hashCode, key, registrationEntry);
                ResetResolvers();
                _allKeys = null;
            }

            if (!isRegistered)
            {
                return false;
            }

            _eventSubject.OnNext(new ContainerEvent(this, ContainerEvent.EventType.Registration, originalKey));
            return true;
        }

        private bool TryUnregister<TKey>(Key originalKey, TKey key, [NotNull] ref Map<TKey, RegistrationEntry> entries)
        {
            var hashCode = key.GetHashCode();
            var isUnregistered = entries.TryGet(hashCode, key, out var _);
            if (isUnregistered)
            {
                entries = entries.Remove(hashCode, key);
                ResetResolvers();
                _allKeys = null;
            }

            if (!isUnregistered)
            {
                return false;
            }

            _eventSubject.OnNext(new ContainerEvent(this, ContainerEvent.EventType.Unregistration, originalKey));
            return true;
        }

        private void ResetResolvers()
        {
            if (_isFreezed || !_hasResolver)
            {
                return;
            }

            _hasResolver = false;
            foreach (var registration in _registrationEntries.Select(i => i.Value))
            {
                registration.Reset();
            }

            _resolvers = Map<FullKey, object>.Empty;
            _resolversByType = Map<ShortKey, object>.Empty;
        }

        void IObserver<ContainerEvent>.OnNext(ContainerEvent value)
        {
            if (value.Container == this)
            {
                return;
            }

            lock (_lockObject)
            {
                ResetResolvers();
            }
        }

        void IObserver<ContainerEvent>.OnError(Exception error)
        {
        }

        void IObserver<ContainerEvent>.OnCompleted()
        {
        }

        [NotNull]
        internal static string CreateContainerName([CanBeNull] string name = "")
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return !string.IsNullOrWhiteSpace(name) ? name : Interlocked.Increment(ref _containerId).ToString(CultureInfo.InvariantCulture);
        }

        private void ApplyConfigurations(IEnumerable<IConfiguration> configurations)
        {
            try
            {
                _isFreezed = true;
                _resources.Add(this.Apply(configurations));
            }
            finally
            {
                _isFreezed = false;
            }
        }
    }
}
