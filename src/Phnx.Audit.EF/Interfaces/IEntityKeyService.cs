using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using System.Reflection;

namespace Phnx.Audit.EF
{
    public interface IEntityKeyService<TContext> where TContext : DbContext
    {
        TKey GetKey<TEntity, TKey>(TEntity entity) where TEntity : class;
        IReadOnlyList<IProperty> GetPrimaryKeys<TEntity>() where TEntity : class;
    }
}