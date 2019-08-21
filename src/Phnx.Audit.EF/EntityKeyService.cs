using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Phnx.Audit.EF
{
    public class EntityKeyService<TContext> : IEntityKeyService<TContext> where TContext : DbContext
    {
        public EntityKeyService(TContext context)
        {
            Context = context;
        }

        protected TContext Context { get; }

        public IReadOnlyList<IProperty> GetPrimaryKeys<TEntity>() where TEntity : class
        {
            IEntityType type = Context.Model.FindEntityType(typeof(TEntity));

            IKey key = type.FindPrimaryKey();

            return key.Properties;
        }

        public TKey GetKey<TEntity, TKey>(TEntity entity) where TEntity : class
        {
            IReadOnlyList<IProperty> keys = GetPrimaryKeys<TEntity>();

            if (keys.Count == 1)
            {
                TKey key = Context.Entry(entity).CurrentValues.GetValue<TKey>(keys[0]);
                return key;
            }
            else if (typeof(TKey) == typeof(object))
            {
                dynamic model = new ExpandoObject();
                IDictionary<string, object> modelDictionary = model;

                foreach (var key in keys)
                {
                    object keyValue = Context.Entry(entity).CurrentValues.GetValue<object>(key);
                    modelDictionary.Add(key.Name, keyValue);
                }

                object modelObject = model;
                return (TKey)modelObject;
            }
            else
            {
                throw new NotSupportedException("Aggregate primary keys are not supported if " + nameof(TKey) + " is not set to type " + typeof(object));
            }
        }
    }
}
