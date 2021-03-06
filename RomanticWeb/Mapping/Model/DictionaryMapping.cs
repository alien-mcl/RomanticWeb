﻿using System;
using System.Diagnostics;
using RomanticWeb.Mapping.Visitors;

namespace RomanticWeb.Mapping.Model
{
    [DebuggerDisplay("Dictionary {Name}")]
    internal class DictionaryMapping : PropertyMapping, IDictionaryMapping
    {
        public DictionaryMapping(Type declaringType, Type returnType, string name, Uri predicateUri, Uri keyPredicate, Uri valuePredicate)
            : base(declaringType, returnType, name, predicateUri)
        {
            ValuePredicate = valuePredicate;
            KeyPredicate = keyPredicate;
        }

        public Uri KeyPredicate { get; private set; }

        public Uri ValuePredicate { get; private set; }

        void IPropertyMapping.Accept(IMappingModelVisitor mappingModelVisitor)
        {
            mappingModelVisitor.Visit(this);
        }
    }
}