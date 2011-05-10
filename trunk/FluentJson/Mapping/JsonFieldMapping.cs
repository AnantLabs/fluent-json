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

#if !NET20
using System.Linq.Expressions;
#endif

using System.Reflection;

namespace FluentJson.Mapping
{
    abstract public class JsonFieldMappingBase : ICloneable
    {
        internal string JsonField { get; set; }
        internal Type DesiredType { get; set; }

        internal MemberInfo ReflectedField { get; set; }

        abstract internal object Encode(object value);
        abstract internal object Decode(object value);

        abstract internal object Get(object target);
        abstract internal void Set(object target, object value);

        abstract public object Clone();
    }

    public class JsonFieldMapping<T> : JsonFieldMappingBase
    {
        private Delegate _decodeAs;
        private Delegate _encodeAs;

        internal JsonFieldMapping(MemberInfo reflectedField)
        {
            this.ReflectedField = reflectedField;
            this.JsonField = this.ReflectedField.Name;

            if (typeof(T) == typeof(object))
            {
                if (this.ReflectedField is PropertyInfo)
                {
                    this.DesiredType = (this.ReflectedField as PropertyInfo).PropertyType;
                }
                else if (this.ReflectedField is FieldInfo)
                {
                    this.DesiredType = (this.ReflectedField as FieldInfo).FieldType;
                }
            }
            else
            {
                this.DesiredType = typeof(T);
            }
        }

        internal JsonFieldMapping(MemberInfo memberInfo, string jsonField) : this (memberInfo)
        {
            this.JsonField = jsonField;
        }

        #region ICloneable Members

        public override object Clone()
        {
            JsonFieldMapping<T> clone = new JsonFieldMapping<T>(this.ReflectedField);

            clone._decodeAs = _decodeAs;
            clone._encodeAs = _encodeAs;

            clone.JsonField = this.JsonField;
            clone.DesiredType = this.DesiredType;

            return clone;
        }

        #endregion

        /// <summary>
        /// Maps this field to the specified json field.
        /// </summary>
        /// <param name="jsonField"></param>
        /// <returns></returns>
        public JsonFieldMapping<T> To(string jsonField)
        {
            this.JsonField = jsonField;
            return this;
        }

        #if !NET20
        /// <summary>
        /// Decode this field as a different type by suplying a conversion expression.
        /// </summary>
        /// <typeparam name="TDecode"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public JsonFieldMapping<T> DecodeAs<TDecode>(Expression<Func<TDecode, T>> expression)
        {
            this.DesiredType = typeof(TDecode);
            _decodeAs = expression.Compile();

            return this;
        }

        /// <summary>
        /// Encode this field as a different type by suplying a conversion expression.
        /// </summary>
        /// <typeparam name="TEncode"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public JsonFieldMapping<T> EncodeAs<TEncode>(Expression<Func<T, TEncode>> expression)
        {
            this.DesiredType = typeof(TEncode);
            _encodeAs = expression.Compile();

            return this;
        }
        #endif

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

        override internal void Set(object target, object value)
        {
            if (this.ReflectedField is PropertyInfo)
            {
                PropertyInfo propertyInfo = (PropertyInfo)this.ReflectedField;
                if (propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(target, value, null);
                    return;
                }
            }
            else if (this.ReflectedField is FieldInfo)
            {
                FieldInfo fieldInfo = (FieldInfo)this.ReflectedField;
                fieldInfo.SetValue(target, value);
                return;
            }

            throw new Exception("Not a field nor a property.");
        }

        override internal object Get(object target)
        {
            if (this.ReflectedField is PropertyInfo)
            {
                PropertyInfo propertyInfo = (PropertyInfo)this.ReflectedField;
                if (propertyInfo.CanWrite)
                {
                    return propertyInfo.GetValue(target, null);
                }
            }
            else if (this.ReflectedField is FieldInfo)
            {
                FieldInfo fieldInfo = (FieldInfo)this.ReflectedField;
                return fieldInfo.GetValue(target);
            }

            throw new Exception("Not a field nor a property.");
        }
    }
}