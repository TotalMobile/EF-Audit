using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;

namespace Phnx.Audit.EF
{
    public interface IEntityKeyService<TContext> where TContext : DbContext
    {
        TKey GetPrimaryKey<TEntity, TKey>(TEntity entity) where TEntity : class;
        TKeys GetPrimaryKeys<TEntity, TKeys>(TEntity entity) where TEntity : class where TKeys : new();
        IReadOnlyList<IProperty> GetPrimaryKeys<TEntity>() where TEntity : class;
        IReadOnlyList<IProperty> GetPrimaryKeys(Type entityType);
    }
}