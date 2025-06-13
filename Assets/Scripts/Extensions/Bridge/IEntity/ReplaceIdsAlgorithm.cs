using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Bridge.Models.Common;

namespace Extensions
{
    internal class ReplaceIdsAlgorithm<T>: IReplaceAlgorithm<T> where T:IEntity
    {
        protected const string ID_FIELD_NAME = nameof(IEntity.Id);

        protected Type[] ChildTypesMustBeReplaced;
        protected string[] FkPropertiesNames;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ReplaceId(IEntity entity, Func<long, bool> condition, Func<string, long> generator)
        {
            if (entity is T)
            {
                ReplaceIdRecursively(null, entity, condition, generator);
            }
            else
            {
                throw new InvalidOperationException($"Current entity type {entity.GetType().Name} is not target type {typeof(T).Name}");
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ReplaceIdRecursively(object parent, object target, Func<long, bool> condition, Func<string, long> generator)
        {
            var type = target.GetType();

            if (target is IEntity entity && (type == typeof(T) || ChildTypesMustBeReplaced.Contains(type)))
            {
                ResetEntityIdAndFk(parent, entity, condition, generator);
            }

            var navigationProperties = GetNavigationProperties(target);
            var collectionType = typeof(IEnumerable);
            var entityNavProperties = navigationProperties.Where(x => !collectionType.IsAssignableFrom(x.PropertyType));

            foreach (var navigationProperty in entityNavProperties)
            {
                var val = navigationProperty.GetValue(target);
                if (val != null) ReplaceIdRecursively(target, val, condition, generator);
            }

            var collectionNavProps = navigationProperties.Where(x => collectionType.IsAssignableFrom(x.PropertyType));

            foreach (var propertyInfo in collectionNavProps)
            {
                var propValue = propertyInfo.GetValue(target);
                if (propValue == null) continue;

                if (!(propValue is IEnumerable collection)) continue;

                var castCollection = collection.Cast<object>().ToArray();
                foreach (var member in castCollection)
                {
                    ReplaceIdRecursively(target, member, condition, generator);
                }
            }
        }
        
        private void ResetEntityIdAndFk(object parent, IEntity entity, Func<long, bool> condition, Func<string, long> generator)
        {
            if (condition.Invoke(entity.Id))
            {
                var typeName = entity.GetType().Name;
                var newId = generator.Invoke(typeName);
                entity.Id = newId;
            }

            var fk = GetForeignKeyPropertyMustBeReset(parent, entity);
            fk?.SetValue(parent, entity.Id);
        }

        private PropertyInfo GetForeignKeyPropertyMustBeReset(object parent, IEntity entity)
        {
            if (parent == null) return null;

            var fkName = $"{entity.GetModelType()}{ID_FIELD_NAME}";
            return parent.GetType()
                         .GetProperties()
                         .Where(x => x.Name == fkName).FirstOrDefault(x => x.PropertyType == typeof(long) || x.PropertyType == typeof(long?));
        }

        private static PropertyInfo[] GetNavigationProperties(object target)
        {
            return target.GetType().GetProperties().Where(x => !IsPrimitive(x.PropertyType)).ToArray();
        }

        private static bool IsPrimitive(Type type)
        {
            return type.IsPrimitive || type.IsValueType || type == typeof(string);
        }
    }
}