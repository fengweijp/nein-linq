﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NeinLinq.Queryable
{
    /// <summary>
    /// Proxy for rewritten queries.
    /// </summary>
    public abstract class RewriteQuery : IOrderedQueryable
    {
        readonly Type elementType;
        readonly Expression expression;
        readonly IQueryProvider provider;
        readonly Lazy<IEnumerable> enumerable;

        /// <summary>
        /// Create a new query to rewrite.
        /// </summary>
        /// <param name="query">The actual query.</param>
        /// <param name="rewriter">The rewriter to rewrite the query.</param>
        protected RewriteQuery(IQueryable query, ExpressionVisitor rewriter)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            if (rewriter == null)
                throw new ArgumentNullException(nameof(rewriter));

            elementType = query.ElementType;
            expression = query.Expression;

            // replace query provider for further chaining
            provider = new RewriteQueryProvider(query.Provider, rewriter);

            // rewrite on enumeration
            enumerable = new Lazy<IEnumerable>(() =>
                query.Provider.CreateQuery(rewriter.Visit(query.Expression)));
        }

        /// <inheritdoc />
        public IEnumerator GetEnumerator() => enumerable.Value.GetEnumerator();

        /// <inheritdoc />
        public Type ElementType => elementType;

        /// <inheritdoc />
        public Expression Expression => expression;

        /// <inheritdoc />
        public IQueryProvider Provider => provider;
    }

    /// <summary>
    /// Proxy for rewritten queries.
    /// </summary>
    public class RewriteQuery<T> : RewriteQuery, IOrderedQueryable<T>
    {
        readonly Lazy<IEnumerable<T>> enumerable;

        /// <summary>
        /// Create a new query to rewrite.
        /// </summary>
        /// <param name="query">The actual query.</param>
        /// <param name="rewriter">The rewriter to rewrite the query.</param>
        public RewriteQuery(IQueryable<T> query, ExpressionVisitor rewriter)
            : base(query, rewriter)
        {
            // rewrite on enumeration
            enumerable = new Lazy<IEnumerable<T>>(() =>
                query.Provider.CreateQuery<T>(rewriter.Visit(query.Expression)));
        }

        /// <inheritdoc />
        public new IEnumerator<T> GetEnumerator() => enumerable.Value.GetEnumerator();
    }
}
