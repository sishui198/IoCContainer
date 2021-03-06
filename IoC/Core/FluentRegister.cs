﻿namespace IoC.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents extensions to register a dependency in the container.
    /// </summary>
    [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
    [PublicAPI]
    internal static class FluentRegister
    {
        private static readonly IEnumerable<object> DefaultTags = new object[] { null };

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The autowiring type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        [NotNull]
        public static IDisposable Register<T>([NotNull] this IContainer container, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null) 
            => container.Register(new[] { TypeDescriptor<T>.Type }, new FullAutoWiringDependency(TypeDescriptor<T>.Type), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The autowiring type.</typeparam>
        /// <typeparam name="T1">The additional contract type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        [NotNull]
        public static IDisposable Register<T, T1>([NotNull] this IContainer container, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null)
            where T : T1 
            => container.Register(new[] { TypeDescriptor<T1>.Type }, new FullAutoWiringDependency(TypeDescriptor<T>.Type), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The autowiring type.</typeparam>
        /// <typeparam name="T1">The additional contract type.</typeparam>
        /// <typeparam name="T2">The additional contract type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        [NotNull]
        public static IDisposable Register<T, T1, T2>([NotNull] this IContainer container, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null)
            where T : T1, T2 
            => container.Register(new[] { TypeDescriptor<T1>.Type, TypeDescriptor<T2>.Type }, new FullAutoWiringDependency(TypeDescriptor<T>.Type), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The autowiring type.</typeparam>
        /// <typeparam name="T1">The additional contract type.</typeparam>
        /// <typeparam name="T2">The additional contract type.</typeparam>
        /// <typeparam name="T3">The additional contract type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions) 256)]
        [NotNull]
        public static IDisposable Register<T, T1, T2, T3>([NotNull] this IContainer container, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null)
            where T : T1, T2, T3 
            => container.Register(new[] {TypeDescriptor<T1>.Type, TypeDescriptor<T2>.Type, TypeDescriptor<T3>.Type}, new FullAutoWiringDependency(TypeDescriptor<T>.Type), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The autowiring type.</typeparam>
        /// <typeparam name="T1">The additional contract type.</typeparam>
        /// <typeparam name="T2">The additional contract type.</typeparam>
        /// <typeparam name="T3">The additional contract type.</typeparam>
        /// <typeparam name="T4">The additional contract type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        [NotNull]
        public static IDisposable Register<T, T1, T2, T3, T4>([NotNull] this IContainer container, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null)
            where T : T1, T2, T3, T4
            => container.Register(new[] { TypeDescriptor<T1>.Type, TypeDescriptor<T2>.Type, TypeDescriptor<T3>.Type, TypeDescriptor<T4>.Type }, new FullAutoWiringDependency(TypeDescriptor<T>.Type), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The autowiring type.</typeparam>
        /// <typeparam name="T1">The additional contract type.</typeparam>
        /// <typeparam name="T2">The additional contract type.</typeparam>
        /// <typeparam name="T3">The additional contract type.</typeparam>
        /// <typeparam name="T4">The additional contract type.</typeparam>
        /// <typeparam name="T5">The additional contract type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        [NotNull]
        public static IDisposable Register<T, T1, T2, T3, T4, T5>([NotNull] this IContainer container, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null)
            where T : T1, T2, T3, T4, T5 
            => container.Register(new[] { TypeDescriptor<T1>.Type, TypeDescriptor<T2>.Type, TypeDescriptor<T3>.Type, TypeDescriptor<T4>.Type, TypeDescriptor<T5>.Type }, new FullAutoWiringDependency(TypeDescriptor<T>.Type), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The autowiring type.</typeparam>
        /// <typeparam name="T1">The additional contract type.</typeparam>
        /// <typeparam name="T2">The additional contract type.</typeparam>
        /// <typeparam name="T3">The additional contract type.</typeparam>
        /// <typeparam name="T4">The additional contract type.</typeparam>
        /// <typeparam name="T5">The additional contract type.</typeparam>
        /// <typeparam name="T6">The additional contract type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        public static IDisposable Register<T, T1, T2, T3, T4, T5, T6>([NotNull] this IContainer container, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null)
            where T : T1, T2, T3, T4, T5, T6
            => container.Register(new[] { TypeDescriptor<T1>.Type, TypeDescriptor<T2>.Type, TypeDescriptor<T3>.Type, TypeDescriptor<T4>.Type, TypeDescriptor<T5>.Type, TypeDescriptor<T6>.Type }, new FullAutoWiringDependency(TypeDescriptor<T>.Type), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The autowiring type.</typeparam>
        /// <typeparam name="T1">The additional contract type.</typeparam>
        /// <typeparam name="T2">The additional contract type.</typeparam>
        /// <typeparam name="T3">The additional contract type.</typeparam>
        /// <typeparam name="T4">The additional contract type.</typeparam>
        /// <typeparam name="T5">The additional contract type.</typeparam>
        /// <typeparam name="T6">The additional contract type.</typeparam>
        /// <typeparam name="T7">The additional contract type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        [NotNull]
        public static IDisposable Register<T, T1, T2, T3, T4, T5, T6, T7>([NotNull] this IContainer container, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null)
            where T : T1, T2, T3, T4, T5, T6, T7
            => container.Register(new[] { TypeDescriptor<T1>.Type, TypeDescriptor<T2>.Type, TypeDescriptor<T3>.Type, TypeDescriptor<T4>.Type, TypeDescriptor<T5>.Type, TypeDescriptor<T6>.Type, TypeDescriptor<T7>.Type }, new FullAutoWiringDependency(TypeDescriptor<T>.Type), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The autowiring type.</typeparam>
        /// <typeparam name="T1">The additional contract type.</typeparam>
        /// <typeparam name="T2">The additional contract type.</typeparam>
        /// <typeparam name="T3">The additional contract type.</typeparam>
        /// <typeparam name="T4">The additional contract type.</typeparam>
        /// <typeparam name="T5">The additional contract type.</typeparam>
        /// <typeparam name="T6">The additional contract type.</typeparam>
        /// <typeparam name="T7">The additional contract type.</typeparam>
        /// <typeparam name="T8">The additional contract type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        [NotNull]
        public static IDisposable Register<T, T1, T2, T3, T4, T5, T6, T7, T8>([NotNull] this IContainer container, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null)
            where T : T1, T2, T3, T4, T5, T6, T7, T8
            => container.Register(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) }, new FullAutoWiringDependency(typeof(T)), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="factory">The expression to create an instance.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="statements">The set of expressions to initialize an instance.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        [NotNull]
        public static IDisposable Register<T>([NotNull] this IContainer container, Expression<Func<Context, T>> factory, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null, [NotNull] [ItemNotNull] params Expression<Action<Context<T>>>[] statements)
            => container.Register(new[] { TypeDescriptor<T>.Type }, new AutoWiringDependency(factory, statements), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <typeparam name="T1">The additional contract type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="factory">The expression to create an instance.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="statements">The set of expressions to initialize an instance.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        [NotNull]
        public static IDisposable Register<T, T1>([NotNull] this IContainer container, Expression<Func<Context, T>> factory, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null, [NotNull] [ItemNotNull] params Expression<Action<Context<T>>>[] statements)
            where T : T1
            => container.Register(new[] { TypeDescriptor<T>.Type, TypeDescriptor<T1>.Type }, new AutoWiringDependency(factory, statements), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <typeparam name="T1">The additional contract type.</typeparam>
        /// <typeparam name="T2">The additional contract type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="factory">The expression to create an instance.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="statements">The set of expressions to initialize an instance.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        [NotNull]
        public static IDisposable Register<T, T1, T2>([NotNull] this IContainer container, Expression<Func<Context, T>> factory, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null, [NotNull] [ItemNotNull] params Expression<Action<Context<T>>>[] statements)
            where T : T1, T2
            => container.Register(new[] { TypeDescriptor<T>.Type, TypeDescriptor<T1>.Type, TypeDescriptor<T2>.Type }, new AutoWiringDependency(factory, statements), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <typeparam name="T1">The additional contract type.</typeparam>
        /// <typeparam name="T2">The additional contract type.</typeparam>
        /// <typeparam name="T3">The additional contract type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="factory">The expression to create an instance.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="statements">The set of expressions to initialize an instance.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        [NotNull]
        public static IDisposable Register<T, T1, T2, T3>([NotNull] this IContainer container, Expression<Func<Context, T>> factory, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null, [NotNull] [ItemNotNull] params Expression<Action<Context<T>>>[] statements)
            where T : T1, T2, T3
            => container.Register(new[] { TypeDescriptor<T>.Type, TypeDescriptor<T1>.Type, TypeDescriptor<T2>.Type, TypeDescriptor<T3>.Type }, new AutoWiringDependency(factory, statements), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <typeparam name="T1">The additional contract type.</typeparam>
        /// <typeparam name="T2">The additional contract type.</typeparam>
        /// <typeparam name="T3">The additional contract type.</typeparam>
        /// <typeparam name="T4">The additional contract type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="factory">The expression to create an instance.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="statements">The set of expressions to initialize an instance.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        [NotNull]
        public static IDisposable Register<T, T1, T2, T3, T4>([NotNull] this IContainer container, Expression<Func<Context, T>> factory, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null, [NotNull] [ItemNotNull] params Expression<Action<Context<T>>>[] statements)
            where T : T1, T2, T3, T4
            => container.Register(new[] { TypeDescriptor<T>.Type, TypeDescriptor<T1>.Type, TypeDescriptor<T2>.Type, TypeDescriptor<T3>.Type, TypeDescriptor<T4>.Type }, new AutoWiringDependency(factory, statements), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <typeparam name="T1">The additional contract type.</typeparam>
        /// <typeparam name="T2">The additional contract type.</typeparam>
        /// <typeparam name="T3">The additional contract type.</typeparam>
        /// <typeparam name="T4">The additional contract type.</typeparam>
        /// <typeparam name="T5">The additional contract type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="factory">The expression to create an instance.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="statements">The set of expressions to initialize an instance.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        [NotNull]
        public static IDisposable Register<T, T1, T2, T3, T4, T5>([NotNull] this IContainer container, Expression<Func<Context, T>> factory, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null, [NotNull] [ItemNotNull] params Expression<Action<Context<T>>>[] statements)
            where T : T1, T2, T3, T4, T5
            => container.Register(new[] { TypeDescriptor<T>.Type, TypeDescriptor<T1>.Type, TypeDescriptor<T2>.Type, TypeDescriptor<T3>.Type, TypeDescriptor<T4>.Type, TypeDescriptor<T5>.Type }, new AutoWiringDependency(factory, statements), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <typeparam name="T1">The additional contract type.</typeparam>
        /// <typeparam name="T2">The additional contract type.</typeparam>
        /// <typeparam name="T3">The additional contract type.</typeparam>
        /// <typeparam name="T4">The additional contract type.</typeparam>
        /// <typeparam name="T5">The additional contract type.</typeparam>
        /// <typeparam name="T6">The additional contract type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="factory">The expression to create an instance.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="statements">The set of expressions to initialize an instance.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        [NotNull]
        public static IDisposable Register<T, T1, T2, T3, T4, T5, T6>([NotNull] this IContainer container, Expression<Func<Context, T>> factory, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null, [NotNull] [ItemNotNull] params Expression<Action<Context<T>>>[] statements)
            where T : T1, T2, T3, T4, T5, T6
            => container.Register(new[] { TypeDescriptor<T>.Type, TypeDescriptor<T1>.Type, TypeDescriptor<T2>.Type, TypeDescriptor<T3>.Type, TypeDescriptor<T4>.Type, TypeDescriptor<T5>.Type, TypeDescriptor<T6>.Type }, new AutoWiringDependency(factory, statements), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <typeparam name="T1">The additional contract type.</typeparam>
        /// <typeparam name="T2">The additional contract type.</typeparam>
        /// <typeparam name="T3">The additional contract type.</typeparam>
        /// <typeparam name="T4">The additional contract type.</typeparam>
        /// <typeparam name="T5">The additional contract type.</typeparam>
        /// <typeparam name="T6">The additional contract type.</typeparam>
        /// <typeparam name="T7">The additional contract type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="factory">The expression to create an instance.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="statements">The set of expressions to initialize an instance.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        [NotNull]
        public static IDisposable Register<T, T1, T2, T3, T4, T5, T6, T7>([NotNull] this IContainer container, Expression<Func<Context, T>> factory, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null, [NotNull] [ItemNotNull] params Expression<Action<Context<T>>>[] statements)
            where T : T1, T2, T3, T4, T5, T6, T7
            => container.Register(new[] { TypeDescriptor<T>.Type, TypeDescriptor<T1>.Type, TypeDescriptor<T2>.Type, TypeDescriptor<T3>.Type, TypeDescriptor<T4>.Type, TypeDescriptor<T5>.Type, TypeDescriptor<T6>.Type, TypeDescriptor<T7>.Type }, new AutoWiringDependency(factory, statements), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <typeparam name="T1">The additional contract type.</typeparam>
        /// <typeparam name="T2">The additional contract type.</typeparam>
        /// <typeparam name="T3">The additional contract type.</typeparam>
        /// <typeparam name="T4">The additional contract type.</typeparam>
        /// <typeparam name="T5">The additional contract type.</typeparam>
        /// <typeparam name="T6">The additional contract type.</typeparam>
        /// <typeparam name="T7">The additional contract type.</typeparam>
        /// <typeparam name="T8">The additional contract type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <param name="factory">The expression to create an instance.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="statements">The set of expressions to initialize an instance.</param>
        /// <returns>The dependency token.</returns>
        [MethodImpl((MethodImplOptions)256)]
        [NotNull]
        public static IDisposable Register<T, T1, T2, T3, T4, T5, T6, T7, T8>([NotNull] this IContainer container, Expression<Func<Context, T>> factory, [CanBeNull] ILifetime lifetime = null, [CanBeNull] object[] tags = null, [NotNull] [ItemNotNull] params Expression<Action<Context<T>>>[] statements)
            where T : T1, T2, T3, T4, T5, T6, T7, T8
            => container.Register(new[] { TypeDescriptor<T>.Type, TypeDescriptor<T1>.Type, TypeDescriptor<T2>.Type, TypeDescriptor<T3>.Type, TypeDescriptor<T4>.Type, TypeDescriptor<T5>.Type, TypeDescriptor<T6>.Type, TypeDescriptor<T7>.Type, TypeDescriptor<T8>.Type }, new AutoWiringDependency(factory, statements), lifetime, tags);

        /// <summary>
        /// Registers a binding.
        /// </summary>
        /// <param name="container">The target container.</param>
        /// <param name="types">The set of types.</param>
        /// <param name="dependency">The dependency.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <param name="tags">The tags.</param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        [NotNull]
        public static IDisposable Register([NotNull] this IContainer container, [NotNull][ItemNotNull] IEnumerable<Type> types, [NotNull] IDependency dependency, [CanBeNull] ILifetime lifetime = null, [CanBeNull][ItemCanBeNull] params object[] tags)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (types == null) throw new ArgumentNullException(nameof(types));
            if (dependency == null) throw new ArgumentNullException(nameof(dependency));
            var keys =
                from type in types
                from tag in tags ?? DefaultTags
                select new Key(type, tag);

            return container.TryRegisterDependency(keys, dependency, lifetime, out var dependencyToken) 
                ? dependencyToken
                : container.Resolve<IIssueResolver>().CannotRegister(container, keys.ToArray());
        }
    }
}