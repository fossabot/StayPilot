using System.Linq.Expressions;

namespace StayPilot.Infrastructure.Persistence.QueryFilters;

/// <summary>
/// Combines predicate expressions by rebinding parameters (rather than using
/// <see cref="Expression.Invoke(Expression, IEnumerable{Expression})"/>), so the
/// result stays translatable by EF Core as a global query filter.
/// </summary>
internal static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> AndAlso<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T), "e");

        var leftBody = new ReplaceParameterVisitor(left.Parameters[0], parameter).Visit(left.Body);
        var rightBody = new ReplaceParameterVisitor(right.Parameters[0], parameter).Visit(right.Body);

        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(leftBody!, rightBody!), parameter);
    }

    private sealed class ReplaceParameterVisitor(ParameterExpression from, ParameterExpression to)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == from ? to : base.VisitParameter(node);
    }
}
