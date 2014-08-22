﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RomanticWeb.LightInject;

namespace RomanticWeb.Converters
{
    /// <summary>
    /// Default implementation of <see cref="IConverterCatalog"/>
    /// </summary>
    public sealed class ConverterCatalog : IConverterCatalog
    {
        private readonly IDictionary<Type, INodeConverter> _nodeConverters = new ThreadSafeDictionary<Type, INodeConverter>();

        public ConverterCatalog(IEnumerable<INodeConverter> converters)
        {
            foreach (var converter in converters)
            {
                AddConverter(converter);
            }
        }

        internal ConverterCatalog()
            : this(new INodeConverter[0])
        {
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<INodeConverter> UriNodeConverters
        {
            get
            {
                return new ReadOnlyCollection<INodeConverter>(_nodeConverters.Values.ToList());
            }
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<ILiteralNodeConverter> LiteralNodeConverters
        {
            get
            {
                return new ReadOnlyCollection<ILiteralNodeConverter>(_nodeConverters.Values.Where(c => c is LiteralNodeConverter).Cast<ILiteralNodeConverter>().ToList());
            }
        }

        /// <inheritdoc/>
        public INodeConverter GetConverter(Type converterType)
        {
            if (!_nodeConverters.ContainsKey(converterType))
            {
                AddConverter(CreateConverter(converterType));
            }

            return _nodeConverters[converterType];
        }

        /// <inheritdoc/>
        public void AddConverter(INodeConverter nodeConverter)
        {
            _nodeConverters[nodeConverter.GetType()] = nodeConverter;
        }

        private INodeConverter CreateConverter(Type converterType)
        {
            if (converterType == typeof(FallbackNodeConverter))
            {
                return new FallbackNodeConverter(this);
            }

            return (INodeConverter)Activator.CreateInstance(converterType);
        }
    }
}