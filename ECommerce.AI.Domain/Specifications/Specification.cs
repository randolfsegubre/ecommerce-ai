using System.Linq.Expressions;

namespace ECommerce.AI.Domain.Specifications;

/// <summary>
/// Base specification interface
/// </summary>
/// <typeparam name="T">The type to specify</typeparam>
public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
    bool IsSatisfiedBy(T entity);
}

/// <summary>
/// Base specification implementation
/// </summary>
/// <typeparam name="T">The type to specify</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }

    public Specification<T> And(ISpecification<T> specification)
    {
        return new AndSpecification<T>(this, specification);
    }

    public Specification<T> Or(ISpecification<T> specification)
    {
        return new OrSpecification<T>(this, specification);
    }

    public Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }

    public static implicit operator Expression<Func<T, bool>>(Specification<T> specification)
    {
        return specification.ToExpression();
    }
}

/// <summary>
/// AND specification combinator
/// </summary>
internal class AndSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(T), "x");
        var leftVisitor = new ReplaceExpressionVisitor(leftExpression.Parameters[0], parameter);
        var left = leftVisitor.Visit(leftExpression.Body)!;

        var rightVisitor = new ReplaceExpressionVisitor(rightExpression.Parameters[0], parameter);
        var right = rightVisitor.Visit(rightExpression.Body)!;

        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, right), parameter);
    }
}

/// <summary>
/// OR specification combinator
/// </summary>
internal class OrSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    public OrSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(T), "x");
        var leftVisitor = new ReplaceExpressionVisitor(leftExpression.Parameters[0], parameter);
        var left = leftVisitor.Visit(leftExpression.Body)!;

        var rightVisitor = new ReplaceExpressionVisitor(rightExpression.Parameters[0], parameter);
        var right = rightVisitor.Visit(rightExpression.Body)!;

        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(left, right), parameter);
    }
}

/// <summary>
/// NOT specification combinator
/// </summary>
internal class NotSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _specification;

    public NotSpecification(ISpecification<T> specification)
    {
        _specification = specification;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var expression = _specification.ToExpression();
        var parameter = expression.Parameters.Single();
        var body = Expression.Not(expression.Body);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}

/// <summary>
/// Helper class for replacing expression parameters
/// </summary>
internal class ReplaceExpressionVisitor : ExpressionVisitor
{
    private readonly Expression _oldValue;
    private readonly Expression _newValue;

    public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
    {
        _oldValue = oldValue;
        _newValue = newValue;
    }

    public override Expression? Visit(Expression? node)
    {
        return node == _oldValue ? _newValue : base.Visit(node);
    }
}