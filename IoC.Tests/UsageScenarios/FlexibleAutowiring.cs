﻿namespace IoC.Tests.UsageScenarios
{
    using Shouldly;
    using Xunit;

    public class FlexibleAutowiring
    {
        [Fact]
        public void Run()
        {
            // $visible=true
            // $group=01
            // $priority=04
            // $description=Flexible Auto-wiring
            // {
            // Create a container
            using (var container = Container.Create())
            // Configure the container
            // Use full auto-wiring
            using (container.Bind<IDependency>().To<Dependency>())
            // Configure auto-wiring
            using (container.Bind<INamedService>().To<InitializingNamedService>(
                // Select the constructor and inject its parameters
                ctx => new InitializingNamedService(ctx.Container.Inject<IDependency>()),
                // Configure the method to invoke after the inctance's creation
                ctx => ctx.It.Initialize("some name", ctx.Container.Inject<IDependency>())))
            {
                // Resolve an instance
                var instance = container.Get<INamedService>();

                instance.ShouldBeOfType<InitializingNamedService>();
                instance.Name.ShouldBe("some name");
            }
            // }
        }
    }
}
