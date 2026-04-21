using System.Linq.Expressions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _db;
    protected readonly DbSet<T> _set;

    public GenericRepository(AppDbContext db)
    {
        _db  = db;
        _set = db.Set<T>();
    }

    public IQueryable<T> Query() => _set.AsQueryable();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public void Update(T entity) => _set.Update(entity);

    public void Delete(T entity)
    {
        if (entity is TenantEntity te)
        {
            te.IsDeleted = true;
            te.DeletedAt = DateTimeOffset.UtcNow;
            _set.Update(entity);
        }
        else
        {
            _set.Remove(entity);
        }
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _set.CountAsync(predicate, ct);
}
