﻿using NUnit.Framework;

namespace RomanticWeb.Tests.IntegrationTests
{
	[TestFixture]
	public class InMemoryTripleStoreDatatypeTests : InMemoryTripleStoreTestsBase
	{
		[Test]
		public void Creating_Entity_should_allow_accessing_existing_literal_properties()
		{
			// given
			LoadTestFile("TriplesWithLiteralSubjects.ttl");

			// when
			dynamic tomasz = EntityFactory.Create(new UriId("http://magi/people/Tomasz"));

			// then
			Assert.That(tomasz.foaf.givenName, Is.EqualTo("Tomasz"));
			Assert.That(tomasz.foaf.familyName, Is.EqualTo("Pluskiewicz"));
			Assert.That(tomasz.foaf.nick == null);
		}
	}
}