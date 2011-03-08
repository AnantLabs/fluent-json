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

#if !NET20

using System;
using System.Linq.Expressions;

using System.Reflection;

namespace FluentJson.Mapping
{
    public abstract class JsonFieldMappingBase
    {
        internal string JsonObjectField { get; set; }
        internal Type DesiredType { get; set; }

        abstract internal object Encode(object value);
        abstract internal object Decode(object value);
    }

    public class JsonFieldMapping<T> : JsonFieldMappingBase
    {
        private MemberInfo _memberInfo;
        private Delegate _decodeAs;
        private Delegate _encodeAs;

        internal JsonFieldMapping(MemberInfo memberInfo)
        {
            _memberInfo = memberInfo;
            this.JsonObjectField = memberInfo.Name;

            if (typeof(T) == typeof(object))
            {
                if (memberInfo is PropertyInfo)
                {
                    this.DesiredType = (memberInfo as PropertyInfo).PropertyType;
                }
                else if (memberInfo is FieldInfo)
                {
                    this.DesiredType = (memberInfo as FieldInfo).FieldType;
                }
            }
            else
            {
                this.DesiredType = typeof(T);
            }
        }

        internal JsonFieldMapping(MemberInfo memberInfo, string jsonObjectField) : this (memberInfo)
        {
            this.JsonObjectField = jsonObjectField;
        }

        public JsonFieldMapping<T> To(string jsonObjectField)
        {
            this.JsonObjectField = jsonObjectField;
            return this;
        }

        public JsonFieldMapping<T> DecodeAs<TDecode>(Expression<Func<TDecode, T>> expression)
        {
            this.DesiredType = typeof(TDecode);
            _decodeAs = expression.Compile();

            return this;
        }

        public JsonFieldMapping<T> EncodeAs<TEncode>(Expression<Func<T, TEncode>> expression)
        {
            this.DesiredType = typeof(TEncode);
            _encodeAs = expression.Compile();

            return this;
        }

        override internal object Encode(object value)
        {
            if (_encodeAs != null)
            {
                try
                {
                    value = _encodeAs.DynamicInvoke(value);
                }
                catch (Exception exception)
                {
                    throw new Exception("EncodeAs expression caused an exception while attempting to encode '" + value + "'.", exception);
                }
            }

            return value;
        }

        override internal object Decode(object value)
        {
            if (_decodeAs != null)
            {
                try
                {
                    value = _decodeAs.DynamicInvoke(value);
                }
                catch (Exception exception)
                {
                    throw new Exception("DecodeAs expression caused an exception while attempting to decode '" + value + "'.", exception);
                }
            }

            return value;
        } 
    }
}

#endif