using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

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
            return GetPrimaryKeys(typeof(TEntity));
        }

        public IReadOnlyList<IProperty> GetPrimaryKeys(Type entityType)
        {
            IEntityType type = Context.Model.FindEntityType(entityType);

            IKey key = type.FindPrimaryKey();

            return key.Properties;
        }

        public TKey GetPrimaryKey<TEntity, TKey>(TEntity entity) where TEntity : class
        {
            IDictionary<string, object> keys = GetKeyValues(entity);

            if (keys.Count > 1 && typeof(TKey) != typeof(object))
            {
                throw new NotSupportedException("Multiple primary keys are not supported if " + nameof(TKey) + " is not set to type " + typeof(object));
            }

            return (TKey)keys.First().Value;
        }

        public TKeys GetPrimaryKeys<TEntity, TKeys>(TEntity entity)
            where TEntity : class
            where TKeys : new()
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            IDictionary<string, object> keys = GetKeyValues(entity);

            if (keys.Count == 1)
            {
                throw new NotSupportedException("Cannot get multiple keys. Only one primary key was detected");
            }

            if (typeof(TKeys) == typeof(object))
            {
                dynamic dynamicKeys = new ExpandoObject();
                IDictionary<string, object> dynamicDictionary = dynamicKeys;

                foreach (var key in keys)
                {
                    dynamicDictionary.Add(key.Key, key.Value);
                }

                return (TKeys)dynamicKeys;
            }

            // Build keys type
            var keysModel = new TKeys();

            foreach (PropertyInfo property in typeof(TKeys).GetProperties())
            {
                if (keys.TryGetValue(property.Name, out var value))
                {
                    property.SetValue(keysModel, value);
                }
            }

            return keysModel;
        }

        private IDictionary<string, object> GetKeyValues<TEntity>(TEntity entity) where TEntity : class
        {
            IReadOnlyList<IProperty> keys = GetPrimaryKeys<TEntity>();

            IDictionary<string, object> modelDictionary = new Dictionary<string, object>();

            foreach (IProperty key in keys)
            {
                var keyValue = Context.Entry(entity).CurrentValues.GetValue<object>(key);
                modelDictionary.Add(key.Name, keyValue);
            }

            return modelDictionary;
        }
    }
}
