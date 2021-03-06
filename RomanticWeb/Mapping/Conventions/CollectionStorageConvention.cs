﻿using System;
using System.Collections.Generic;
using System.Reflection;
using RomanticWeb.Mapping.Model;
using RomanticWeb.Mapping.Providers;

namespace RomanticWeb.Mapping.Conventions
{
    /// <summary>Convention to ensure <see cref="ICollection{T}"/> and <see cref="IEnumerable{T}"/> properties are stored and read as RDF multi objects.</summary>
    public class CollectionStorageConvention : ICollectionConvention
    {
        private static readonly IEnumerable<Type> Predecesors = new Type[0];
 
        /// <inheritdoc/>
        public IEnumerable<Type> Requires { get { return Predecesors; } }

        /// <inheritdoc/>
        public bool ShouldApply(ICollectionMappingProvider target)
        {
            var propertyType = target.PropertyInfo.PropertyType;

            return (target.StoreAs == StoreAs.Undefined) && ((propertyType.IsArray) || ((propertyType.GetTypeInfo().IsGenericType) && 
                (propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>) || (propertyType.GetGenericTypeDefinition() == typeof(ICollection<>)))));
        }

        /// <inheritdoc/>
        public void Apply(ICollectionMappingProvider target)
        {
            target.StoreAs = StoreAs.SimpleCollection;
        }
    }
}