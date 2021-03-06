﻿namespace IoC
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Core;

    /// <summary>
    /// The list of well-known expressions.
    /// </summary>
    [PublicAPI]
    public static class WellknownExpressions
    {
        /// <summary>
        /// The container parameter.
        /// </summary>
        [NotNull]
        public static readonly ParameterExpression ContainerParameter = Expression.Parameter(TypeDescriptor<IContainer>.Type, nameof(Context.Container));

        /// <summary>
        /// The args parameters.
        /// </summary>
        [NotNull]
        public static readonly ParameterExpression ArgsParameter = Expression.Parameter(TypeDescriptor<object[]>.Type, nameof(Context.Args));

        /// <summary>
        /// All resolvers parameters.
        /// </summary>
        [NotNull][ItemNotNull]
        public static readonly IEnumerable<ParameterExpression> ResolverParameters = new List<ParameterExpression>{ ContainerParameter, ArgsParameter };
    }
}
