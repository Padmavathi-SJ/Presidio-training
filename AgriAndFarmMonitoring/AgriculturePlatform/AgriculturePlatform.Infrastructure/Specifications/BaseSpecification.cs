// AgriculturePlatform.Infrastructure/Specifications/BaseSpecification.cs
using System.Linq.Expressions;

namespace AgriculturePlatform.Infrastructure.Specifications;

public abstract class BaseSpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    // ✅ FIX: Combine criteria instead of replacing
    protected void AddCriteria(Expression<Func<T, bool>> criteria)
    {
        if (Criteria == null)
        {
            Criteria = criteria;
        }
        else
        {
            // Combine with AND
            var parameter = Expression.Parameter(typeof(T));
            var left = new ReplaceParameterVisitor(Criteria.Parameters[0], parameter).Visit(Criteria.Body);
            var right = new ReplaceParameterVisitor(criteria.Parameters[0], parameter).Visit(criteria.Body);
            var combined = Expression.AndAlso(left!, right!);
            var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
            Criteria = lambda;
        }
    }

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }

    // ✅ Helper class to replace parameter in expression
    private class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        public ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }
}