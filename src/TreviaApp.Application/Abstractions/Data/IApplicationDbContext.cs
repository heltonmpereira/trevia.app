using Microsoft.EntityFrameworkCore;
using TreviaApp.Domain.Interfaces;

namespace TreviaApp.Application.Abstractions.Data;

public interface IApplicationDbContext : IUnitOfWork
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
}
