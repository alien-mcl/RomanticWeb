﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Moq;
using NUnit.Framework;
using RomanticWeb.DotNetRDF;
using RomanticWeb.LightInject;
using RomanticWeb.Mapping;
using RomanticWeb.Mapping.Sources;
using RomanticWeb.Ontologies;
using RomanticWeb.TestEntities.Animals;
using RomanticWeb.Tests.Stubs;
using VDS.RDF;

namespace RomanticWeb.Tests.IntegrationTests
{
    public abstract class IntegrationTestsBase
    {
        private IEntityContext _entityContext;
        private IEntityContextFactory _factory;
        private ServiceContainer _container;
        private ITripleStore _store;

        public virtual bool IncludeFoaf { get { return false; } }

        public IMappingProviderSource Mappings { get; private set; }

        protected static Uri MetaGraphUri { get { return new Uri("http://app.magi/graphs"); } }

        protected IEntityContext EntityContext
        {
            get
            {
                if (_entityContext == null)
                {
                    _entityContext = Factory.CreateContext();
                }

                return _entityContext;
            }
        }

        protected IEntityStore EntityStore { get { return EntityContext.Store; } }

        protected EntityContextFactory Factory { get { return (EntityContextFactory)_factory; } }

        protected virtual ITripleStore Store { get { return _store; } }

        protected virtual bool ThreadSafe { get { return false; } }

        protected virtual bool TrackChanges { get { return true; } }

        [SetUp]
        public void Setup()
        {
            Mappings = SetupMappings();

            _container = new ServiceContainer();
            _store = CreateTripleStore();
            EntityContextFactory factory = new EntityContextFactory(_container)
                                                 .WithOntology(new DefaultOntologiesProvider())
                                                 .WithOntology(new LifeOntology())
                                                 .WithOntology(new TestOntologyProvider(IncludeFoaf))
                                                 .WithOntology(new ChemOntology())
                                                 .WithMappings(BuildMappings)
                                                 .WithMetaGraphUri(MetaGraphUri)
                                                 .WithDotNetRDF(_store)
                                                 .WithBaseUri(b => b.Default.Is(new Uri("http://example.com/")));
            factory.ThreadSafe = ThreadSafe;
            factory.TrackChanges = TrackChanges;
            _factory = factory;
            ChildSetup();
        }

        [TearDown]
        public void Teardown()
        {
            ChildTeardown();
            _factory.Dispose();
            _entityContext = null;
            _store = null;
        }

        protected virtual void BuildMappings(MappingBuilder m)
        {
            m.FromAssemblyOf<IAnimal>();
            m.AddMapping(GetType().GetTypeInfo().Assembly, Mappings);
        }

        protected virtual void ChildTeardown()
        {
        }

        protected virtual IMappingProviderSource SetupMappings()
        {
            var mock = new Mock<IMappingProviderSource>();
            mock.SetupGet(p => p.Description).Returns("Mock provider");
            return mock.Object;
        }

        protected virtual void ChildSetup()
        {
        }

        protected virtual ITripleStore CreateTripleStore()
        {
            return new TripleStore();
        }

        protected abstract void LoadTestFile(string fileName);

        public class LifeOntology : IOntologyProvider
        {
            public IEnumerable<IOntology> Ontologies
            {
                get
                {
                    yield return new Ontology("life", new Uri("http://example/livingThings#"));
                }
            }

            public Uri ResolveUri(string prefix, string rdfTermName)
            {
                if (prefix == "life")
                {
                    return new Uri("http://example/livingThings#" + rdfTermName);
                }

                return null;
            }
        }

        public class ChemOntology : IOntologyProvider
        {
            public IEnumerable<IOntology> Ontologies
            {
                get
                {
                    yield return new Ontology("chem", new Uri("http://chem.com/vocab/"));
                }
            }

            public Uri ResolveUri(string prefix, string rdfTermName)
            {
                if (prefix == "chem")
                {
                    return new Uri("http://chem.com/vocab/" + rdfTermName);
                }

                return null;
            }
        }
    }
}