﻿namespace IoC.Features
{
    using System;
    using System.Collections.Generic;
    using Internal;

    public sealed  class FuncFeature : IConfiguration
    {
        public static readonly IConfiguration Shared = new FuncFeature();

        private FuncFeature()
        {
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return container
                .Bind(typeof(Func<>))
                .AnyTag()
                .To(CreateFunc);

            yield return container
                .Bind(typeof(Func<,>))
                .AnyTag()
                .To(CreateFunc);

            yield return container
                .Bind(typeof(Func<,,>))
                .AnyTag()
                .To(CreateFunc);

            yield return container
                .Bind(typeof(Func<,,,>))
                .AnyTag()
                .To(CreateFunc);

            yield return container
                .Bind(typeof(Func<,,,,>))
                .AnyTag()
                .To(CreateFunc);

            yield return container
                .Bind(typeof(Func<,,,,,>))
                .AnyTag()
                .To(CreateFunc);

            yield return container
                .Bind(typeof(Func<,,,,,,>))
                .AnyTag()
                .To(CreateFunc);

            yield return container
                .Bind(typeof(Func<,,,,,,,>))
                .AnyTag()
                .To(CreateFunc);

            yield return container
                .Bind(typeof(Func<,,,,,,,,>))
                .AnyTag()
                .To(CreateFunc);
        }

        private static object CreateFunc(Context context)
        {
            Type[] genericTypeArguments;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (context.IsConstructedGenericResolvingContractType)
            {
                genericTypeArguments = context.ResolvingKey.ContractType.GenericTypeArguments();
            }
            else
            {
                genericTypeArguments = context.ResolvingContainer.Get<IIssueResolver>().CannotGetGenericTypeArguments(context.ResolvingKey.ContractType);
            }

            Type instanceFuncType;
            switch (genericTypeArguments.Length)
            {
                case 1:
                    instanceFuncType = typeof(InstanceFunc<>);
                    break;

                case 2:
                    instanceFuncType = typeof(InstanceFunc<,>);
                    break;

                case 3:
                    instanceFuncType = typeof(InstanceFunc<,,>);
                    break;

                case 4:
                    instanceFuncType = typeof(InstanceFunc<,,,>);
                    break;

                case 5:
                    instanceFuncType = typeof(InstanceFunc<,,,,>);
                    break;

                case 6:
                    instanceFuncType = typeof(InstanceFunc<,,,,,>);
                    break;

                case 7:
                    instanceFuncType = typeof(InstanceFunc<,,,,,,>);
                    break;

                case 8:
                    instanceFuncType = typeof(InstanceFunc<,,,,,,,>);
                    break;

                case 9:
                    instanceFuncType = typeof(InstanceFunc<,,,,,,,,>);
                    break;

                default:
                    throw new NotSupportedException($"{genericTypeArguments.Length} is not supported count of arguments");
            }

            var instanceType = instanceFuncType.MakeGenericType(genericTypeArguments);
            var targetContractType = genericTypeArguments[genericTypeArguments.Length - 1];
            var resolvingKey = new Key(targetContractType, context.ResolvingKey.Tag);
            var newContext = new Context(context.RegistrationId, context.RegistrationKey, context.RegistrationContainer, resolvingKey, context.ResolvingContainer, context.Args, targetContractType.IsConstructedGenericType());
            return ((IFactory)Activator.CreateInstance(instanceType, newContext)).Create(newContext);
        }


        private class InstanceFunc<T> : IFactory
        {
            protected readonly IResolver Resolver;

            // ReSharper disable once MemberCanBeProtected.Local
            public InstanceFunc(Context context)
            {
                if (!context.ResolvingContainer.TryGetResolver(context.ResolvingKey, out Resolver))
                {
                    Resolver = context.ResolvingContainer.Get<IIssueResolver>().CannotGetResolver(context.ResolvingContainer, context.ResolvingKey);
                }
            }

            public virtual object Create(Context context)
            {
                return new Func<T>(() => (T) Resolver.Resolve(context.ResolvingKey, context.ResolvingContainer));
            }
        }

        private class InstanceFunc<T1, T> : InstanceFunc<T>
        {
            public InstanceFunc(Context context) : base(context) { }

            public override object Create(Context context)
            {
                return new Func<T1, T>(arg1 => (T) Resolver.Resolve(context.ResolvingKey, context.ResolvingContainer, 0, arg1));
            }
        }

        private class InstanceFunc<T1, T2, T> : InstanceFunc<T>
        {
            public InstanceFunc(Context context) : base(context) { }

            public override object Create(Context context)
            {
                return new Func<T1, T2, T>((arg1, arg2) => (T)Resolver.Resolve(context.ResolvingKey, context.ResolvingContainer, 0, arg1, arg2));
            }
        }

        private class InstanceFunc<T1, T2, T3, T> : InstanceFunc<T>
        {
            public InstanceFunc(Context context) : base(context) { }

            public override object Create(Context context)
            {
                return new Func<T1, T2, T3, T>((arg1, arg2, arg3) => (T) Resolver.Resolve(context.ResolvingKey, context.ResolvingContainer, 0, arg1, arg2, arg3));
            }
        }

        private class InstanceFunc<T1, T2, T3, T4, T> : InstanceFunc<T>
        {
            public InstanceFunc(Context context) : base(context) { }

            public override object Create(Context context)
            {
                return new Func<T1, T2, T3, T4, T>((arg1, arg2, arg3, arg4) => (T)Resolver.Resolve(context.ResolvingKey, context.ResolvingContainer, 0, arg1, arg2, arg3, arg4));
            }
        }

        private class InstanceFunc<T1, T2, T3, T4, T5, T> : InstanceFunc<T>
        {
            public InstanceFunc(Context context) : base(context) { }

            public override object Create(Context context)
            {
                return new Func<T1, T2, T3, T4, T5, T>((arg1, arg2, arg3, arg4, arg5) => (T)Resolver.Resolve(context.ResolvingKey, context.ResolvingContainer, 0, arg1, arg2, arg3, arg4, arg5));
            }
        }

        private class InstanceFunc<T1, T2, T3, T4, T5, T6, T> : InstanceFunc<T>
        {
            public InstanceFunc(Context context) : base(context) { }

            public override object Create(Context context)
            {
                return new Func<T1, T2, T3, T4, T5, T6, T>((arg1, arg2, arg3, arg4, arg5, arg6) => (T)Resolver.Resolve(context.ResolvingKey, context.ResolvingContainer, 0, arg1, arg2, arg3, arg4, arg5, arg6));
            }
        }

        private class InstanceFunc<T1, T2, T3, T4, T5, T6, T7, T> : InstanceFunc<T>
        {
            public InstanceFunc(Context context) : base(context) { }

            public override object Create(Context context)
            {
                return new Func<T1, T2, T3, T4, T5, T6, T7, T>((arg1, arg2, arg3, arg4, arg5, arg6, arg7) => (T)Resolver.Resolve(context.ResolvingKey, context.ResolvingContainer, 0, arg1, arg2, arg3, arg4, arg5, arg6, arg7));
            }
        }

        private class InstanceFunc<T1, T2, T3, T4, T5, T6, T7, T8, T> : InstanceFunc<T>
        {
            public InstanceFunc(Context context) : base(context) { }

            public override object Create(Context context)
            {
                return new Func<T1, T2, T3, T4, T5, T6, T7, T8, T>((arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) => (T)Resolver.Resolve(context.ResolvingKey, context.ResolvingContainer, 0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
            }
        }
    }
}
