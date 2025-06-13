using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Bridge.Models.Common;

namespace Extensions.ResetEntity
{
    internal class GenericResetAlgorithm<T>: ResetAlgorithm where T:IEntity
    {
        private readonly Type[] _childTypesMustBeReset;
        private readonly string[] _fkPropertiesNames;
        private const string ID_FIELD_NAME = nameof(IEntity.Id);
        
        public GenericResetAlgorithm(Type[] childTypesMustBeReset)
        {
            _childTypesMustBeReset = childTypesMustBeReset;
            _fkPropertiesNames = _childTypesMustBeReset.Concat(new []{typeof(T)}).Select(x => GetEntityTypeUnifiedName(x) + ID_FIELD_NAME).ToArray();
        }

        public sealed override void Reset(IEntity entity)
        {
            if(entity is T)
                ResetRecursively(entity);
            else 
                throw new InvalidOperationException($"Current entity type {entity.GetType().Name} is not target type {typeof(T).Name}");
        }
        
        private void ResetRecursively(object target)
        {
            var type = target.GetType();

            if (target is IEntity entity && (type == typeof(T) || _childTypesMustBeReset.Contains(type)))
            {
                ResetEntityIdAndFk(entity);
            }

            var navigationProperties = GetNavigationProperties(target);
            var collectionType = typeof(IEnumerable);
            var entityNavProperties = navigationProperties.Where(x => !collectionType.IsAssignableFrom(x.PropertyType));
            foreach (var navigationProperty in entityNavProperties)
            {
                var val = navigationProperty.GetValue(target);
                if(val!=null)
                    ResetRecursively(val);
            }

            var collectionNavProps = navigationProperties.Where(x => collectionType.IsAssignableFrom(x.PropertyType));
            foreach (var propertyInfo in collectionNavProps)
            {
                var propValue = propertyInfo.GetValue(target);
                if(propValue==null)
                    continue;
                
                if (!(propValue is IEnumerable collection)) 
                    continue;

                var castCollection = collection.Cast<object>().ToArray();
                foreach (var member in castCollection)
                {
                    ResetRecursively(member);
                }
            }
        }

        private void ResetEntityIdAndFk(IEntity entity)
        {
            entity.Id = 0;
            var fks = GetForeignKeyPropertiesMustBeReset(entity);
            foreach (var propertyInfo in fks)
            {
                if (IsNullable(propertyInfo.PropertyType))
                    propertyInfo.SetValue(entity, null);
                else
                    propertyInfo.SetValue(entity, 0);
            }
        }
        
        static bool IsNullable(Type type)
        {
            if (!type.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
            return false; // value-type
        }

        private PropertyInfo[] GetForeignKeyPropertiesMustBeReset(object entity)
        {
            var fkProperties = entity.GetType().GetProperties().Where(x => x.PropertyType == typeof(long) || x.PropertyType==typeof(long?))
                .Where(x => x.Name.EndsWith(nameof(IEntity.Id)));

            return fkProperties.Where(x => _fkPropertiesNames.Contains(x.Name)).ToArray();
        }

        private PropertyInfo[] GetNavigationProperties(object target)
        {
            return target.GetType().GetProperties().Where(x => !IsPrimitive(x.PropertyType)).ToArray();
        }
        
        private bool IsPrimitive(Type type)
        {
            return type.IsPrimitive || type.IsValueType || type == typeof(string);
        }

        private string GetEntityTypeUnifiedName(Type entityType)
        {
            return DbModelExtensions.GetModelType(entityType).ToString();
        }
    }
}