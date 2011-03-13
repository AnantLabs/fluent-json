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
using System.Reflection;

using FluentJson.Configuration;
using FluentJson.Helpers;

namespace FluentJson.Mapping
{
    internal class MappedDecoder<T> : IJsonDecoder<T>
    {
        private JsonDecoder _decoder;
        private JsonConfiguration<T> _configuration;

        internal MappedDecoder(JsonConfiguration<T> configuration)
        {
            _decoder = new JsonDecoder();
            _configuration = configuration;
        }

        public T Decode(string json)
        {
            // Do a normal decode
            object decoded = _decoder.Decode(json);

            // Convert
            return (T)_toDesiredValue(decoded, typeof(T), new ReferenceStore());
        }

        /// <summary>
        /// Attempts to convert a value to the desired type.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="desiredValueType"></param>
        /// <returns></returns>
        private object _toDesiredValue(object value, Type desiredValueType, ReferenceStore references)
        {
            // Nothing we can do for null values
            if (value == null)
            {
                // But we can see if the desired type is nullable
                if (!desiredValueType.IsValueType)
                {
                    return value;
                }
                else
                {
                    throw new Exception("Value type '" + desiredValueType.Name + "' cannot be assigned null.");
                }
            }

            // Get the actual type
            Type type = value.GetType();

            if (_configuration.Mappings.ContainsKey(desiredValueType))
            {
                // Desired type is a mapped type
                if (_configuration.Mappings[desiredValueType].UsesReferencing && value is double)
                {
                    // See if current value is a reference
                    if (references.IsReference((double)value))
                    {
                        // Current value is a reference
                        return references.GetFromReference((double)value);
                    }
                    else
                    {
                        throw new Exception("Decoded value (" + value + ") is an invalid reference to'" + desiredValueType.Name + "'.");
                    }
                }

                // Desired type has been mapped, use this mapping to construct the desired type.
                object result = Activator.CreateInstance(desiredValueType);

                // See if the current value should be referenced
                if (_configuration.Mappings[desiredValueType].UsesReferencing)
                {
                    // Add a reference to this newly constructed value
                    references.StoreObject(result);
                }

                // JsonDecoder is expected to return a Dictionary<string, object> for a json object
                if (value is IDictionary<string, object>)
                {
                    _transferDictionary((IDictionary<string, object>)value, result, references);
                    return result;
                }
                else
                {
                    throw new Exception("Decoded value (" + value + ") could not be mapped against '" + desiredValueType.Name + "'.");
                }
            }
            else if(TypeHelper.IsThreatableAs(type, desiredValueType))
            {
                // Safe to assign, simply pass on.
                return value;
            }
            else if ((TypeHelper.IsList(desiredValueType) || desiredValueType.IsArray) && value is IList)
            {
                Type desiredElementType = null;
                IList list = null;

                if (desiredValueType.IsArray && desiredValueType.GetArrayRank() == 1)
                {
                    desiredElementType = desiredValueType.GetElementType();
                    list = (IList)Activator.CreateInstance(desiredElementType.MakeArrayType(), new object[] { (value as IList).Count });

                    for (int i = 0; i < (value as IList).Count; i++)
                    {
                        list[i] = _toDesiredValue((value as IList)[i], desiredElementType, references);
                    }
                }
                else if (TypeHelper.IsGeneric(desiredValueType))
                {
                    desiredElementType = desiredValueType.GetGenericArguments()[0];

                    if (desiredValueType.IsInterface)
                    {
                        list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(desiredValueType.GetGenericArguments()));
                    }
                    else
                    {
                        list = (IList)Activator.CreateInstance(desiredValueType);
                    }

                    foreach (object element in (value as IList))
                    {
                        list.Add(_toDesiredValue(element, desiredElementType, references));
                    }
                }

