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
using System.Globalization;

using FluentJson.Helpers;

namespace FluentJson
{
    public enum JsonTokenType
    {
        Structural,
        String,
        Number,
        Literal,

        /// <summary>
        /// Internal usage
        /// </summary>
        Unknown,

        /// <summary>
        /// Default type
        /// </summary>
        Undefined
    }

    /// <summary>
    /// Represents a json token as defined in http://tools.ietf.org/html/rfc4627
    /// </summary>
    public struct JsonToken
    {
        private const string BEGIN_OBJECT = "{";
        private const string END_OBJECT = "}";
        private const string BEGIN_ARRAY = "[";
        private const string END_ARRAY = "]";
        private const string NAME_SEPERATOR = ":";
        private const string VALUE_SEPERATOR = ",";

        private const string NULL = "null";
        private const string FALSE = "false";
        private const string TRUE = "true";

        private const string NUMERICAL = "0123456789+-.eE";

        private const char DOUBLE_QUOTES = '\"';
        private const char MINUS_SIGN = '-';

        private string _utf;
        private JsonTokenType _type;

        /// <summary>
        /// Constructs a json token from the given utf string.
        /// </summary>
        /// <param name="utf"></param>
        public JsonToken(string utf)
        {
            _utf = utf;
            _type = JsonTokenType.Undefined;
        }

        /// <summary>
        /// Construct a json token from the given utf string and manually assigns the token type.
        /// </summary>
        /// <param name="utf"></param>
        /// <param name="type"></param>
        public JsonToken(string utf, JsonTokenType type)
        {
            _utf = utf;
            _type = type;
        }

        /// <summary>
        /// The type of token.
        /// </summary>
        public JsonTokenType Type
        {
            get
            {
                if (_type == JsonTokenType.Undefined && _utf.Length > 0)
                {
                    if (_utf == BEGIN_ARRAY || _utf == END_ARRAY || _utf == BEGIN_OBJECT || _utf == END_OBJECT || _utf == VALUE_SEPERATOR || _utf == NAME_SEPERATOR)
                    {
                        _type = JsonTokenType.Structural;
                    }
                    else if(_utf == NULL || _utf == FALSE || _utf == TRUE)
                    {
                        _type = JsonTokenType.Literal;
                    }
                    else if (char.IsDigit(_utf[0]) || _utf[0] == MINUS_SIGN)
                    {
                        _type = JsonTokenType.Number;
                    }
                    else if (_utf[0] == DOUBLE_QUOTES)
                    {
                        _type = JsonTokenType.String;
                    }
                    else
                    {
                        _type = JsonTokenType.Unknown;
                    }
                }

                return _type;
            }
        }

        /// <summary>
        /// Some tokens (string / number) can be of a variable length, this method tries to expand this token by appending the given char.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryAppend(char value)
        {
            if (this.Type == JsonTokenType.String)
            {
                if (!IsValid())
                {
                    _utf += value;
                    return true;
                }

                return false;
            }
            else if (this.Type == JsonTokenType.Number)
            {
                if (NUMERICAL.IndexOf(value) == -1)
                {
                    return false;
                }

                _utf += value;
                return true;
            }


            throw new Exception("Can only append to string and number tokens.");
        }

        /// <summary>
        /// Sees to it that this token is valid.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (this.Type == JsonTokenType.String)
            {
                if (_utf.Length < 2 || _utf[_utf.Length - 1] != DOUBLE_QUOTES || _utf[_utf.Length - 2] == '\\')
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The number of characters that make up this token.
        /// </summary>
        public int Length
        {
            get
            {
                return _utf.Length;
            }
        }

        /// <summary>
        /// Returns the actual .NET value for this token.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Value<T>()
        {
            return (T)Value(typeof(T));
        }

        /// <summary>
        /// Returns the actual .NET value for this token.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public object Value(Type expectedType)
        {
            object result = null;

            if (_type == JsonTokenType.Literal)
            {
                switch (_utf)
                {
                    case NULL: return null;
                    case TRUE: result = true; break;
                    case FALSE: result = false; break;
                }
            }
            else if (_type == JsonTokenType.Number)
            {
                result = double.Parse(_utf, NumberStyles.Any, CultureInfo.InvariantCulture);
            }
            else if (_type == JsonTokenType.String)
            {
                string str = string.Empty;

                for (int i = 1; i < _utf.Length - 1; i++)
                {
                    char c = _utf[i];

                    switch (c)
                    {
                        case '\\':

                            // At least one more char is expected.
                            if (_utf.Length - i < 2) throw new Exception("At least one escaped character was expected.");

                            char c2 = _utf[++i];
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
                                    if (_utf.Length - i < 6) throw new Exception("At least 4 more escaped character was expected.");

                                    string code = _utf[++i].ToString() + _utf[++i].ToString() + _utf[++i].ToString() + _utf[++i].ToString();

                                    try
                                    {
                                        str += char.ConvertFromUtf32(Convert.ToInt32(code, 16));
                                    }
                                    catch (Exception exception)
                                    {
                                        throw new Exception("Unrecognized unicode character.", exception);
                                    }

                                    break;

                                default:
                                    throw new Exception("Unrecognized escape sequence.");
                            }

                            break;

                        default:
                            str += c;
                            break;
                    }
                }

                result = str;
            }
            else
            {
                throw new Exception("This token is not a value token.");
            }

            Type type = result.GetType();
            if(!TypeHelper.IsThreatableAs(type, expectedType))
            {
                if (TypeHelper.IsNumerical(expectedType) && result is double)
                {
                    if (expectedType == typeof(byte)) return Convert.ToByte(result);
                    if (expectedType == typeof(sbyte)) return Convert.ToSByte(result);
                    if (expectedType == typeof(short)) return Convert.ToInt16(result);
                    if (expectedType == typeof(ushort)) return Convert.ToUInt16(result);
                    if (expectedType == typeof(int)) return Convert.ToInt32(result);
                    if (expectedType == typeof(uint)) return Convert.ToUInt32(result);
                    if (expectedType == typeof(long)) return Convert.ToInt64(result);
                    if (expectedType == typeof(ulong)) return Convert.ToUInt64(result);
                    if (expectedType == typeof(float)) return Convert.ToSingle(result);
                }
            }

            return result;
        }

