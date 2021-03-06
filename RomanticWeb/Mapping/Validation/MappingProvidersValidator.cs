﻿using System;
using System.Collections.Generic;
using RomanticWeb.Diagnostics;
using RomanticWeb.Mapping.Providers;

namespace RomanticWeb.Mapping.Validation
{
    /// <summary>A visitor, which executes validation logic on mapping providers.</summary>
    public class MappingProvidersValidator : Visitors.IMappingProviderVisitor
    {
        private readonly ILogger _log;
        private Type _currentType;

        /// <summary>Initializes a new instance of the <see cref="MappingProvidersValidator" /> class.</summary>
        /// <param name="log">Logging facility.</param>
        public MappingProvidersValidator(ILogger log)
        {
            _log = log;
        }

        public IEnumerable<Type> Requires { get { return Type.EmptyTypes; } }

        /// <summary>Validates the specified collection mapping provider.</summary>
        public void Visit(ICollectionMappingProvider collectionMappingProvider)
        {
            Visit(collectionMappingProvider as IPropertyMappingProvider);
        }

        /// <summary>Validates the specified property mapping provider.</summary>
        public void Visit(IPropertyMappingProvider propertyMappingProvider)
        {
            AssertTermMapped(propertyMappingProvider);
            AssertConverter(propertyMappingProvider);
        }

        /// <summary>Validates the specified dictionary mapping provider.</summary>
        public void Visit(IDictionaryMappingProvider dictionaryMappingProvider)
        {
            Visit(dictionaryMappingProvider as IPropertyMappingProvider);
            AssertTermMapped(dictionaryMappingProvider.Key, string.Format("{0}.Key", dictionaryMappingProvider));
            AssertTermMapped(dictionaryMappingProvider.Value, string.Format("{0}.Value", dictionaryMappingProvider));
        }

        /// <summary>Validates the specified class mapping provider.</summary>
        public void Visit(IClassMappingProvider classMappingProvider)
        {
            AssertTermMapped(classMappingProvider);
        }

        /// <summary>Validates the specified entity mapping provider.</summary>
        public void Visit(IEntityMappingProvider entityMappingProvider)
        {
            _currentType = entityMappingProvider.EntityType;
        }

        private void AssertConverter(IPropertyMappingProvider propertyMappingProvider)
        {
            if (propertyMappingProvider.ConverterType == null)
            {
                _log.Warning("Entity {0}: missing converter for property {1}", _currentType, propertyMappingProvider);
            }
        }

        private void AssertTermMapped(ITermMappingProvider term, string errorString = null)
        {
            if (term.GetTerm == null)
            {
                _log.Warning("Entity {0}: missing term for {1}", _currentType, errorString ?? term.ToString());
            }
        }
    }
}