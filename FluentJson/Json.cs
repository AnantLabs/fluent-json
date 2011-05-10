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
using FluentJson.Processing;

namespace FluentJson
{
    /// <summary>
    /// 
    /// </summary>
    public class Json
    {
        /// <summary>
        /// Encodes any value to a json string.
        /// </summary>
        /// <param name="value">Value to encode.</param>
        /// <returns>A json string.</returns>
        static public string Encode(object value)
        {
            return new JsonEncoder<object>(new JsonEncodingConfiguration<object>()).Encode(value);
        }

        /// <summary>
        /// Encodes a value of type T to a json string. Type T will be automatically mapped as best as possible.
        /// </summary>
        /// <param name="value">Value to encode.</param>
        /// <returns>A json string.</returns>
        static public string EncodeType<T>(T value)
        {
            return EncoderFor<T>().Encode(value);
        }

        #if !NET20

        /// <summary>
        /// Returns a json encoder for the specified configuration.
        /// </summary>
        /// <typeparam name="T">Type to encode.</typeparam>
        /// <param name="expression">Configuration expression.</param>
        /// <returns>An encoder for type T.</returns>
        static public JsonEncoder<T> EncoderFor<T>(Action<JsonEncodingConfiguration<T>> expression)
        {
            JsonEncodingConfiguration<T> configuration = new JsonEncodingConfiguration<T>();
            expression(configuration);

            return new JsonEncoder<T>(configuration);
        }

        #endif

        /// <summary>
        /// Returns a json encoder for the specified type. Type T will be automatically mapped as best as possible.
        /// </summary>
        /// <typeparam name="T">Type to encode.</typeparam>
        /// <returns>An encoder for type T.</returns>
        static public JsonEncoder<T> EncoderFor<T>()
        {
            JsonEncodingConfiguration<T> configuration = new JsonEncodingConfiguration<T>();
            configuration.AutoGenerate();

            return new JsonEncoder<T>(configuration);
        }

        /// <summary>
        /// Decodes a json string.
        /// </summary>
        /// <param name="json">Json string to decode.</param>
        /// <returns>The decoded value.</returns>
        static public object Decode(string json)
        {
            return new JsonDecoder<object>(new JsonDecodingConfiguration<object>()).Decode(json);
        }

        /// <summary>
        /// Decodes a json string to type T. Type T will be automatically mapped as best as possible.
        /// </summary>
        /// <param name="json">Json string to decode.</param>
        /// <returns>The decoded value.</returns>
        static public T DecodeType<T>(string json)
        {
            return DecoderFor<T>().Decode(json);
        }

        #if !NET20

        /// <summary>
        ///  Returns a json decoder for the specified configuration.
        /// </summary>
        /// <typeparam name="T">Type to decode.</typeparam>
        /// <param name="expression">Configuration expression.</param>
        /// <returns>A decoder for type T.</returns>
        static public JsonDecoder<T> DecoderFor<T>(Action<JsonDecodingConfiguration<T>> expression)
        {
            JsonDecodingConfiguration<T> configuration = new JsonDecodingConfiguration<T>();
            expression(configuration);

            return new JsonDecoder<T>(configuration);
        }

        #endif

        /// <summary>
        ///  Returns a json decoder for the specified configuration. Type T will be automatically mapped as best as possible.
        /// </summary>
        /// <typeparam name="T">Type to decode.</typeparam>
        /// <returns>A decoder for type T.</returns>
        static public JsonDecoder<T> DecoderFor<T>()
        {
            JsonDecodingConfiguration<T> configuration = new JsonDecodingConfiguration<T>();
            configuration.AutoGenerate();

            return new JsonDecoder<T>(configuration);
        }

        /// <summary>
        /// Returns an empty base configuration for the specified type T.
        /// </summary>
        /// <typeparam name="T">Type to create configuration for.</typeparam>
        /// <returns>A configuration for type T.</returns>
        static public JsonConfiguration<T> ConfigurationFor<T>()
        {
            JsonConfiguration<T> configuration = new JsonConfiguration<T>();
            return configuration;
        }

        /// <summary>
        /// Returns an empty encoding configuration for the specified type T.
        /// </summary>
        /// <typeparam name="T">Type to create configuration for.</typeparam>
        /// <returns>A configuration for type T.</returns>
        static public JsonEncodingConfiguration<T> EncodingConfigurationFor<T>()
        {
            JsonEncodingConfiguration<T> configuration = new JsonEncodingConfiguration<T>();
            return configuration;
        }

        /// <summary>
        /// Returns an empty decoding configuration for the specified type T.
        /// </summary>
        /// <typeparam name="T">Type to create configuration for.</typeparam>
        /// <returns>A configuration for type T.</returns>
        static public JsonDecodingConfiguration<T> DecodingConfigurationFor<T>()
        {
            JsonDecodingConfiguration<T> configuration = new JsonDecodingConfiguration<T>();
            return configuration;
        }
    }

    /// <summary>
    /// Defines a json encoder for type T.
    /// </summary>
    /// <typeparam name="T">Type to encode.</typeparam>
    public class JsonEncoder<T>
    {
        private JsonEncodingConfiguration<T> _configuration;

        internal JsonEncoder(JsonEncodingConfiguration<T> configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Encodes a value to a json string.
        /// </summary>
        /// <param name="value">Value of type T to encode.</param>
        /// <returns>A json string.</returns>
        public string Encode(T value)
        {
            return new EncodingProcess<T>(_configuration).Encode(value);
        }
    }

    /// <summary>
    /// Defines a json decoder for type T.
    /// </summary>
    /// <typeparam name="T">Type to decode.</typeparam>
    public class JsonDecoder<T>
    {
        private JsonDecodingConfiguration<T> _configuration;

        internal JsonDecoder(JsonDecodingConfiguration<T> configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Decodes a json string.
        /// </summary>
        /// <param name="json">Json string to decode.</param>
        /// <returns>The decoded value of type T.</returns>
        public T Decode(string json)
        {
            return new DecodingProcess<T>(_configuration).Decode(json);
        }
    }
}

