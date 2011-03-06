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
    /// Json encoder.
    /// </summary>
    internal class JsonEncoder : IJsonEncoder
    {
        internal const char CHAR_SPACE = ' ';
        internal const char CHAR_LINE_BREAK = '\n';
        internal const char CHAR_TAB = '\t';

        delegate bool JsonTokenEncoder(object value, ref string encoded, JsonWhiteSpaceState whiteSpace);
        private List<JsonTokenEncoder> _encoders;

        #region Configuration

        public bool IsTidy { get; set; }

        #endregion

        #region Constructors

        internal JsonEncoder()
        {
            _encoders = new List<JsonTokenEncoder>();

            // Most common types should be on top.
            // !! Null encoder needs to be on top, prevents the need for further null checking.

            _encoders.Add(_encodeNull);
            _encoders.Add(_encodeString);
            _encoders.Add(_encodeNumber);
            _encoders.Add(_encodeBoolean);
            _encoders.Add(_encodeArray);
            _encoders.Add(_encodeObject);
        }

        #endregion

        public string Encode(object value)
        {
            string json = "";

            // Initiate recursion
            _encodeNextToken(value, ref json, new JsonWhiteSpaceState());
            
            return json;
        }

        private JsonTokenEncoder _encodeNextToken(object value, ref string json, JsonWhiteSpaceState whiteSpace)
        {
            // Match current value against encoders.
            foreach (JsonTokenEncoder encoder in _encoders)
            {
                if (encoder(value, ref json, whiteSpace)) return encoder;
            }

            throw new JsonEncodingException("Value could not be encoded.", value);
        }

        #region Encoders

        private bool _encodeNull(object value, ref string json, JsonWhiteSpaceState whiteSpace)
        {
            if (value != null) return false;

            json += JsonTokenType.Null;

            return true;
        }

        private bool _encodeString(object value, ref string json, JsonWhiteSpaceState whiteSpace)
        {
            if (!(value is String)) return false;

            // Begin string
            json += JsonTokenType.String;

            // Append character by character, and handle certain special characters.
            // As defined in http://www.ietf.org/rfc/rfc4627 section 2.5
            string str = (string)value;
            foreach(char c in str)
            {
                switch (c)
                {
                    case '"':
                        json += "\\\"";
                        break;
                    case '\\':
                        json += "\\\\";
                        break;
                    case '/':
                        json += "\\/";
                        break;
                    case '\b':
                        json += "\\b";
                        break;
                    case '\f':
                        json += "\\f";
                        break;
                    case '\n':
                        json += "\\n";
                        break;
                    case '\r':
                        json += "\\r";
                        break;
                    case '\t':
                        json += "\\t";
                        break;
                    default:
                        if (c < ' ' || c > 127)
                        {
                            // Get hexadecimal representation for current character
                            // \uXXXX format
                            json += "\\u" + ((uint)c).ToString("X4");
                        }
                        else
                        {
                            // Normal character, simply append.
                            json += c;
                        }
                        break;
                }

            }

            // Close string
            json += JsonTokenType.String;

            return true;
        }

        private bool _encodeNumber(object value, ref string json, JsonWhiteSpaceState whiteSpace)
        {
            double parsed;
            if (!double.TryParse(value.ToString(), out parsed)) return false;

            json += Convert.ToString(parsed, CultureInfo.InvariantCulture);

            return true;
        }

        private bool _encodeBoolean(object value, ref string json, JsonWhiteSpaceState whiteSpace)
        {
            if (!(value is bool)) return false;

            json += (bool)value ? JsonTokenType.True : JsonTokenType.False;

            return true;
        }

        private bool _encodeArray(object value, ref string json, JsonWhiteSpaceState whiteSpace)
        {
            if (!(value is IList)) return false;

            IList list = (IList)value;

            // Begin array
            json += JsonTokenType.ArrayStart;

            // Tidy printing
            if (this.IsTidy)
            {
                json += CHAR_LINE_BREAK;
                whiteSpace.BeginInset(0, 2);
            }

            for (int i = 0; i < list.Count; i++)
            {
                // Tidy printing
                if (this.IsTidy)
                {
                    json += whiteSpace.getWhiteSpace();
                    whiteSpace.BeginInset(0, 1);
                }

                // Encode child node
                _encodeNextToken(list[i], ref json, whiteSpace);

                // Append seperator for next child node
                if (i + 1 < list.Count)
                {
                    json += JsonTokenType.ElementSeperator;
                }

                // Tidy printing
                if (this.IsTidy)
                {
                    json += CHAR_LINE_BREAK;
                    whiteSpace.EndInset();
                }
            }

            // Tidy printing
            if (this.IsTidy)
            {
                whiteSpace.EndInset();
                json += whiteSpace.getWhiteSpace();
            }

            // End array
            json += JsonTokenType.ArrayEnd;

            return true;
        }

        private bool _encodeObject(object value, ref string json, JsonWhiteSpaceState whiteSpace)
        {
            if (!(value is IDictionary)) return false;

            IDictionary dict = (IDictionary)value;

            // Begin object
            json += JsonTokenType.ObjectStart;

            // Tidy printing
            if (this.IsTidy)
            {
                json += CHAR_LINE_BREAK;
                whiteSpace.BeginInset(0, 2);
            }

            int i = 0;

            IDictionaryEnumerator enumerator = dict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                // Tidy printing
                if (this.IsTidy)
                {
                    json += whiteSpace.getWhiteSpace() + CHAR_SPACE;
                }

                // Encode child node field
                if (_encodeNextToken(enumerator.Key, ref json, whiteSpace) != _encodeString)
                {
                    // The string encoder should have been used.
                    // Since it wasn't, this is not a string.
                    throw new JsonEncodingException("Invalid dictionary entry. A String based key was expected.", value);
                }

                // Tidy printing
                if (this.IsTidy)
                {
                    json += CHAR_SPACE;
                }

                // Append pair seperator
                json += JsonTokenType.PairSeperator;

                // Tidy printing
                if (this.IsTidy)
                {
                    json += CHAR_SPACE;
                    whiteSpace.BeginInset(0, (enumerator.Key as string).Length + 6);
                }

                // Encode child node value
                _encodeNextToken(enumerator.Value, ref json, whiteSpace);

                // Append seperator for next child node
                if (i + 1 < dict.Count)
                {
                    json += JsonTokenType.ElementSeperator;
                }

                // Tidy printing
                if (this.IsTidy)
                {
                    json += CHAR_LINE_BREAK;
                    whiteSpace.EndInset();
                }

                i++;
            }

            // Tidy printing
            if (this.IsTidy)
            {
                whiteSpace.EndInset();
                json += whiteSpace.getWhiteSpace();
            }

            // End object
            json += JsonTokenType.ObjectEnd;

            return true;
        }

        #endregion
    }

    class JsonWhiteSpaceState
    {
        internal int TotalTabs { get; private set; }
        private List<int> _tabInsets;

        internal int TotalSpaces { get; private set; }
        private List<int> _spaceInsets;

        internal JsonWhiteSpaceState()
        {
            this.TotalTabs = 0;
            this.TotalSpaces = 0;

            _tabInsets = new List<int>();
            _spaceInsets = new List<int>();
        }

        internal void BeginInset(int numTabs, int numSpaces)
        {
            _tabInsets.Add(numTabs);
            _spaceInsets.Add(numSpaces);

            this.TotalTabs += numTabs;
            this.TotalSpaces += numSpaces;
        }

        internal void EndInset()
        {
            this.TotalTabs -= _tabInsets[_tabInsets.Count - 1];
            this.TotalSpaces -= _spaceInsets[_spaceInsets.Count - 1];

            _tabInsets.RemoveAt(_tabInsets.Count - 1);
            _spaceInsets.RemoveAt(_spaceInsets.Count - 1);
        }

        internal string getWhiteSpace()
        {
            return getWhiteSpace(this.TotalTabs, this.TotalSpaces);
        }

        internal string getWhiteSpace(int numTabs, int numSpaces)
        {
            return _repeatChar(JsonEncoder.CHAR_TAB, numTabs) + _repeatChar(JsonEncoder.CHAR_SPACE, numSpaces);
        }

        private string _repeatChar(char c, int count)
        {
            return string.Empty.PadLeft(count, c);
        }
    }
}