        /// <summary>
        /// Parses the given object to a json token.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static public JsonToken Parse(object value)
        {
            if (value == null)
            {
                return JsonToken.Null;
            }

            if (value is string)
            {
                string parsed = string.Empty;

                parsed += DOUBLE_QUOTES;

                // Append character by character, and handle certain special characters.
                // As defined in http://www.ietf.org/rfc/rfc4627 section 2.5
                string str = (string)value;
                foreach (char c in str)
                {
                    switch (c)
                    {
                        case '"':
                            parsed += "\\\"";
                            break;
                        case '\\':
                            parsed += "\\\\";
                            break;
                        case '/':
                            parsed += "\\/";
                            break;
                        case '\b':
                            parsed += "\\b";
                            break;
                        case '\f':
                            parsed += "\\f";
                            break;
                        case '\n':
                            parsed += "\\n";
                            break;
                        case '\r':
                            parsed += "\\r";
                            break;
                        case '\t':
                            parsed += "\\t";
                            break;
                        default:
                            if (c < ' ' || c > 127)
                            {
                                // Get hexadecimal representation for current character
                                // \uXXXX format
                                parsed += "\\u" + ((uint)c).ToString("X4");
                            }
                            else
                            {
                                // Normal character, simply append.
                                parsed += c;
                            }
                            break;
                    }
                }

                parsed += DOUBLE_QUOTES;

                return new JsonToken(parsed, JsonTokenType.String);
            }

            if (value is bool)
            {
                return (bool)value ? JsonToken.True : JsonToken.False;
            }

            Type type = value.GetType();
            if (TypeHelper.IsNumerical(type))
            {
                string parsed = null;

                // Maximize precision when converting to string
                if (type == typeof(float)) parsed = ((float)value).ToString("R", CultureInfo.InvariantCulture);
                // Maximize precision when converting to string
                else if (type == typeof(double)) parsed = ((double)value).ToString("R", CultureInfo.InvariantCulture);
                else parsed = Convert.ToString(value, CultureInfo.InvariantCulture);

                return new JsonToken(parsed, JsonTokenType.Number);
            }

            throw new Exception("Could not parse object of type '" + type.FullName + "'.");
        }

        /// <summary>
        /// Parses the given object to a json token. If the parsed token does not match the expected type,
        /// an exception will be thrown.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="expectedType"></param>
        /// <returns></returns>
        static public JsonToken Parse(object value, JsonTokenType expectedType)
        {
            JsonToken parsed = Parse(value);
            if (parsed.Type != expectedType)
            {
                throw new Exception("Parsed type '" + parsed.Type + "' does not equal expected type '" + expectedType + "'.");
            }

            return parsed;
        }

        #region Operators

        public static JsonToken operator +(JsonToken c1, JsonToken c2)
        {
            return new JsonToken(c1._utf + c2._utf);
        }

        public static bool operator ==(JsonToken c1, JsonToken c2)
        {
            return c1._utf == c2._utf;
        }

        public static bool operator !=(JsonToken c1, JsonToken c2)
        {
            return c1._utf != c2._utf;
        }

        #endregion

        #region Object Overrides

        public override bool Equals(object obj)
        {
            if (obj is JsonToken) return (JsonToken)obj == this;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (_utf != null)
            {
                return _utf.GetHashCode();
            }
            else
            {
                return 0;
            }
        }

        public override string ToString()
        {
            if (_utf != null)
            {
                return _utf;
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion

        #region Structural Tokens

        public static JsonToken BeginObject
        {
            get
            {
                return new JsonToken(BEGIN_OBJECT, JsonTokenType.Structural);
            }
        }

        public static JsonToken EndObject
        {
            get
            {
                return new JsonToken(END_OBJECT, JsonTokenType.Structural);
            }
        }

        public static JsonToken BeginArray
        {
            get
            {
                return new JsonToken(BEGIN_ARRAY, JsonTokenType.Structural);
            }
        }

        public static JsonToken EndArray
        {
            get
            {
                return new JsonToken(END_ARRAY, JsonTokenType.Structural);
            }
        }

        public static JsonToken ValueSeperator
        {
            get
            {
                return new JsonToken(VALUE_SEPERATOR, JsonTokenType.Structural);
            }
        }

        public static JsonToken NameSeperator
        {
            get
            {
                return new JsonToken(NAME_SEPERATOR, JsonTokenType.Structural);
            }
        }

        public static JsonToken Null
        {
            get
            {
                return new JsonToken(NULL, JsonTokenType.Literal);
            }
        }

        public static JsonToken False
        {
            get
            {
                return new JsonToken(FALSE, JsonTokenType.Literal);
            }
        }

        public static JsonToken True
        {
            get
            {
                return new JsonToken(TRUE, JsonTokenType.Literal);
            }
        }

        public static JsonToken Undefined
        {
            get
            {
                return new JsonToken(string.Empty);
            }
        }

        #endregion
    }
}
