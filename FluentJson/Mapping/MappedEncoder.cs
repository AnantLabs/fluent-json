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
using System.Reflection;

using FluentJson.Configuration;
using FluentJson.Helpers;

namespace FluentJson.Mapping
{
    class MappedEncoder<T> : IJsonEncoder<T>
    {
        private JsonEncoder _encoder;
        private JsonConfiguration<T> _configuration;

        public MappedEncoder(JsonEncodingConfiguration<T> configuration)
        {
            _encoder = new JsonEncoder();
            _encoder.IsTidy = configuration.UsesTidy;
            _configuration = configuration;
        }

        public string Encode(T value)
        {
            return _encoder.Encode(_toEncodableValue(value, new ReferenceStore()));
        }

        private IDictionary<string, object> _toDictionary(object value, ReferenceStore references)
        {
            Dictionary<string, object> result = new Dictionary<string,object>();

            // Get the mapping
            JsonObjectMappingBase mapping = _configuration.Mappings[value.GetType()];

            // Itterate over each field mapping
            Dictionary<MemberInfo, JsonFieldMappingBase>.Enumerator mappings = mapping.FieldMappings.GetEnumerator();
            while (mappings.MoveNext())
            {
                JsonFieldMappingBase fieldMapping = mappings.Current.Value;
                MemberInfo memberInfo = mappings.Current.Key;

                // Get value for encoding
                object fieldValue = null;
                if (memberInfo is PropertyInfo)
                {
                    // Access property
                    PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
                    if (propertyInfo.CanRead)
                    {
                        fieldValue = propertyInfo.GetValue(value, null);
                    }
                    else
                    {
                        throw new Exception("Property '" + propertyInfo.Name + "' could not be read.");
                    }
                }
                else if (memberInfo is FieldInfo)
                {
                    // Access field
                    FieldInfo fieldInfo = (FieldInfo)memberInfo;
                    fieldValue = fieldInfo.GetValue(value);
                }

                // Pass trough (possible) fieldmapping encoder
                fieldValue = fieldMapping.Encode(fieldValue);

                // Convert to encodable value
                fieldValue = _toEncodableValue(fieldValue, references);

                if (!result.ContainsKey(fieldMapping.JsonObjectField))
                {
                    result.Add(fieldMapping.JsonObjectField, fieldValue);
                }
                else
                {
                    throw new Exception("A field mapping already exists for field name '" + fieldMapping.JsonObjectField + "'.");
                }
            }

            return result;
        }

        private object _toEncodableValue(object value, ReferenceStore references)
        {
            // Nothing we can do
            if (value == null) return value;

            // Get the actual type
            Type type = value.GetType();

            if (_configuration.Mappings.ContainsKey(type))
            {
                // This is a mapped type
                if (_configuration.Mappings[type].UsesReferencing)
                {
                    // This type uses referencing
                    if (references.HasReferenceTo(value))
                    {
                        // This value should be encoded as a reference
                        return references.GetReferenceTo(value);
                    }
                    else
                    {
                        // This type has no reference yet, create one.
                        references.StoreObject(value);
                    }
                }

                // Convert this mapped type to a dictionary
                return _toDictionary(value, references);
            }
            else if(TypeHelper.IsList(type))
            {
                // This type will be threated as a list.

                // Each element will be processed, a new list is required.
                List<object> list = new List<object>();
                foreach (object element in (value as IList))
                {
                    list.Add(_toEncodableValue(element, references));
                }

                return list;
            }
            else if(TypeHelper.IsDictionary(type))
            {
                // This type will be threated as a dictionary
                Dictionary<string, object> dictionary = new Dictionary<string, object>();

                // Each element will be processed, a new list is required.
                IDictionaryEnumerator enumerator = (value as IDictionary).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    // For successful encoding, the key is required to be a string
                    if(!(enumerator.Key is string))
                    {
                        // Key is not a string
                        throw new Exception("Dictionary key '" + enumerator.Key + "; is not of type String.");
                    }

                    dictionary.Add((string)enumerator.Key, _toEncodableValue(enumerator.Value, references));
                }

                return dictionary;
            }

            return value;
        }
    }
}
