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
using System.Globalization;

using FluentJson.Exceptions;

namespace FluentJson
{
    /// <summary>
    /// Actual json decoder.
    /// </summary>
    internal class JsonDecoder : IJsonDecoder
    {
        // Defines all possible numerical characters to be matched.
        private const string _numerical = "0123456789+-.eE";

        delegate bool JsonTokenDecoder(ref string value, ref object decoded);
        private List<JsonTokenDecoder> _decoders;

        #region Configuration

        public bool IsStrict { get; set; }

        #endregion

        #region Constructors

        internal JsonDecoder()
        {
            _decoders = new List<JsonTokenDecoder>();

            _decoders.Add(_decodeElementSeperator);
            _decoders.Add(_decodePairSeperator);

            _decoders.Add(_decodeNull);
            _decoders.Add(_decodeString);
            _decoders.Add(_decodeNumber);
            _decoders.Add(_decodeBoolean);
            _decoders.Add(_decodeArray);
            _decoders.Add(_decodeArrayEnd);

            _decoders.Add(_decodeObject);
            _decoders.Add(_decodeObjectEnd);
        }

        #endregion

        public object Decode(string json)
        {
            if (json == null) throw new ArgumentNullException();

            object decoded = null;
            _decodeNextToken(ref json, ref decoded);

            return decoded;
        }

        private JsonTokenDecoder _decodeNextToken(ref string json, ref object decoded)
        {
            // Remove possible whitespace located in front of token.
            json = json.TrimStart();

            // Match current token against decoders.
            foreach (JsonTokenDecoder decoder in _decoders)
            {
                if (decoder(ref json, ref decoded))
                {
                    return decoder;
                }
            }

            throw new JsonDecodingException("Token could not be decoded.", json);
        }

        #region Decoders

        private bool _decodeNull(ref string json, ref object decoded)
        {
            if (!json.StartsWith(JsonTokenType.Null)) return false;

            decoded = null;
            json = json.Substring(JsonTokenType.Null.Length);

            return true;
        }

        private bool _decodeString(ref string json, ref object decoded)
        {
            if (json[0] != JsonTokenType.String) return false;

            json = json.Substring(1);
            string str = "";

            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];

                if (c == JsonTokenType.String)
                {
                    // End of string has been reached.
                    json = json.Substring(i + 1);
                    decoded = str;
                    return true;
                }

                switch (c)
                {
                    case '\\':
                        
                        // At least one more char is expected.
                        if (json.Length - i < 2) throw new JsonDecodingException("At least one escaped character was expected.", json);

                        char c2 = json[++i];
                        switch (c2)
                        {
                            case '\"':
                                str += '\"';
                                break;
                            case '\\':
                                str += '\\';
                                break;
                            case '/':
                                str += "/";
                                break;
                            case 'b':
                                str += "\b";
                                break;
                            case 'f':
                                str += "\f";
                                break;
                            case 'n':
                                str += "\n";
                                break;
                            case 'r':
                                str += "\r";
                                break;
                            case 't':
                                str += "\t";
                                break;
                            case 'u':

                                // At least 4 more chars are expected.
                                if (json.Length - i < 6) throw new JsonDecodingException("At least 4 more escaped character was expected.", json);

                                string code = json[++i].ToString() + json[++i].ToString() + json[++i].ToString() + json[++i].ToString();

                                try
                                {
                                    str += char.ConvertFromUtf32(Convert.ToInt32(code, 16));
                                }
                                catch (Exception exception)
                                {
                                    throw new JsonDecodingException("Unrecognized unicode character.", json);
                                }

                                break;
                            default:

                                throw new JsonDecodingException("Unrecognized escape sequence.", json);
                        }

                        break;

                    default:
                        str += c;
                        break;
                }
            }

