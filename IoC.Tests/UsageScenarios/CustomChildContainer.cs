﻿namespace IoC.Tests.UsageScenarios
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Shouldly;
    using Xunit;

    public class CustomChildContainer
    {
        [Fact]
        // $visible=true
        // $tag=customization
        // $priority=00
        // $description=Custom Child Container
        // {
        public void Run()
        {
            // Create a root container
            using (var container = Container.Create())
            // Configure the root container to use a custom container as a child container
            using (container.Bind<IContainer>().Tag(WellknownContainers.NewChild).To<MyContainer>())
            // Create the custom child container
            using (var childContainer = container.CreateChild("abc"))
            // Configure our container
            using (childContainer.Bind<IDependency>().To<Dependency>())
            using (childContainer.Bind<IService>().To<Service>())
            {
                // Resolve an instance
                var instance = childContainer.Resolve<IService>();

                childContainer.ShouldBeOfType<MyContainer>();
                instance.ShouldBeOfType<Service>();
            }
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public class MyContainer: IContainer
        {
            public MyContainer(IContainer currentContainer) => Parent = currentContainer;

            public IContainer Parent { get; }

            public bool TryRegisterDependency(IEnumerable<Key> keys, IoC.IDependency dependency, ILifetime lifetime, out IDisposable dependencyToken) 
                => Parent.TryRegisterDependency(keys, dependency, lifetime, out dependencyToken);

            public bool TryGetDependency(Key key, out IoC.IDependency dependency, out ILifetime lifetime)
                => Parent.TryGetDependency(key, out dependency, out lifetime);

            public bool TryGetResolver<T>(Type type, object tag, out Resolver<T> resolver, out Exception error, IContainer resolvingContainer = null)
                => Parent.TryGetResolver(type, tag, out resolver, out error, resolvingContainer);

            public void RegisterResource(IDisposable resource) { }

            public void UnregisterResource(IDisposable resource) { }

            public void Dispose() { }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public IEnumerator<IEnumerable<Key>> GetEnumerator() => Parent.GetEnumerator();

            public IDisposable Subscribe(IObserver<ContainerEvent> observer) => Parent.Subscribe(observer);
        }
        // }
    }
}
