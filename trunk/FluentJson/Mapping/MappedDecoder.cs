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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

using FluentJson.Configuration;

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
            if (value == null)
            {
                if (!desiredValueType.IsValueType)
                {
                    return value;
                }
                else
                {
                    throw new Exception("Value type '" + desiredValueType.Name + "' cannot be assigned null.");
                }
            }

            if (_configuration.Mappings.ContainsKey(desiredValueType))
            {
                if (_configuration.Mappings[desiredValueType].UsesReferencing && value is double)
                {
                    if (references.IsReference((double)value))
                    {
                        return references.GetFromReference((double)value);
                    }
                    else
                    {
                        throw new Exception("Decoded value (" + value + ") is an invalid reference to'" + desiredValueType.Name + "'.");
                    }
                }

                // Desired type has been mapped, use this mapping.
                object result = Activator.CreateInstance(desiredValueType);

                if (_configuration.Mappings[desiredValueType].UsesReferencing)
                {
                    references.StoreObject(result);
                }


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
            else if (desiredValueType.IsAssignableFrom(value.GetType()))
            {
                // Safe to assign, simply pass on.
                return value;
            }
            else if (desiredValueType.IsGenericType)
            {
                // Try to convert current value to a matching generic type
                Type[] genericArguments = desiredValueType.GetGenericArguments();
                if (genericArguments.Length == 1)
                {
                    // 1 generic argument could indicate a generic list
                    if (typeof(IList<>).MakeGenericType(genericArguments[0]).IsAssignableFrom(desiredValueType))
                    {
                        // A list is desired
                        IList list = null;
                        if (!desiredValueType.IsInterface)
                        {
                            list = (IList)Activator.CreateInstance(desiredValueType);
                        }
                        else
                        {
                            list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(genericArguments[0]));
                        }
                        
                        if (value is IList)
                        {
                            foreach (object element in (value as IList))
                            {
                                list.Add(_toDesiredValue(element, genericArguments[0], references));
                            }

                            return list;
                        }
                    }
                }
                else if (genericArguments.Length == 2 && genericArguments[0].IsAssignableFrom(typeof(string)))
                {
                    // 2 generic arguments could indicate a generic dictionary
                    if (typeof(IDictionary<,>).MakeGenericType(genericArguments[0], genericArguments[1]).IsAssignableFrom(desiredValueType))
                    {
                        // A dictionary is desired
                        IDictionary dict = null;
                        if (!desiredValueType.IsInterface)
                        {
                            dict = (IDictionary)Activator.CreateInstance(desiredValueType);
                        }
                        else
                        {
                            dict = (IDictionary)Activator.CreateInstance(typeof(IDictionary<,>).MakeGenericType(genericArguments[0], genericArguments[1]));
                        }

                        if (value is IDictionary)
                        {
                            IDictionaryEnumerator enumerator = (value as IDictionary).GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                dict.Add(enumerator.Key, _toDesiredValue(enumerator.Value, genericArguments[1], references));
                            }

                            return dict;
                        }
                    }
                }
            }
            else if (desiredValueType.IsPrimitive && !desiredValueType.IsAssignableFrom(typeof(bool)) && value is double)
            {
                string str = Convert.ToString(value, CultureInfo.InvariantCulture);

                try
                {
                    if (desiredValueType == typeof(byte)) return byte.Parse(str);
                    if (desiredValueType == typeof(sbyte)) return sbyte.Parse(str);
                    if (desiredValueType == typeof(short)) return short.Parse(str);
                    if (desiredValueType == typeof(ushort)) return ushort.Parse(str);
                    if (desiredValueType == typeof(int)) return int.Parse(str);
                    if (desiredValueType == typeof(uint)) return uint.Parse(str);
                    if (desiredValueType == typeof(float)) return float.Parse(str);
                    if (desiredValueType == typeof(double)) return double.Parse(str);
                    if (desiredValueType == typeof(long)) return long.Parse(str);
                }
                catch (Exception exception)
                {
                    throw new Exception("Decoded number (" + value + ") could not be converted to number type '" + desiredValueType.Name + "'.", exception);
                }
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

#endif