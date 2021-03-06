﻿using System;
using System.Diagnostics;
using RomanticWeb.Entities;

namespace RomanticWeb.Model
{
    /// <summary>Represents an RDF node (URI or literal).</summary>
    /// <remarks>Blank nodes are not supported currently.</remarks>
    [DebuggerDisplay("{DebuggerString,nq}")]
    [DebuggerTypeProxy(typeof(DebuggerViewProxy))]
    public sealed class Node : INode
    {
        /// <summary>Gets a reference for node with rdf:type predicate usually shortened in Turtle syntax to 'a'.</summary>
        private static readonly Node A = new Node(Vocabularies.Rdf.type);

        private readonly string _literal;
        private readonly string _language;
        private readonly Uri _dataType;
        private readonly Uri _uri;
        private readonly BlankId _blankNodeId;
        private readonly string _identifier;
        private readonly int _hashCode;

        private Node(Uri uri)
        {
            _uri = uri;
            _hashCode = CalculateHashCode();
        }

        private Node(string literal, string language, Uri dataType)
        {
            if (language != null && dataType != null)
            {
                throw new InvalidOperationException("Literal node cannot have both laguage and data type");
            }

            _literal = literal;
            _language = language;
            _dataType = dataType;
            _hashCode = CalculateHashCode();
        }

        private Node(string identifier, Uri graphUri,  EntityId entityId)
        {
            _identifier = identifier;
            _blankNodeId = new BlankId(identifier, entityId, graphUri);
            _uri = _blankNodeId.Uri;
            _hashCode = CalculateHashCode();
        }

        /// <summary>Gets the value indicating that the node is a URI.</summary>
        public bool IsUri { get { return (_uri != null) && (_blankNodeId == null); } }

        /// <summary>Gets the value indicating that the node is a literal.</summary>
        public bool IsLiteral { get { return _literal != null; } }

        /// <summary>Gets the value indicating that the node is a blank node.</summary>
        public bool IsBlank { get { return _blankNodeId != null; } }

        /// <summary>Gets the URI of a URI node.</summary>
        /// <exception cref="InvalidOperationException">thrown when node is a literal.</exception>
        public Uri Uri
        {
            get
            {
                if (!IsUri)
                {
                    throw new InvalidOperationException("Only Uri nodes have a Uri");
                }

                return _uri;
            }
        }

        /// <summary>Gets the string value of a literal node.</summary>
        /// <exception cref="InvalidOperationException">thrown when node is URI.</exception>
        public string Literal
        {
            get
            {
                if (IsUri || IsBlank)
                {
                    throw new InvalidOperationException("Uri and blank nodes do not have a literal value");
                }

                return _literal;
            }
        }

        /// <summary>Gets the string value of a blank node.</summary>
        public string BlankNode
        {
            get
            {
                if (!IsBlank)
                {
                    throw new InvalidOperationException("Uri and literal nodes do not have a blank node value");
                }

                return _identifier;
            }
        }

        /// <summary>Gets the data type of a literal node.</summary>
        /// <exception cref="InvalidOperationException">thrown when node is URI.</exception>
        public Uri DataType
        {
            get
            {
                if (IsUri || IsBlank)
                {
                    throw new InvalidOperationException("Uri and blank nodes do not have a data type");
                }

                return _dataType;
            }
        }

        /// <summary>Gets the language tag of a literal node.</summary>
        /// <exception cref="InvalidOperationException">thrown when node is URI.</exception>
        public string Language
        {
            get
            {
                if (IsUri || IsBlank)
                {
                    throw new InvalidOperationException("Uri and blank nodes do not have a language tag");
                }

                return _language;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerString
        {
            get
            {
                if (IsUri)
                {
                    return Uri.ToString();
                }

                if (IsLiteral)
                {
                    return Literal;
                }

                if (IsBlank)
                {
                    return _identifier;
                }

                return null;
            }
        }

        /// <summary>Factory method for creating URI nodes.</summary>
        public static Node ForUri(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                throw new ArgumentOutOfRangeException("uri", "Node URI must be absolute!");
            }

            return (uri.AbsoluteUri == Vocabularies.Rdf.type.AbsoluteUri ? A : new Node(uri));
        }

        /// <summary>Factory method for creating simple literal nodes.</summary>
        public static Node ForLiteral(string literal)
        {
            return new Node(literal, null, null);
        }

        /// <summary>Factory method for creating typed literal nodes.</summary>
        public static Node ForLiteral(string literal, Uri datatype)
        {
            return new Node(literal, null, datatype);
        }

        /// <summary>Factory method for creating literal nodes with language tag.</summary>
        public static Node ForLiteral(string literal, string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                language = null;
            }

            return new Node(literal, language, null);
        }

        /// <summary>Factory method for creating blank nodes.</summary>
        public static Node ForBlank(string blankNodeId, EntityId entityId,  Uri graphUri)
        {
            return new Node(blankNodeId, graphUri, entityId);
        }