                if (list != null)
                {
                    return list;
                }
            }
            else if (TypeHelper.IsDictionary(desiredValueType) &&  value is IDictionary<string, object>)
            {
                if (!TypeHelper.IsGeneric(desiredValueType) || TypeHelper.IsThreatableAs(desiredValueType.GetGenericArguments()[0], typeof(string)))
                {
                    Type desiredElementType = typeof(object);
                    if (desiredValueType.IsGenericType)
                    {
                        desiredElementType = desiredValueType.GetGenericArguments()[1];
                    }

                    Type[] genericArguments = new Type[] { typeof(string), desiredElementType };

                    IDictionary dict = null;
                    if (desiredValueType.IsInterface)
                    {
                        dict = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(genericArguments));
                    }
                    else
                    {
                        dict = (IDictionary)Activator.CreateInstance(desiredValueType);
                    }

                    foreach (KeyValuePair<string, object> pair in (value as IDictionary<string, object>))
                    {
                        dict.Add(pair.Key, _toDesiredValue(pair.Value, desiredElementType, references));
                    }

                    return dict;
                }
            }
            else if (TypeHelper.IsNumerical(desiredValueType) && TypeHelper.IsNumerical(type))
            {
                if (desiredValueType == typeof(byte)) return Convert.ToByte(value, CultureInfo.InvariantCulture);
                if (desiredValueType == typeof(sbyte)) return Convert.ToSByte(value, CultureInfo.InvariantCulture);
                if (desiredValueType == typeof(short)) return Convert.ToInt16(value, CultureInfo.InvariantCulture);
                if (desiredValueType == typeof(ushort)) return Convert.ToUInt16(value, CultureInfo.InvariantCulture);
                if (desiredValueType == typeof(int)) return Convert.ToInt32(value, CultureInfo.InvariantCulture);
                if (desiredValueType == typeof(uint)) return Convert.ToUInt32(value, CultureInfo.InvariantCulture);
                if (desiredValueType == typeof(long)) return Convert.ToInt64(value, CultureInfo.InvariantCulture);
                if (desiredValueType == typeof(ulong)) return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
                if (desiredValueType == typeof(float)) return Convert.ToSingle(value, CultureInfo.InvariantCulture);
                if (desiredValueType == typeof(double)) return (double)value;
            }

            throw new Exception("Decoded value (" + value + ") could not be converted to type '" + desiredValueType.Name + "'.");
        }

        private void _transferDictionary(IDictionary<string, object> dictionary, object target, ReferenceStore references)
        {
            if (_configuration.Mappings.ContainsKey(target.GetType()))
            {
                JsonObjectMappingBase mapping = _configuration.Mappings[target.GetType()];

                Dictionary<MemberInfo, JsonFieldMappingBase>.Enumerator fieldMappings = mapping.FieldMappings.GetEnumerator();
                while (fieldMappings.MoveNext())
                {
                    JsonFieldMappingBase fieldMapping = fieldMappings.Current.Value;
                    if (dictionary.ContainsKey(fieldMapping.JsonObjectField))
                    {
                        MemberInfo memberInfo = fieldMappings.Current.Key;

                        // Convert to decodable value
                        object fieldValue = _toDesiredValue(dictionary[fieldMapping.JsonObjectField], fieldMapping.DesiredType, references);

                        // Pass trough (possible) fieldmapping decoder
                        fieldValue = fieldMapping.Decode(fieldValue);

                        if (memberInfo is PropertyInfo)
                        {
                            // Assign property
                            PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
                            if (propertyInfo.CanWrite)
                            {
                                propertyInfo.SetValue(target, fieldValue, null);
                            }
                            else
                            {
                                throw new Exception("Property '" + propertyInfo.Name + "' could not be assigned.");
                            }
                        }
                        else if (memberInfo is FieldInfo)
                        {
                            //Assign field
                            FieldInfo fieldInfo = (FieldInfo)memberInfo;
                            fieldInfo.SetValue(target, fieldValue);
                        }

                    }
                    else
                    {
                        throw new Exception("Field '" + fieldMapping + "' is not present in decoded dictionary.");
                    }
                }
            }
            else
            {
                throw new Exception("No mapping could be resolved for type '" + target.GetType() + "'.");
            }
        }
    }
}