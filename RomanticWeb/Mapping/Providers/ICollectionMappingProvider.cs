using System;
using RomanticWeb.Mapping.Model;

namespace RomanticWeb.Mapping.Providers
{
    /// <summary>Provides collection mapping.</summary>
    public interface ICollectionMappingProvider : IPropertyMappingProvider
    {
        /// <summary>Gets or sets the storage strategy.</summary>
        StoreAs StoreAs { get; set; }

        /// <summary>Gets or sets an element converter type.</summary>
        Type ElementConverterType { get; set; }
    }
}