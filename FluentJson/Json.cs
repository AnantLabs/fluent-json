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

using FluentJson.Configuration;
using FluentJson.Mapping;

namespace FluentJson
{
    /// <summary>
    /// 
    /// </summary>
    public class Json
    {
        static private JsonEncoder _defaultEncoder;
        static private JsonDecoder _defaultDecoder;

        /// <summary>
        /// Encodes a value to a json string.
        /// </summary>
        /// <param name="value">Value to encode.</param>
        /// <returns>A json string.</returns>
        static public string Encode(object value)
        {
            if(_defaultEncoder == null) _defaultEncoder = new JsonEncoder();
            return _defaultEncoder.Encode(value);
        }

        /// <summary>
        /// Returns a json encoder for the specified configuration.
        /// </summary>
        /// <typeparam name="T">Type to encode.</typeparam>
        /// <param name="expression">Configuration expression.</param>
        /// <returns>An encoder for type T.</returns>
        static public IJsonEncoder<T> EncoderFor<T>(Action<JsonEncodingConfiguration<T>> expression)
        {
            JsonEncodingConfiguration<T> configuration = new JsonEncodingConfiguration<T>();
            expression(configuration);

            return new MappedEncoder<T>(configuration);
        }

        /// <summary>
        /// Decodes a json string.
        /// </summary>
        /// <param name="json">Json string to decode.</param>
        /// <returns>The decoded value.</returns>
        static public object Decode(string json)
        {
            if(_defaultDecoder == null) _defaultDecoder = new JsonDecoder();
            return _defaultDecoder.Decode(json);
        }

        /// <summary>
        ///  Returns a json decoder for the specified configuration.
        /// </summary>
        /// <typeparam name="T">Type to decode.</typeparam>
        /// <param name="expression">Configuration expression.</param>
        /// <returns>A decoder for type T.</returns>
        static public IJsonDecoder<T> DecoderFor<T>(Action<JsonDecodingConfiguration<T>> expression)
        {
            JsonDecodingConfiguration<T> configuration = new JsonDecodingConfiguration<T>();
            expression(configuration);

            return new MappedDecoder<T>(configuration);
        }
    }

    /// <summary>
    /// Defines a json encoder.
    /// </summary>
    public interface IJsonEncoder
    {
        /// <summary>
        /// Encodes a value to a json string.
        /// </summary>
        /// <param name="value">Value to encode.</param>
        /// <returns>A json string.</returns>
        string Encode(object value);
    }

    /// <summary>
    /// Defines a json encoder for type T.
    /// </summary>
    /// <typeparam name="T">Type to encode.</typeparam>
    public interface IJsonEncoder<T>
    {
        /// <summary>
        /// Encodes a value to a json string.
        /// </summary>
        /// <param name="value">Value of type T to encode.</param>
        /// <returns>A json string.</returns>
        string Encode(T value);
    }

    /// <summary>
    /// Defines a json decoder.
    /// </summary>
    public interface IJsonDecoder
    {
        /// <summary>
        /// Decodes a json string.
        /// </summary>
        /// <param name="json">Json string to decode.</param>
        /// <returns>The decoded value.</returns>
        object Decode(string json);
    }

    /// <summary>
    /// Defines a json decoder for type T.
    /// </summary>
    /// <typeparam name="T">Type to decode.</typeparam>
    public interface IJsonDecoder<T>
    {
        /// <summary>
        /// Decodes a json string.
        /// </summary>
        /// <param name="json">Json string to decode.</param>
        /// <returns>The decoded value of type T.</returns>
        T Decode(string json);
    }
}