        /// <summary>Factory method for creating nodes from <see cref="EntityId"/>.</summary>
        /// <returns>A URI node or a Blank Node</returns>
        public static Node FromEntityId(EntityId entityId)
        {
            if (entityId is BlankId)
            {
                return ((BlankId)entityId).ToNode();
            }

            return ForUri(entityId.Uri);
        }

#pragma warning disable 1591
        public static bool operator ==(Node left, Node right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Node left, Node right)
        {
            return !Equals(left, right);
        }
#pragma warning restore 1591

        /// <summary>Determines whether the specified System.Object is equal to the current node.</summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var otherNode = obj as Node;
            return (otherNode != null && Equals(otherNode));
        }

        /// <summary>Gets hash code for the node.</summary>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        int IComparable.CompareTo(object obj)
        {
            return ((IComparable<INode>)this).CompareTo(obj as Node);
        }

        int IComparable<INode>.CompareTo(INode other)
        {
            var compare = FluentCompare<Node>.Arguments(this, other);

            if ((IsUri || IsBlank))
            {
                if ((IsUri) && (other.IsUri))
                {
                    return compare.By(n => n.Uri.AbsoluteUri).End();
                }

                if ((IsBlank) && (other.IsBlank))
                {
                    return compare.By(n => n.BlankNode).End();
                }

                if (((IsUri) && (!other.IsUri)) || (IsBlank && other.IsLiteral))
                {
                    // blank node is more than literal node
                    // Uri node is more than blank node
                    return 1;
                }

                // Literal node is always less then URI and blanks
                return -1;
            }

            if (other.IsLiteral)
            {
                return compare.By(n => n.Literal, StringComparer.Ordinal)
                              .By(n => n.DataType, new AbsoluteUriComparer())
                              .By(n => n.Language, StringComparer.OrdinalIgnoreCase).End();
            }

            // Literal node is always less then URI and blanks
            return -1;
        }

        bool IEquatable<INode>.Equals(INode other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (ReferenceEquals(other, null))
            {
                return false;
            }

            var otherNode = other as Node;
            if (otherNode != null)
            {
                return Equals(otherNode);
            }

            return (IsUri && other.IsUri && AbsoluteUriComparer.Default.Equals(Uri, other.Uri)) ||
                (IsBlank && other.IsBlank && BlankNode.Equals(other.BlankNode)) ||
                (IsLiteral && other.IsLiteral && Equals(Literal, other.Literal) && (Equals(Language, other.Language)));
        }

        /// <summary>Gets the string representation of a node.</summary>
        /// <returns>Literal value or URI for literal and URI nodes respectively</returns>
        public override string ToString()
        {
            if (IsLiteral)
            {
                return Literal;
            }

            if (IsUri)
            {
                return Uri.ToString();
            }

            if (IsBlank)
            {
                return _blankNodeId.ToString();
            }

            throw new InvalidOperationException("Invalid node state");
        }

        /// <summary>Returns a <see cref="System.String" /> that represents this instance.</summary>
        /// <param name="nQuadFormat">if set to <c>true</c> the string will be a valid NQuad node.</param>
        public string ToString(bool nQuadFormat)
        {
            if (!nQuadFormat)
            {
                return ToString();
            }

            if (IsLiteral)
            {
                string suffix = string.Empty;
                if (DataType != null)
                {
                    suffix = string.Format("^^{0}", DataType);
                }
                else if (Language != null)
                {
                    suffix = string.Format("@{0}", DataType);
                }

                return string.Format("\"{0}\"{1}", Literal, suffix);
            }

            if (IsUri)
            {
                return string.Format("<{0}>", Uri);
            }

            if (IsBlank)
            {
                return _blankNodeId.ToString(true);
            }

            throw new InvalidOperationException("Invalid node state");
        }

        /// <summary>Creates an <see cref="EntityId"/> for a <see cref="Node"/>.</summary>
        public EntityId ToEntityId()
        {
            if (IsBlank)
            {
                return _blankNodeId;
            }

            if (IsUri)
            {
                return new EntityId(Uri);
            }

            throw new InvalidOperationException("Cannot convert literal node to EntityId");
        }

        private bool Equals(Node other)
        {
            return _hashCode == other._hashCode;
        }

        private int CalculateHashCode()
        {
            unchecked
            {
                return 397 * (!IsLiteral ? _uri.AbsoluteUri.GetHashCode() :
                    (_literal.GetHashCode() ^ (_language != null ? _language.GetHashCode() : 0) ^ (_dataType != null ? _dataType.GetHashCode() : 0)));
            }
        }

        private class DebuggerViewProxy
        {
            private readonly string _displayString;

            public DebuggerViewProxy(Node node)
            {
                if (node.IsUri)
                {
                    _displayString = string.Format("<{0}>", node.Uri);
                }
                else if (node.IsLiteral)
                {
                    _displayString = string.Format("\"{0}\"", node.Literal);
                    if (!string.IsNullOrWhiteSpace(node.Language))
                    {
                        _displayString += string.Format("@{0}", node.Language);
                    }
                    else if (node.DataType != null)
                    {
                        _displayString += string.Format("^^<{0}>", node.DataType);
                    }
                }
                else
                {
                    _displayString = node._identifier;
                }
            }

            public string Value { get { return _displayString; } }
        }
    }
}