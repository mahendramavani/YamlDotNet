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
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.Converters;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.ObjectGraphTraversalStrategies;
using YamlDotNet.Serialization.ObjectGraphVisitors;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;

namespace YamlDotNet.Serialization
{
    public sealed class Serializer : ISerializer
    {
        private readonly IValueSerializer valueSerializer;

        /// <summary>
        /// Initializes a new instance of <see cref="Serializer" /> using the default configuration.
        /// </summary>
        /// <remarks>
        /// To customize the behavior of the serializer, use <see cref="SerializerBuilder" />.
        /// </remarks>
        public Serializer()
            : this(new SerializerBuilder().BuildValueSerializer())
        {
        }

        /// <remarks>
        /// This constructor is private to discourage its use.
        /// To invoke it, call the <see cref="FromValueSerializer"/> method.
        /// </remarks>
        private Serializer(IValueSerializer valueSerializer)
        {
            if (valueSerializer == null)
            {
                throw new ArgumentNullException("valueSerializer");
            }

            this.valueSerializer = valueSerializer;
        }

        /// <summary>
        /// Creates a new <see cref="Serializer" /> that uses the specified <see cref="IValueSerializer" />.
        /// This method is available for advanced scenarios. The preferred way to customize the bahavior of the
        /// deserializer is to use <see cref="SerializerBuilder" />.
        /// </summary>
        public static Serializer FromValueSerializer(IValueSerializer valueSerializer)
        {
            return new Serializer(valueSerializer);
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter" /> where to serialize the object.</param>
        /// <param name="graph">The object to serialize.</param>
        public void Serialize(TextWriter writer, object graph)
        {
            Serialize(new Emitter(writer), graph);
        }

        /// <summary>
        /// Serializes the specified object into a string.
        /// </summary>
        /// <param name="graph">The object to serialize.</param>
        public string Serialize(object graph)
        {
            using (var buffer = new StringWriter())
            {
                Serialize(buffer, graph);
                return buffer.ToString();
            }
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter" /> where to serialize the object.</param>
        /// <param name="graph">The object to serialize.</param>
        /// <param name="type">The static type of the object to serialize.</param>
        public void Serialize(TextWriter writer, object graph, Type type)
        {
            Serialize(new Emitter(writer), graph, type);
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="emitter">The <see cref="IEmitter" /> where to serialize the object.</param>
        /// <param name="graph">The object to serialize.</param>
        public void Serialize(IEmitter emitter, object graph)
        {
            if (emitter == null)
            {
                throw new ArgumentNullException("emitter");
            }

            EmitDocument(emitter, graph, null);
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="emitter">The <see cref="IEmitter" /> where to serialize the object.</param>
        /// <param name="graph">The object to serialize.</param>
        /// <param name="type">The static type of the object to serialize.</param>
        public void Serialize(IEmitter emitter, object graph, Type type)
        {
            if (emitter == null)
            {
                throw new ArgumentNullException("emitter");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            EmitDocument(emitter, graph, type);
        }

        private void EmitDocument(IEmitter emitter, object graph, Type type)
        {
            emitter.Emit(new StreamStart());
            emitter.Emit(new DocumentStart());

            valueSerializer.SerializeValue(emitter, graph, type);

            emitter.Emit(new DocumentEnd(true));
            emitter.Emit(new StreamEnd());
        }
    }
}