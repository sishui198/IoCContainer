﻿// ReSharper disable UnusedVariable
namespace IoC.Tests.UsageScenarios
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Shouldly;
    using Xunit;

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class Generator
    {
        [Fact]
        // $visible=true
        // $tag=samples
        // $priority=00
        // $description=Generator
        // {
        public void Run()
        {
            Func<int, int, (int, int)> valueGetter = (sequential, random) => (sequential, random);

            // Create and configure the container using a configuration class 'Generators'
            using (var container = Container.Create().Using<Generators>())
            // Bind tuple of 2 integers to instance, constructing by different injected generators
            using (container.Bind<(int, int)>().To(
                // Use a function because of the expression trees have a limitation in syntax
                ctx => valueGetter(
                    // The first one is of sequential number generator
                    ctx.Container.Inject<int>(GeneratorType.Sequential),
                    // The second one is of random number generator
                    ctx.Container.Inject<int>(GeneratorType.Random))))
            {
                // Generate sequential numbers
                var sequential1 = container.Resolve<int>(GeneratorType.Sequential.AsTag());
                var sequential2 = container.Resolve<int>(GeneratorType.Sequential.AsTag());

                // Check numbers
                sequential2.ShouldBe(sequential1 + 1);

                // Generate a random number
                var random = container.Resolve<int>(GeneratorType.Random.AsTag());

                // Generate a tuple of numbers
                var setOfValues = container.Resolve<(int, int)>();

                // Check sequential numbers
                setOfValues.Item1.ShouldBe(sequential2 + 1);
            }
        }

        // Represents tags for generators
        public enum GeneratorType
        {
            Sequential, Random
        }

        // Represents IoC configuration
        public class Generators: IConfiguration
        {
            public IEnumerable<IDisposable> Apply(IContainer container)
            {
                var value = 0;
                // Define function to get sequential integer value
                Func<int> generator = () => Interlocked.Increment(ref value);
                // Bind to this function using the corresponding tag 'Sequential'
                yield return container.Bind<int>().Tag(GeneratorType.Sequential).To(ctx => generator());

                var random = new Random();
                // Define function to get random integer value
                Func<int> randomizer = () => random.Next();
                // Bind to this function using the corresponding tag 'Random'
                yield return container.Bind<int>().Tag(GeneratorType.Random).To(ctx => randomizer());
            }
        }
        // }
    }
}
