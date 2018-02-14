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
        // $group=08
        // $priority=00
        // $description=Custom Child Container
        // {
        public void Run()
        {
            // Create the container
            using (var container = Container.Create())
            // Configure current container to use a custom container's class to create a child container
            using (container.Bind<IContainer>().Tag(ContainerReference.Child).To<MyContainer>())
            // Create our child container
            using (var childContainer = container.CreateChild("abc"))
            // Configure the child container
            using (childContainer.Bind<IDependency>().To<Dependency>())
            using (childContainer.Bind<IService>().To<Service>())
            {
                // Resolve the instance
                var instance = childContainer.Get<IService>();

                childContainer.ShouldBeOfType<MyContainer>();
                instance.ShouldBeOfType<Service>();
            }
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public class MyContainer: IContainer
        {
            public MyContainer(IContainer currentContainer)
            {
                Parent = currentContainer;
            }

            public IContainer Parent { get; }

            public bool TryRegister(IEnumerable<Key> keys, IoC.IDependency dependency, ILifetime lifetime, out IDisposable registrationToken)
            {
                return Parent.TryRegister(keys, dependency, lifetime, out registrationToken);
            }

            public bool TryGetDependency(Key key, out IoC.IDependency dependency, out ILifetime lifetime)
            {
                return Parent.TryGetDependency(key, out dependency, out lifetime);
            }

            public Resolver<T> GetResolver<T>(Type type)
            {
                return Parent.GetResolver<T>(type);
            }

            public bool TryGetResolver<T>(Type type, out Resolver<T> resolver)
            {
                return Parent.TryGetResolver(type, out resolver);
            }

            public Resolver<T> GetResolver<T>(Type type, object tag)
            {
                return Parent.GetResolver<T>(type, tag);
            }

            public bool TryGetResolver<T>(Type type, object tag, out Resolver<T> resolver)
            {
                return Parent.TryGetResolver<T>(type, tag, out resolver);
            }

            public bool TryGetResolver<T>(IContainer container, Type type, object tag, out Resolver<T> resolver)
            {
                return Parent.TryGetResolver(container, type, tag, out resolver);
            }

            public void Dispose() { }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<Key> GetEnumerator()
            {
                return Parent.GetEnumerator();
            }

            public IDisposable Subscribe(IObserver<ContainerEvent> observer)
            {
                return Parent.Subscribe(observer);
            }
        }
        // }
    }
}
