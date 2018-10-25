﻿namespace IoC
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Represents the abstraction for build context.
    /// </summary>
    [PublicAPI]
    public interface IBuildContext
    {
        /// <summary>
        /// The target key.
        /// </summary>
        Key Key { get; }

        /// <summary>
        /// The depth of current context.
        /// </summary>
        int Depth { get; }

        /// <summary>
        /// The target container.
        /// </summary>
        [NotNull] IContainer Container { get; }

        /// <summary>
        /// Autowiring strategy.
        /// </summary>
        [NotNull] IAutowiringStrategy AutowiringStrategy { get; }

        /// <summary>
        /// Creates a child context.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="container">The container.</param>
        /// <returns>The new build context.</returns>
        [NotNull] IBuildContext CreateChild(Key key, [NotNull] IContainer container);

        /// <summary>
        /// Prepares base expression, replacing generic types' markers.
        /// </summary>
        /// <param name="baseExpression">The base expression.</param>
        /// <returns>The resulting expression.</returns>
        [NotNull] Expression PrepareTypes([NotNull] Expression baseExpression);

        /// <summary>
        /// Add element for replacing generic types' markers.
        /// </summary>
        /// <param name="type">The target raw type.</param>
        /// <param name="targetType">The replacing type.</param>
        void MapTypes([NotNull] Type type, [NotNull]Type targetType);

        /// <summary>
        /// Add replacing generic types' markers.
        /// </summary>
        /// <param name="type">The target raw type.</param>
        /// <param name="targetType">The replacing type.</param>
        /// <returns></returns>
        bool TryReplaceType([NotNull] Type type, out Type targetType);

        /// <summary>
        /// Prepares base expression, injecting dependencies. 
        /// </summary>
        /// <param name="baseExpression">The base expression.</param>
        /// <param name="instanceExpression">The instance expression.</param>
        /// <returns>The resulting expression.</returns>
        [NotNull] Expression MakeInjections([NotNull] Expression baseExpression, [CanBeNull] ParameterExpression instanceExpression = null);

        /// <summary>
        /// Wraps by lifetime.
        /// </summary>
        /// <param name="baseExpression">The base expression.</param>
        /// <param name="lifetime">The target lifetime.</param>
        /// <returns></returns>
        [NotNull] Expression AppendLifetime([NotNull] Expression baseExpression, [CanBeNull] ILifetime lifetime);

        /// <summary>
        /// Appends value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The value type.</param>
        /// <returns>The parameter expression.</returns>
        [NotNull] Expression AppendValue([CanBeNull] object value, [NotNull] Type type);

        /// <summary>
        /// Appends value.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The parameter expression.</returns>
        [NotNull] Expression AppendValue<T>([CanBeNull] T value);

        /// <summary>
        /// Appends variable.
        /// </summary>
        /// <param name="expression">The value expression.</param>
        /// <returns>The parameter expression.</returns>
        [NotNull] ParameterExpression AppendVariable([NotNull] Expression expression);

        /// <summary>
        /// Closes block for specified variables.
        /// </summary>
        /// <param name="targetExpression">The target expression.</param>
        /// <param name="variableExpressions">Variable expressions.</param>
        /// <returns>The resulting block expression.</returns>
        [NotNull] Expression CloseBlock([NotNull] Expression targetExpression, [NotNull][ItemNotNull] params ParameterExpression[] variableExpressions);

        /// <summary>
        /// Creates an expression for dependency based on Key.
        /// </summary>
        /// <returns>The dependency expression.</returns>
        [NotNull] Expression CreateDependencyExpression();
    }
}