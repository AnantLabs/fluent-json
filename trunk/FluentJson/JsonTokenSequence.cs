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
using System.Collections.Generic;
using System.Text;

using FluentJson.Exceptions;

namespace FluentJson
{
    /// <summary>
    /// Represents a sequence of json tokens.
    /// </summary>
    public class JsonTokenSequence
    {
        private string _tokens;
        private int _zero = 0;

        private StringBuilder _sb;

        #region Constructors

        /// <summary>
        /// Constructs an empty token sequence.
        /// </summary>
        public JsonTokenSequence()
        {
            _tokens = string.Empty;
        }

        /// <summary>
        /// Constructs and populates the token sequence.
        /// </summary>
        /// <param name="value"></param>
        public JsonTokenSequence(string value)
        {
            if (value == null) throw new ArgumentNullException();
            _tokens = value;
        }

        /// <summary>
        /// Constructs and populates the token sequence.
        /// </summary>
        /// <param name="value"></param>
        public JsonTokenSequence(params JsonToken[] value)
        {
            _tokens = string.Empty;
            _prepareAppend();
            foreach (JsonToken token in value)
            {
                _sb.Append(token.ToString());
            }
        }

        #endregion

        /// <summary>
        /// Looks ahead in the remaining sequence and returns a (partial) json token.
        /// </summary>
        /// <returns></returns>
        public JsonToken Peek()
        {
            _prepareRead();
            _skipWhiteSpace();

            JsonToken token = JsonToken.Undefined;
            for (int length = 1; _zero + length <= _tokens.Length && (token.Type == JsonTokenType.Undefined || token.Type == JsonTokenType.Unknown); length++)
            {
                token = new JsonToken(_tokens.Substring(_zero, length));
            }

            return token;
        }

        /// <summary>
        /// Returns the first token in this sequence and moves forward.
        /// </summary>
        /// <returns></returns>
        public JsonToken Pop()
        {
            _prepareRead();
            _skipWhiteSpace();

            JsonToken token = JsonToken.Undefined;

            for (int length = 1; _zero + length <= _tokens.Length && (token.Type == JsonTokenType.Undefined || token.Type == JsonTokenType.Unknown); length++)
            {
                token = new JsonToken(_tokens.Substring(_zero, length));
            }

            _zero += token.Length;

            if (token.Type == JsonTokenType.String || token.Type == JsonTokenType.Number)
            {
                while (_zero < _tokens.Length && token.TryAppend(_tokens[_zero]))
                {
                    _zero++;
                }

                if (!token.IsValid())
                {
                    token = JsonToken.Undefined;
                }
            }

            return token;
        }

        /// <summary>
        /// Indicates if a token is available for reading.
        /// </summary>
        public bool TokenAvailable
        {
            get
            {
                return _zero < _tokens.Length;
            }
        }

        private void _skipWhiteSpace()
        {
            while (_zero < _tokens.Length && char.IsWhiteSpace(_tokens[_zero]))
            {
                _zero++;
            }
        }

        private void _prepareRead()
        {
            if (_sb != null)
            {
                _tokens += _sb.ToString();
                _sb = null;
            }
        }

        private void _prepareAppend()
        {
            if (_sb == null)
            {
                _sb = new StringBuilder();
            }
        }

        #region Operators

        public static JsonTokenSequence operator +(JsonTokenSequence c1, JsonToken c2)
        {
            c1._prepareAppend();
            c1._sb.Append(c2.ToString());

            return c1;
        }

        public static JsonTokenSequence operator +(JsonToken c1, JsonTokenSequence c2)
        {
            c2._prepareAppend();
            c2._sb.Append(c1.ToString());

            return c2;
        }

        public static JsonTokenSequence operator +(JsonTokenSequence c1, JsonTokenSequence c2)
        {
            c1._prepareAppend();
            c1._sb.Append(c2.ToString());

            return c1;
        }

        public static JsonTokenSequence operator -(JsonTokenSequence c1, JsonToken c2)
        {
            JsonToken t1 = c1.Pop();

            if (t1 == c2)
            {
                return c1;
            }

            throw new JsonException("Token '" + c2.ToString() + "' was expected. Wrong token '" + t1.ToString() + "'.", c1._tokens, c1._zero - t1.Length);
        }

        public static JsonTokenSequence operator -(JsonToken c1, JsonTokenSequence c2)
        {
            JsonToken t2 = c2.Pop();

            if (t2 == c1)
            {
                return c2;
            }

            throw new JsonException("", c2._tokens, c2._zero - t2.Length);
        }

        public static JsonTokenSequence operator -(JsonTokenSequence c1, JsonTokenSequence c2)
        {
            int p2 = c2._zero;

            while (c2.TokenAvailable)
            {
                JsonToken t1 = c1.Pop();
                JsonToken t2 = c2.Pop();

                if (t1 != t2)
                {
                    throw new JsonException("", c1._tokens, c1._zero - t1.Length);
                }
            }

            c2._zero = p2;
            return c1;
        }

        #endregion

        #region Object Overrides

        public override int GetHashCode()
        {
            _prepareRead();
            return _tokens.GetHashCode();
        }

        /// <summary>
        /// Returns the string representation of this token sequence.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            _prepareRead();
            return _tokens;
        }

        private class StructureState
        {
            internal bool IsEmpty = true;
        }

        /// <summary>
        /// Returns a json string formatted to improve human readability.
        /// </summary>
        /// <returns></returns>
        public string ToTidyString()
        {
            _prepareRead();
            _zero = 0;

            Stack<StructureState> structures = new Stack<StructureState>();
            int numSpaces = 0;

            StringBuilder tidy = new StringBuilder();
            JsonToken prev = JsonToken.Undefined;

            while (this.TokenAvailable)
            {
                JsonToken token = this.Pop();

                if (prev == JsonToken.NameSeperator)
                    tidy.Append(' ');

                if (token.Type == JsonTokenType.Structural)
                {
                    if (token == JsonToken.BeginArray || token == JsonToken.BeginObject)
                    {
                        structures.Push(new StructureState());
                        numSpaces += 3;
                    }
                    else if (token == JsonToken.EndArray || token == JsonToken.EndObject)
                    {
                        numSpaces -= 3;

                        if (!structures.Pop().IsEmpty)
                            // Newline
                            tidy.Append(_newLine(numSpaces));
                    }
                    else if (token == JsonToken.NameSeperator)
                        tidy.Append(' ');
                }
                else
                {
                    structures.Peek().IsEmpty = false;

                    if (prev != JsonToken.NameSeperator)
                        // Newline
                        tidy.Append(_newLine(numSpaces));
                        
                }

                tidy.Append(token.ToString());
                prev = token;
            }

            _zero = 0;
            return tidy.ToString();
        }

        private string _newLine(int numSpaces)
        {
            return "\n" + "".PadRight(numSpaces, ' ');
        }

        #endregion
    }
}
