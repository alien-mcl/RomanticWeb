﻿using System;
using RomanticWeb.Mapping;

namespace RomanticWeb.Tests.Stubs
{
    using RomanticWeb.Mapping.Model;

    public class TestPropertyMapping : IPropertyMapping
	{
		public Uri Uri { get; set; }

		public IGraphSelectionStrategy GraphSelector { get; set; }

        public string Name { get; set; }

        public bool IsCollection { get; set; }

        public Type ReturnType { get; set; }
	}
}