            throw new JsonDecodingException("String is not closed properly.", json);
        }

        private bool _decodeNumber(ref string json, ref object decoded)
        {
            if (!Char.IsDigit(json[0]) && json[0] != '-') return false;

            int length = 1;
            while (length < json.Length && _numerical.IndexOf(json[length]) != -1)
            {
                length++;
            }

            double number = 0;
            if (double.TryParse(json.Substring(0, length), NumberStyles.Number, CultureInfo.InvariantCulture, out number))
            {
                decoded = number;
                json = json.Substring(length);
                return true;
            }

            throw new JsonDecodingException("What appeared to be a number could not be parsed as a number.", json);
        }

        private bool _decodeBoolean(ref string json, ref object decoded)
        {
            if (json.StartsWith(JsonTokenType.False))
            {
                decoded = false;
                json = json.Substring(JsonTokenType.False.Length);
                return true;
            }
            else if (json.StartsWith(JsonTokenType.True))
            {
                decoded = true;
                json = json.Substring(JsonTokenType.True.Length);
                return true;
            }

            return false;
        }

        private bool _decodeArray(ref string json, ref object decoded)
        {
            if (json[0] != JsonTokenType.ArrayStart) return false;

            IList list = new ArrayList();
            decoded = list;

            json = json.Substring(1);
            while (json.Length > 0)
            {
                object element = null;
                JsonTokenDecoder  decoder = _decodeNextToken(ref json, ref element);
                if (decoder == _decodeArrayEnd)
                {
                    return true;
                }
                else if (list.Count > 0)
                {
                    if (decoder != _decodeElementSeperator)
                    {
                        throw new JsonDecodingException("An element seperator was expected.", json);
                    }

                    _decodeNextToken(ref json, ref element);
                }

                if (element is char)
                {
                    throw new JsonDecodingException("An element was expected.", json);
                }

                list.Add(element);
            }

            throw new JsonDecodingException("Array is not closed properly.", json);
        }

        private bool _decodeArrayEnd(ref string json, ref object decoded)
        {
            if (json[0] != JsonTokenType.ArrayEnd) return false;

            json = json.Substring(1);
            decoded = JsonTokenType.ArrayEnd;

            return true;
        }

        private bool _decodeObject(ref string json, ref object decoded)
        {
            if (json[0] != JsonTokenType.ObjectStart) return false;

            Dictionary<string, object> dict = new Dictionary<string, object>();
            decoded = dict;

            json = json.Substring(1);
            while (json.Length > 0)
            {
                object fieldName = null;
                object fieldValue = null;

                JsonTokenDecoder decoder = _decodeNextToken(ref json, ref fieldName);
                if (decoder == _decodeObjectEnd)
                {
                    return true;
                }
                else if (dict.Count > 0)
                {
                    if (decoder != _decodeElementSeperator)
                    {
                        throw new JsonDecodingException("An element seperator was expected.", json);
                    }

                    decoder = _decodeNextToken(ref json, ref fieldName);
                }

                if (decoder != _decodeString)
                {
                    throw new JsonDecodingException("A field name (a string) was expected.", json);
                }

                object pairSeperator = null;
                decoder = _decodeNextToken(ref json, ref pairSeperator);
                if (decoder != _decodePairSeperator)
                {
                    throw new JsonDecodingException("A pair seperator was expected.", json);
                }

                _decodeNextToken(ref json, ref fieldValue);

                if (fieldValue is char)
                {
                    throw new JsonDecodingException("A field value was expected.", json);
                }

                dict.Add((string)fieldName, fieldValue);
            }

            throw new JsonDecodingException("Object is not closed properly.", json);
        }

        private bool _decodeObjectEnd(ref string json, ref object decoded)
        {
            if (json[0] != JsonTokenType.ObjectEnd) return false;

            json = json.Substring(1);
            decoded = JsonTokenType.ObjectEnd;

            return true;
        }

        private bool _decodeElementSeperator(ref string json, ref object decoded)
        {
            if (json[0] != JsonTokenType.ElementSeperator) return false;

            json = json.Substring(1);
            decoded = JsonTokenType.ElementSeperator;

            return true;
        }

        private bool _decodePairSeperator(ref string json, ref object decoded)
        {
            if (json[0] != JsonTokenType.PairSeperator) return false;

            json = json.Substring(1);
            decoded = JsonTokenType.PairSeperator;
            json = json.Substring(1);

            return true;
        }

        #endregion
    }
}
