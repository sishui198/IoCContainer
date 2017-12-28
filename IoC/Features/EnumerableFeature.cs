﻿namespace IoC.Features
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Internal;

    public sealed class EnumerableFeature : IConfiguration
    {
        public static readonly IConfiguration Shared = new EnumerableFeature();

        private EnumerableFeature()
        {
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return container
                .Bind(typeof(IEnumerable<>))
                .To(CreateEnumerable);
        }

        private static object CreateEnumerable(Context ctx)
        {
            var keys =
                from key in ctx.ResolvingContainer as IEnumerable<Key> ?? Enumerable.Empty<Key>()
                where key.ContractType == ctx.TargetContractType
                select key;

            Type[] genericTypeArguments;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (ctx.IsConstructedGenericTargetContractType)
            {
                genericTypeArguments = ctx.TargetContractType.GenericTypeArguments();
            }
            else
            {
                genericTypeArguments = ctx.ResolvingContainer.Get<IIssueResolver>().CannotGetGenericTypeArguments(ctx.TargetContractType);
            }

            var instanceType = typeof(InstanceEnumerable<>).MakeGenericType(genericTypeArguments);
            return Activator.CreateInstance(instanceType, keys, ctx);
        }

        private class InstanceEnumerable<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _instances;

            public InstanceEnumerable(IEnumerable<Key> key, Context context)
            {
                _instances = GetInstances(key, context);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _instances.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private static IEnumerable<T> GetInstances(IEnumerable<Key> keys, Context context)
            {
                foreach (var key in keys)
                {
                    if (!context.ResolvingContainer.TryGetResolver(key, out var resolver))
                    {
                        continue;
                    }

                    yield return (T)resolver.Resolve(context.ResolvingContainer, context.TargetContractType, 0, context.Args);
                }
            }
        }
    }
}
