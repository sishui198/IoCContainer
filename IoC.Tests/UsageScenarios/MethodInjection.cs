﻿namespace IoC.Tests.UsageScenarios
{
    using System;
    using Shouldly;
    using Xunit;

    public class MethodInjection
    {
        [Fact]
        public void Run()
        {
            // $visible=true
            // $group=01
            // $priority=03
            // $description=Method Injection
            // {
            // Create the container
            using (var container = Container.Create())
            // Configure the container
            // Use full auto-wiring
            using (container.Bind<IDependency>().To<Dependency>())
            using (container.Bind<INamedService>().To<InitializingNamedService>(
                // Configure the constructor to use
                ctx => new InitializingNamedService(ctx.Container.Inject<IDependency>()),
                // Configure the method to initialize
                ctx => ctx.It.Initialize((string)ctx.Args[0], ctx.Container.Inject<IDependency>())))
            {
                // Resolve the instance "alpha"
                var instance = container.Get<INamedService>("alpha");

                instance.ShouldBeOfType<InitializingNamedService>();
                instance.Name.ShouldBe("alpha");

                // Resolve the instance "beta"
                var func = container.Get<Func<string, INamedService>>();
                var otherInstance = func("beta");
                otherInstance.Name.ShouldBe("beta");
            }
            // }
        }
    }
}
