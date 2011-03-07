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

using FluentJson.Mapping;

namespace FluentJson.Configuration
{
    public class JsonEncodingConfiguration<T> : JsonBaseConfiguration<T>
    {
        internal bool UseTidy { get; private set; }

        /// <summary>
        /// Enables or disables tidy encoding.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public JsonEncodingConfiguration<T> Tidy(bool value)
        {
            this.UseTidy = value;
            return this;
        }

        /// <summary>
        /// Returns a mapping expression for the root type.
        /// </summary>
        /// <param name="expression">The object mapping expression.</param>
        /// <returns>The configuration.</returns>
        new public JsonEncodingConfiguration<T> Map(Action<JsonObjectMapping<T>> expression)
        {
            return (JsonEncodingConfiguration<T>)base.Map(expression);
        }

        /// <summary>
        /// Returns a mapping expression for the type TObject.
        /// </summary>
        /// <typeparam name="TType">Type to map.</typeparam>
        /// <param name="expression">The object mapping expression.</param>
        /// <returns>The configuration.</returns>
        new public JsonEncodingConfiguration<T> MapType<TType>(Action<JsonObjectMapping<TType>> expression)
        {
            return (JsonEncodingConfiguration<T>)base.MapType<TType>(expression);
        }
    }
}
