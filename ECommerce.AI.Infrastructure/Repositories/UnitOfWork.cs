using ECommerce.AI.Domain.Interfaces;
using ECommerce.AI.Domain.Common;
using ECommerce.AI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace ECommerce.AI.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ECommerceDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ECommerceDbContext context)
    {
        _context = context;
        Products = new ProductRepository(_context);
        Categories = new CategoryRepository(_context);
    }

    public IProductRepository Products { get; }
    public ICategoryRepository Categories { get; }

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        if (_repositories.ContainsKey(typeof(T)))
        {
            return (IRepository<T>)_repositories[typeof(T)];
        }

        var repository = new Repository<T>(_context) as IRepository<T>;
        _repositories.Add(typeof(T), repository!);
        return repository!;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}