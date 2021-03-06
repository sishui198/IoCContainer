﻿namespace IoC.Features.Interception
{
    using System;
    using Castle.DynamicProxy;

    internal interface IInterceptorRegistry
    {
        [NotNull]
        IDisposable Register([NotNull] Predicate<Key> filter, [NotNull] [ItemNotNull] params IInterceptor[] interceptors);
    }
}