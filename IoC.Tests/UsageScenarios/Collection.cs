﻿namespace IoC.Tests.UsageScenarios
{
    using System.Collections.Generic;
    using Shouldly;
    using Xunit;

    public class Collection
    {
        [Fact]
        public void Run()
        {
            // $visible=true
            // $tag=injection
            // $priority=05
            // $description=Resolve all appropriate instances as ICollection
            // {
            // Create and configure the container
            using (var container = Container.Create())
            // Bind some dependency
            using (container.Bind<IDependency>().To<Dependency>())
            // Bind to the implementation #1
            using (container.Bind<IService>().Tag(1).To<Service>())
            // Bind to the implementation #2
            using (container.Bind<IService>().Tag(2).Tag("abc").To<Service>())
            // Bind to the implementation #3
            using (container.Bind<IService>().Tag(3).To<Service>())
            {
                // Resolve all appropriate instances
                var instances = container.Resolve<ICollection<IService>>();

                // Check the number of resolved instances
                instances.Count.ShouldBe(3);
                // }
                foreach (var instance in instances)
                {
                    // Check the instance's type
                    instance.ShouldBeOfType<Service>();
                }
                // {
            }
            // }
        }
    }
}
