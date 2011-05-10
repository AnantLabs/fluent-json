// Copyright (c) 2011, Adaptiv Design
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice, this
// list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation and/or
// other materials provided with the distribution.
//    * Neither the name of the <ORGANIZATION> nor the names of its contributors may
// be used to endorse or promote products derived from this software without specific
// prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
// NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections;
using System.Collections.Generic;

using FluentJson.Helpers;
using FluentJson.Mapping;

namespace FluentJson.Configuration
{
    /// <summary>
    /// Configuration for both encoding and decoding.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonConfiguration<T>
    {
        internal Dictionary<Type, JsonTypeMappingBase> Mappings { get; private set; }
        internal bool UsesParallelProcessing { get; private set; }

        internal JsonConfiguration()
        {
            this.Mappings = new Dictionary<Type, JsonTypeMappingBase>();
        }

        /// <summary>
        /// Automatically generates a configuration for the current type.
        /// </summary>
        /// <returns>The configuration.</returns>
        public JsonConfiguration<T> AutoGenerate()
        {
            // Reset
            this.Mappings = new Dictionary<Type, JsonTypeMappingBase>();

            Stack<Type> mapped = new Stack<Type>();
            Stack<Type> unmapped = new Stack<Type>();

            Type root = _resolveMapType(typeof(T));
            if (root != null)
            {
                unmapped.Push(root);
            }

            while (unmapped.Count > 0)
            {
                Type type = unmapped.Pop();
                mapped.Push(type);

                JsonTypeMappingBase mapping = (JsonTypeMappingBase)Activator.CreateInstance(typeof(JsonTypeMapping<>).MakeGenericType(type));
                _addMapping(mapping);

                mapping.AutoGenerate();

                foreach (JsonFieldMappingBase field in mapping.FieldMappings.Values)
                {
                    Type nested = _resolveMapType(field.DesiredType);
                    if (nested != null  && !mapped.Contains(nested) && !unmapped.Contains(nested))
                    {
                        unmapped.Push(nested);
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// Derives this configuration from an existing configuration.
        /// </summary>
        /// <param name="configuration">The configuration to derive from.</param>
        /// <returns>The configuration.</returns>
        public JsonTypeMapping<TType> GetMapping<TType>()
        {
            return (JsonTypeMapping<TType>)this.Mappings[typeof(TType)];
        }

        /// <summary>
        /// Derives this configuration from an existing configuration.
        /// </summary>
        /// <param name="configuration">The configuration to derive from.</param>
        /// <returns>The configuration.</returns>
        public JsonConfiguration<T> DeriveFrom(JsonConfiguration<T> configuration)
        {
            this.Mappings = new Dictionary<Type, JsonTypeMappingBase>();

            Dictionary<Type, JsonTypeMappingBase>.Enumerator enumerator = configuration.Mappings.GetEnumerator();
            while (enumerator.MoveNext())
            {
                _addMapping((JsonTypeMappingBase)enumerator.Current.Value.Clone());
            }

            return this;
        }

        /// <summary>
        /// Returns a mapping expression for the root type.
        /// </summary>
        /// <param name="expression">The object mapping expression.</param>
        /// <returns>The configuration.</returns>
        public JsonConfiguration<T> WithMapping(JsonTypeMappingBase mapping)
        {
            _addMapping(mapping);
            return this;
        }

        #if !NET20

        /// <summary>
        /// Returns a mapping expression for the type TObject.
        /// </summary>
        /// <typeparam name="TObject">Type to map.</typeparam>
        /// <param name="expression">The object mapping expression.</param>
        /// <returns>The configuration.</returns>
        public JsonConfiguration<T> MapType<TType>(Action<JsonTypeMapping<TType>> expression)
        {
            JsonTypeMapping<TType> mapping = null;
            if (!this.Mappings.ContainsKey(typeof(TType)))
            {
                mapping = new JsonTypeMapping<TType>();
                _addMapping(mapping);
            }
            else
            {
                mapping = (JsonTypeMapping<TType>)this.Mappings[typeof(TType)];
            }


            expression(mapping);
            return this;
        }

        #endif

        #if NET40
        /// <summary>
        /// Enable or disable parallel processing.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public JsonConfiguration<T> UseParallelProcessing(bool value)
        {
            this.UsesParallelProcessing = value;
            return this;
        }
        #endif

        private void _addMapping(JsonTypeMappingBase mapping)
        {
            Type type = mapping.GetType().GetGenericArguments()[0];
            if (!type.IsInterface)
            {
                if (!Mappings.ContainsKey(type))
                {
                    this.Mappings.Add(type, mapping);
                }
                else
                {
                    throw new Exception("A mapping for type '" + type.Name + "' already exists.");
                }
            }
            else
            {
                throw new Exception("Interfaces cannot be mapped.");
            }
        }

        private Type _resolveMapType(Type type)
        {
            Type result = type;

            if (type.IsGenericType)
            {
                if (TypeHelper.IsThreatableAs(type, typeof(IList<>)))
                {
                    result = type.GetGenericArguments()[0];
                }
                else if (TypeHelper.IsThreatableAs(type, typeof(IDictionary<,>)))
                {
                    result = type.GetGenericArguments()[1];
                }
            }
            else if (type.IsArray)
            {
                result = type.GetElementType();
            }

            if (_shouldMapType(result))
            {
                return result;
            }

            return null;
        }

        private bool _shouldMapType(Type type)
        {
            return !TypeHelper.IsBasic(type) && !TypeHelper.IsDictionary(type) && !TypeHelper.IsList(type) && type != typeof(object);
        }
    }
}