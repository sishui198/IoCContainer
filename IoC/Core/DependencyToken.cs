﻿namespace IoC.Core
{
    using System;
    using System.Runtime.CompilerServices;

    internal struct DependencyToken: IDisposable
    {
        [NotNull] internal readonly IContainer Container;
        [NotNull] private readonly IDisposable _dependencyToken;

        public DependencyToken([NotNull] IContainer container, [NotNull] IDisposable dependencyToken)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            _dependencyToken = dependencyToken ?? throw new ArgumentNullException(nameof(dependencyToken));
        }

        [MethodImpl((MethodImplOptions)256)]
        public void Dispose() => _dependencyToken.Dispose();
    }
}
