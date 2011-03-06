﻿// Copyright (c) 2011, Adaptiv Design
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

namespace FluentJson.Mapping
{
    internal class MappedDecoder<T> : IJsonDecoder<T>
    {
        private JsonDecoder _decoder;
        private JsonBaseConfiguration<T> _configuration;

        internal MappedDecoder(JsonBaseConfiguration<T> configuration)
        {
            _decoder = new JsonDecoder();
            _configuration = configuration;
        }

        public T Decode(string json)
        {
            // Do a normal decode
            object decoded = _decoder.Decode(json);

            // Convert
            return (T)_toDesiredValue(decoded, typeof(T));
        }

        /// <summary>
        /// Attempts to convert a value to the desired type.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="desiredValueType"></param>
        /// <returns></returns>
        private object _toDesiredValue(object value, Type desiredValueType)
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
                // Desired type has been mapped, use this mapping.
                object result = Activator.CreateInstance(desiredValueType);
                if (value is IDictionary<string, object>)
                {
                    _transferDictionary((IDictionary<string, object>)value, result);
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
                                if (genericArguments[0].IsAssignableFrom(element.GetType()))
                                {
                                    list.Add(element);
                                }
                                else
                                {
                                    throw new Exception("Decoded value (" + element + ") could not be converted to List element type '" + genericArguments[0].Name +"'.");
                                }
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
                                if (genericArguments[1].IsAssignableFrom(enumerator.Value.GetType()))
                                {
                                    dict.Add(enumerator.Key, enumerator.Value);
                                }
                                else
                                {
                                    throw new Exception("Decoded value (" + enumerator.Value + ") could not be converted to Dictionary value type '" + genericArguments[1].Name + "'.");
                                }
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

        private void _transferDictionary(IDictionary<string, object> dictionary, object target)
        {
            if (_configuration.Mappings.ContainsKey(target.GetType()))
            {
                JsonObjectMappingBase mapping = _configuration.Mappings[target.GetType()];

                Dictionary<MemberInfo, string>.Enumerator mappings = mapping.Mappings.GetEnumerator();
                while (mappings.MoveNext())
                {
                    string fieldName = mappings.Current.Value;
                    if (dictionary.ContainsKey(fieldName))
                    {
                        MemberInfo memberInfo = mappings.Current.Key;

                        // Assign property
                        if (memberInfo is PropertyInfo)
                        {
                            PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
                            if (propertyInfo.CanWrite)
                            {
                                propertyInfo.SetValue(target, _toDesiredValue(dictionary[fieldName], propertyInfo.PropertyType), null);
                            }
                            else
                            {
                                throw new Exception("Property '" + propertyInfo.Name + "' could not be assigned.");
                            }
                        }

                        //Assign field
                        if (memberInfo is FieldInfo)
                        {
                            FieldInfo fieldInfo = (FieldInfo)memberInfo;
                            fieldInfo.SetValue(target, _toDesiredValue(dictionary[fieldName], fieldInfo.FieldType));
                        }

                    }
                    else
                    {
                        throw new Exception("Field '" + fieldName + "' is not present in decoded dictionary.");
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