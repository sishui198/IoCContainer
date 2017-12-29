﻿namespace IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Moq;
    using Shouldly;
    using Xunit;

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public class ContainerTests
    {
        [Fact]
        public void ContainerShouldResolve()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedInstance = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).To(ctx => expectedInstance))
                {
                    // Then
                    var actualInstance = container.Get<IMyService>();
                    actualInstance.ShouldBe(expectedInstance);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveFunc()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedInstance = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).To(ctx => expectedInstance))
                {
                    // Then
                    var actualInstance = container.FuncGet<IMyService>();
                    actualInstance().ShouldBe(expectedInstance);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveChildContainer()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedInstance = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).To(ctx => expectedInstance))
                using (var childContainer = container.Tag(Scope.Child).Get<IContainer>())
                {
                    // Then
                    var actualInstance = childContainer.Get<IMyService>();
                    actualInstance.ShouldBe(expectedInstance);
                }
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData(Scope.Current)]
        public void ContainerShouldResolveCurrentContainer(Scope? scope)
        {
            // Given
            using (var container = Container.Create())
            {
                // When
                using (var curContainer = scope.HasValue ? container.Tag(scope.Value).Get<IContainer>() : container.Get<IContainer>())
                {
                    // Then
                    curContainer.ShouldBe(container);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveParentContainer()
        {
            // Given
            using (var container = Container.Create())
            {
                // When
                var curContainer = container.Tag(Scope.Parent).Get<IContainer>();

                // Then
                curContainer.ShouldBe(container.Parent);
            }
        }

        [Fact]
        public void ShouldRegisterCustomChildContainer()
        {
            // Given
            var childContainer = new Mock<IContainer>();
            using (var container = Container.Create("my"))
            {
                // When
                IContainer actualChildContainer;
                using (container.Bind<IContainer>().To(ctx => childContainer.Object))
                {
                    actualChildContainer = container.Get<IContainer>();
                }

                // Then
                actualChildContainer.ShouldBe(childContainer.Object);
            }
        }

        [Fact]
        public void ParentContainerShouldDisposeChildContainerWhenDispose()
        {
            // Given
            var expectedInstance = new Mock<IDisposable>();
            using (var container = Container.Create("root"))
            {
                var childContainer = container.CreateChild("child");
                IDisposable actualInstance;
                using (childContainer.Bind<IDisposable>().Lifetime(Lifetime.Container).To(ctx => expectedInstance.Object))
                {
                    // When
                    actualInstance = childContainer.Get<IDisposable>();
                }

                // Then
                actualInstance.ShouldBe(expectedInstance.Object);
            }

            expectedInstance.Verify(i => i.Dispose(), Times.Once);
        }

        [Fact]
        public void ContainerShouldResolveWhenSeveralContracts()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedInstance = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).To(ctx => expectedInstance))
                {
                    // Then
                    var actualInstance = container.Get<IMyService>();
                    var actualInstance1 = container.Get<IMyService>();
                    actualInstance.ShouldBe(expectedInstance);
                    actualInstance1.ShouldBe(expectedInstance);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenTransientLifetime()
        {
            // Given
            using (var container = Container.Create())
            {
                // When
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).To(ctx => Mock.Of<IMyService>()))
                {
                    // Then
                    var instance1 = container.Get<IMyService>();
                    var instance2 = container.Get<IMyService>();
                    instance1.ShouldNotBe(instance2);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenSingletonLifetime()
        {
            // Given
            using (var container = Container.Create())
            {
                // When
                using (container.Bind<IMyService>().Lifetime(Lifetime.Singletone).To(ctx => Mock.Of<IMyService>()))
                {
                    // Then
                    var instance1 = container.Get<IMyService>();
                    var instance2 = container.Get<IMyService>();
                    using (var childContainer = container.CreateChild())
                    {
                        // Then
                        var instance3 = childContainer.Get<IMyService>();
                        instance1.ShouldBe(instance2);
                        instance1.ShouldBe(instance3);
                    }
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenSingletonLifetimeAndSeveralContracts()
        {
            // Given
            using (var container = Container.Create())
            {
                // When
                using (container.Bind<IMyService, IMyService1>().Lifetime(Lifetime.Singletone).To(ctx => Mock.Of<IMyService>()))
                {
                    // Then
                    var instance1 = container.Get<IMyService>();
                    var instance2 = container.Get<IMyService1>();
                    using (var childContainer = container.CreateChild())
                    {
                        // Then
                        var instance3 = childContainer.Get<IMyService>();
                        var instance4 = childContainer.Get<IMyService1>();
                        instance1.ShouldBe(instance2);
                        instance1.ShouldBe(instance3);
                        instance1.ShouldBe(instance4);
                    }
                }
            }
        }

        [Fact]
        public void ContainerShouldDiposeWhenSingletonLifetime()
        {
            // Given
            using (var container = Container.Create())
            {
                // When
                using (container.Bind<IMyService>().Lifetime(Lifetime.Singletone).To(ctx => Mock.Of<IMyService>()))
                {
                    // Then
                    var instance1 = container.Get<IMyService>();
                    var instance2 = container.Get<IMyService>();
                    using (var childContainer = container.CreateChild())
                    {
                        // Then
                        var instance3 = childContainer.Get<IMyService>();
                        instance1.ShouldBe(instance2);
                        instance1.ShouldBe(instance3);
                    }
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenContainerLifetime()
        {
            // Given
            using (var container = Container.Create())
            {
                // When
                using (container.Bind<IMyService>().Lifetime(Lifetime.Container).To(ctx => Mock.Of<IMyService>()))
                {
                    // Then
                    var instance1 = container.Get<IMyService>();
                    var instance2 = container.Get<IMyService>();

                    // Then
                    instance1.ShouldBe(instance2);
                    IMyService instance3;
                    using (var childContainer1 = container.CreateChild())
                    {
                        instance3 = childContainer1.Get<IMyService>();
                        var instance4 = childContainer1.Get<IMyService>();
                        instance3.ShouldBe(instance4);
                        instance1.ShouldNotBe(instance3);
                    }

                    using (var childContainer2 = container.CreateChild())
                    {
                        // Then
                        var instance5 = childContainer2.Get<IMyService>();
                        var instance6 = childContainer2.Get<IMyService>();
                        instance5.ShouldBe(instance6);
                        instance1.ShouldNotBe(instance5);
                        instance3.ShouldNotBe(instance5);
                    }
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenResolveLifetime()
        {
            // Given
            using (var container = Container.Create())
            {
                // When
                using (container.Bind<IMyService>().To(
                    typeof(MyService),
                    Has.Arg<string>("name", 0),
                    Has.Method(
                        "Init",
                        Has.Arg("intiValue", 1))))
                using (container.Bind<IMyService1>().Lifetime(Lifetime.Resolve).To(ctx => Mock.Of<IMyService1>()))
                {
                    var instance1 = (MyService)container.Get<IMyService>("abc", "xyz");
                    var instance2 = (MyService)container.Get<IMyService>("klm", "qrs");

                    // Then
                    instance1.SomeRef.ShouldBe(instance1.SomeRef2);
                    instance1.SomeRef.ShouldBe(instance1.SomeRef3);
                    instance1.SomeRef.ShouldBe(instance2.SomeRef);

                    instance2.SomeRef.ShouldBe(instance2.SomeRef2);
                    instance2.SomeRef.ShouldBe(instance2.SomeRef3);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenContainerLifetimeAndSeveralContracts()
        {
            // Given
            using (var container = Container.Create())
            {
                // When
                using (container.Bind<IMyService, IMyService1>().Lifetime(Lifetime.Container).To(ctx => Mock.Of<IMyService>()))
                {
                    // Then
                    var instance1 = container.Get<IMyService>();
                    var instance2 = container.Get<IMyService1>();
                    using (var childContainer = container.CreateChild())
                    {
                        // Then
                        var instance3 = childContainer.Get<IMyService>();
                        var instance4 = childContainer.Get<IMyService1>();
                        instance1.ShouldBe(instance2);
                        instance1.ShouldNotBe(instance3);
                        instance3.ShouldBe(instance4);
                    }
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveFromChild()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedInstance = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).To(ctx => expectedInstance))
                {
                    using (var childContainer = container.CreateChild())
                    {
                        // Then
                        var actualInstance = childContainer.Get<IMyService>();
                        actualInstance.ShouldBe(expectedInstance);
                    }
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenHasTag()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedInstance = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).Tag("abc").Tag(10).To(ctx => expectedInstance))
                {
                    // Then
                    var actualInstance = container.Tag(10).Get<IMyService>();
                    actualInstance.ShouldBe(expectedInstance);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenHasState()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedRef = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService1>().Lifetime(Lifetime.Transient).To(ctx => expectedRef))
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).To(typeof(MyService), Has.Arg<string>("name", 0)))
                {
                    // Then
                    var actualInstance = container.Get<IMyService>("abc");

                    actualInstance.ShouldBeOfType<MyService>();
                    ((MyService)actualInstance).Name.ShouldBe("abc");
                    ((MyService)actualInstance).SomeRef.ShouldBe(expectedRef);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenHasStateUsingFunc()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedRef = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService1>().Lifetime(Lifetime.Transient).To(ctx => expectedRef))
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).To(typeof(MyService), Has.Arg<string>("name", 0)))
                {
                    // Then
                    var actualInstance = container.FuncGet<string, IMyService>()("abc");

                    actualInstance.ShouldBeOfType<MyService>();
                    ((MyService)actualInstance).Name.ShouldBe("abc");
                    ((MyService)actualInstance).SomeRef.ShouldBe(expectedRef);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenHasStateUsingFuncWithTag()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedRef = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService1>().Lifetime(Lifetime.Transient).To(ctx => expectedRef))
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).Tag(33).To(typeof(MyService), Has.Arg<string>("name", 0)))
                {
                    // Then
                    var actualInstance = container.Tag(33).FuncGet<string, IMyService>()("abc");

                    actualInstance.ShouldBeOfType<MyService>();
                    ((MyService)actualInstance).Name.ShouldBe("abc");
                    ((MyService)actualInstance).SomeRef.ShouldBe(expectedRef);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenHasStateInDependency()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedRef = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService1>().To(ctx => expectedRef))
                using (container.Bind<IMyService>().To(typeof(MyService), Has.Arg<string>("name", 0)))
                using (container.Bind(typeof(IMyGenericService<>)).To(typeof(MyGenericService<>), Has.Arg("value", 0), Has.Ref("service", Scope.Current, 1)))
                {
                    // Then
                    var actualInstance = container.Get<IMyGenericService<int>>(99, "abc");

                    actualInstance.ShouldBeOfType<MyGenericService<int>>();
                    actualInstance.Value.ShouldBe(99);
                    actualInstance.Service.ShouldBeOfType<MyService>();
                    ((MyService)actualInstance.Service).Name.ShouldBe("abc");
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenHasStateAndRefTag()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedRef = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService1>().Tag(33).Lifetime(Lifetime.Transient).To(ctx => expectedRef))
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).To(typeof(MyService), Has.Arg<string>("name", 0), Has.Ref<IMyService1>("someRef", 33)))
                {
                    // Then
                    var actualInstance = container.Get<IMyService>("abc");

                    actualInstance.ShouldBeOfType<MyService>();
                    ((MyService)actualInstance).Name.ShouldBe("abc");
                    ((MyService)actualInstance).SomeRef.ShouldBe(expectedRef);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenGeneric()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedInstance = Mock.Of<IMyGenericService<int, string>>();

                // When
                using (container.Bind(typeof(IMyGenericService<,>)).Lifetime(Lifetime.Transient).To(ctx => expectedInstance))
                {
                    // Then
                    var actualInstance = container.Get<IMyGenericService<int, string>>();
                    actualInstance.ShouldBe(expectedInstance);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenGenericAutowring()
        {
            // Given
            using (var container = Container.Create())
            {
                // When
                using (container.Bind(typeof(IMyGenericService<,>)).Lifetime(Lifetime.Transient).To(typeof(MyGenericService<,>)))
                {
                    // Then
                    var actualInstance = container.Get<IMyGenericService<int, string>>();
                    actualInstance.ShouldBeOfType<MyGenericService<int, string>>();
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveEnumerable()
        {
            // Given
            using (var container = Container.Create())
            {
                // When
                using (container.Bind<IMyService, IMyService1>().Lifetime(Lifetime.Singletone).Tag(1).To(ctx => Mock.Of<IMyService>()))
                using (container.Bind<IMyService1>().Tag("abc").To(ctx => Mock.Of<IMyService1>()))
                using (container.Bind<IMyService, IMyService1>().Lifetime(Lifetime.Transient).Tag("xyz").To(ctx => Mock.Of<IMyService>()))
                {
                    // Then
                    var actualInstances = container.Get<IEnumerable<IMyService1>>().ToList();
                    actualInstances.Count.ShouldBe(3);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenFunc()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedInstance = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).To(ctx => expectedInstance))
                {
                    // Then
                    var func = container.Get<Func<IMyService>>();
                    var actualInstance = func();
                    actualInstance.ShouldBe(expectedInstance);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenFuncWithArg()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedRef = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService1>().Lifetime(Lifetime.Transient).To(ctx => expectedRef))
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).To(typeof(MyService), Has.Arg<string>("name", 0)))
                {
                    // Then
                    var func = container.Get<Func<string, IMyService>>();
                    var actualInstance = func("abc");
                    actualInstance.ShouldBeOfType<MyService>();
                    ((MyService)actualInstance).Name.ShouldBe("abc");
                    ((MyService)actualInstance).SomeRef.ShouldBe(expectedRef);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenSingletonLifetimeWithGenerics()
        {
            // Given
            using (var container = Container.Create())
            {
                // When
                using (container.Bind(typeof(IMyGenericService1<>)).Lifetime(Lifetime.Singletone).To(ctx =>
                {
                    if(Equals(ctx.ResolvingKey, new Key(typeof(IMyGenericService1<string>))))
                    {
                        return Mock.Of<IMyGenericService1<string>>();
                    }

                    if (Equals(ctx.ResolvingKey, new Key(typeof(IMyGenericService1<int>))))
                    {
                        return Mock.Of<IMyGenericService1<int>>();
                    }

                    return null;
                }))
                {
                    // Then
                    var instance1 = container.Get<IMyGenericService1<int>>();
                    var instance2 = container.Get<IMyGenericService1<string>>();
                    var instance3 = container.Get<IMyGenericService1<int>>();
                    var instance4 = container.Get<IMyGenericService1<string>>();
                    instance1.ShouldBe(instance3);
                    instance2.ShouldBe(instance4);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveTask()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedInstance = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).To(ctx => expectedInstance))
                {
                    // Then
                    var task = container.Get<Task<IMyService>>();
                    task.Start();
                    var actualInstance = task.Result;
                    actualInstance.ShouldBe(expectedInstance);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenHasInitializerMethod()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedRef = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService1>().Lifetime(Lifetime.Transient).To(ctx => expectedRef))
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).To(
                    typeof(MyService),
                    Has.Arg<string>("name", 0),
                    Has.Method(
                        "Init",
                        Has.Arg<string>("intiValue", 1))))
                {
                    // Then
                    var actualInstance = container.Get<IMyService>("abc", "xyz");

                    actualInstance.ShouldBeOfType<MyService>();
                    ((MyService)actualInstance).Name.ShouldBe("xyz");
                    ((MyService)actualInstance).SomeRef.ShouldBe(expectedRef);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenHasInitializerMethodWithNotSpecifiedTypeOfArg()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedRef = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService1>().Lifetime(Lifetime.Transient).To(ctx => expectedRef))
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).To(
                    typeof(MyService),
                    Has.Arg<string>("name", 0),
                    Has.Method(
                        "Init",
                        Has.Arg("intiValue", 1))))
                {
                    // Then
                    var actualInstance = container.Get<IMyService>("abc", "xyz");

                    actualInstance.ShouldBeOfType<MyService>();
                    ((MyService)actualInstance).Name.ShouldBe("xyz");
                    ((MyService)actualInstance).SomeRef.ShouldBe(expectedRef);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenHasInitializerSetterWithNotSpecifiedTypeOfProperty()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedRef = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService1>().Lifetime(Lifetime.Transient).To(ctx => expectedRef))
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).To(
                    typeof(MyService),
                    Has.Arg("name", 0),
                    Has.Property("Name", 1)))
                {
                    // Then
                    var actualInstance = container.Get<IMyService>("abc", "xyz");

                    actualInstance.ShouldBeOfType<MyService>();
                    ((MyService)actualInstance).Name.ShouldBe("xyz");
                    ((MyService)actualInstance).SomeRef.ShouldBe(expectedRef);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveWhenHasInitializerSetter()
        {
            // Given
            using (var container = Container.Create())
            {
                var expectedRef = Mock.Of<IMyService>();

                // When
                using (container.Bind<IMyService1>().Lifetime(Lifetime.Transient).To(ctx => expectedRef))
                using (container.Bind<IMyService>().Lifetime(Lifetime.Transient).To(
                    typeof(MyService),
                    Has.Arg("name", 0),
                    Has.Property<string>("Name", 1)))
                {
                    // Then
                    var actualInstance = container.Get<IMyService>("abc", "xyz");

                    actualInstance.ShouldBeOfType<MyService>();
                    ((MyService)actualInstance).Name.ShouldBe("xyz");
                    ((MyService)actualInstance).SomeRef.ShouldBe(expectedRef);
                }
            }
        }

        [Fact]
        public void ContainerShouldResolveFromParentWhenParentScopeForRef()
        {
            // Given
            var expectedRef = Mock.Of<IMyService>();
            using (var container = Container.Create())
            using (container.Bind<IMyService1>().Lifetime(Lifetime.Transient).To(ctx => expectedRef))
            {
                var childRef = Mock.Of<IMyService>();

                // When
                using (var childContainer = container.CreateChild())
                using (childContainer.Bind<IMyService1>().Lifetime(Lifetime.Transient).To(ctx => childRef))
                using (childContainer.Bind<IMyService>().Lifetime(Lifetime.Transient).To(typeof(MyService), Has.Arg<string>("name", 0), Has.Ref("someRef", Scope.Parent)))
                {
                    // Then
                    var actualInstance = childContainer.Get<IMyService>("abc");
                    actualInstance.ShouldBeOfType<MyService>();
                    ((MyService)actualInstance).SomeRef.ShouldBe(expectedRef);
                }
            }
        }


        [Fact]
        public void ContainerShouldSupportWrapping()
        {
            // Given
            using (var container = Container.Create())
            using (container.Bind<IMyWrapper>().To(typeof(Wrappered)))
            {
                // When
                using (var childContainer = container.CreateChild())
                using (childContainer.Bind<IMyWrapper>().To(typeof(Wrapper), Has.Ref("wrapped", Scope.Parent)))
                {
                    // Then
                    var actualInstance = childContainer.Get<IMyWrapper>();
                    actualInstance.ShouldBeOfType<Wrapper>();
                    actualInstance.Wrapped.ShouldBeOfType<Wrappered>();
                }
            }
        }

        public interface IMyService: IMyService1
        {
        }

        public interface IMyService1
        {
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public class MyGenericService<T> : IMyGenericService<T>
        {
            public MyGenericService(T value, IMyService service)
            {
                Value = value;
                Service = service;
            }

            public T Value { get; }

            public IMyService Service { get; }
        }

        public interface IMyGenericService<out T>
        {
            T Value { get; }

            IMyService Service { get; }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public class MyService: IMyService
        {
            public MyService(string name, IMyService1 someRef)
            {
                Name = name;
                SomeRef = someRef;
            }

            public string Name { get; set; }

            public IMyService1 SomeRef { get; }

            public IMyService1 SomeRef2 { get; private set; }

            public IMyService1 SomeRef3 { get; private set; }

            public void Init(IMyService1 someRef2, IMyService1 someRef3, string intiValue)
            {
                Name = intiValue;
                SomeRef2 = someRef2;
                SomeRef3 = someRef3;
            }
        }

        public class MyGenericService<T1, T2> : IMyGenericService<T1, T2>
        {
        }

        public interface IMyGenericService<T1, T2> : IMyGenericService1<T1>
        {
        }

        public interface IMyGenericService1<T1>
        {
        }

        public interface IMyWrapper
        {
            IMyWrapper Wrapped { get; }
        }

        public class Wrappered: IMyWrapper
        {
            public IMyWrapper Wrapped => null;
        }

        public class Wrapper : IMyWrapper
        {
            public Wrapper(IMyWrapper wrapped)
            {
                Wrapped = wrapped;
            }

            public IMyWrapper Wrapped { get; }
        }
    }
}
