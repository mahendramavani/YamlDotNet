//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using YamlDotNet.Core;

namespace YamlDotNet.RepresentationModel
{
    /// <summary>
    /// Manages the state of a <see cref="YamlDocument" /> while it is loading.
    /// </summary>
    internal class DocumentLoadingState
    {
        private readonly IDictionary<string, YamlNode> anchors = new Dictionary<string, YamlNode>();
        private readonly IList<YamlNode> nodesWithUnresolvedAliases = new List<YamlNode>();

        /// <summary>
        /// Adds the specified node to the anchor list.
        /// </summary>
        /// <param name="node">The node.</param>
        public void AddAnchor(YamlNode node)
        {
            if (node.Anchor == null)
            {
                throw new ArgumentException("The specified node does not have an anchor");
            }

            if (anchors.ContainsKey(node.Anchor))
            {
                anchors[node.Anchor] = node;
            }
            else
            {
                anchors.Add(node.Anchor, node);
            }
        }

        /// <summary>
        /// Gets the node with the specified anchor.
        /// </summary>
        /// <param name="anchor">The anchor.</param>
        /// <param name="throwException">if set to <c>true</c>, the method should throw an exception if there is no node with that anchor.</param>
        /// <param name="start">The start position.</param>
        /// <param name="end">The end position.</param>
        /// <returns></returns>
        [Obsolete("Please use GetNode(string, Mark, Mark) when throwException = true or TryGetNode(string, Node) when throwException = false")]
        public YamlNode? GetNode(string anchor, bool throwException, Mark start, Mark end)
        {
            return throwException
                ? GetNode(anchor, start, end)
                : TryGetNode(anchor, out var node) ? node : null;
        }

        /// <summary>
        /// Gets the node with the specified anchor.
        /// </summary>
        /// <param name="anchor">The anchor.</param>
        /// <param name="start">The start position.</param>
        /// <param name="end">The end position.</param>
        /// <returns></returns>
        /// <exception cref="AnchorNotFoundException">if there is no node with that anchor.</exception>
        public YamlNode GetNode(string anchor, Mark start, Mark end)
        {
            if (anchors.TryGetValue(anchor, out var target))
            {
                return target;
            }
            else
            {
                throw new AnchorNotFoundException(start, end, $"The anchor '{anchor}' does not exists");
            }
        }

        /// <summary>
        /// Gets the node with the specified anchor.
        /// </summary>
        /// <param name="anchor">The anchor.</param>
        /// <param name="node">The node that was retrieved.</param>
        /// <returns>true if the anchor was found; otherwise false.</returns>
        public bool TryGetNode(string anchor, [NotNullWhen(true)] out YamlNode? node)
        {
            return anchors.TryGetValue(anchor, out node);
        }

        /// <summary>
        /// Adds the specified node to the collection of nodes with unresolved aliases.
        /// </summary>
        /// <param name="node">
        /// The <see cref="YamlNode"/> that has unresolved aliases.
        /// </param>
        public void AddNodeWithUnresolvedAliases(YamlNode node)
        {
            nodesWithUnresolvedAliases.Add(node);
        }

        /// <summary>
        /// Resolves the aliases that could not be resolved while loading the document.
        /// </summary>
        public void ResolveAliases()
        {
            foreach (var node in nodesWithUnresolvedAliases)
            {
                node.ResolveAliases(this);
            }
        }
    }
}