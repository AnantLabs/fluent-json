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
using System.Collections.Generic;
using System.Linq.Expressions;

using System.Reflection;

namespace FluentJson.Mapping
{
    abstract public class JsonObjectMappingBase : ICloneable
    {
        internal bool UsesReferencing { get; set; }
        internal Dictionary<MemberInfo, JsonFieldMappingBase> FieldMappings { get; private set; }
        internal JsonObjectMappingBase()
        {
            this.FieldMappings = new Dictionary<MemberInfo, JsonFieldMappingBase>();
        }

        abstract public object Clone();
    }

    public class JsonObjectMapping<T> : JsonObjectMappingBase
    {
        private List<MemberInfo> _exludes;

        public JsonObjectMapping()
        {
            _exludes = new List<MemberInfo>();
        }

        public override object Clone()
        {
            JsonObjectMapping<T> clone = new JsonObjectMapping<T>();
            clone.UsesReferencing = this.UsesReferencing;

            Dictionary<MemberInfo, JsonFieldMappingBase>.Enumerator enumerator = this.FieldMappings.GetEnumerator();
            while (enumerator.MoveNext())
            {
                clone.FieldMappings.Add(enumerator.Current.Key, (JsonFieldMappingBase)enumerator.Current.Value.Clone());
            }

            foreach(MemberInfo exclude in _exludes)
            {
                clone._exludes.Add(exclude);
            }

            return clone;
        }

        public JsonObjectMapping<T> UseReferencing(bool value)
        {
            this.UsesReferencing = value;
            return this;
        }

        public JsonObjectMapping<T> AllFields()
        {
            List<MemberInfo> members = new List<MemberInfo>();
            members.AddRange(typeof(T).GetFields());
            members.AddRange(typeof(T).GetProperties());

            foreach (MemberInfo memberInfo in members)
            {
                _mapMember(memberInfo, new JsonFieldMapping<object>(memberInfo));
            }

            return this;
        }

        public JsonObjectMapping<T> FieldTo(Expression<Func<T, object>> fieldExpression, string jsonObjectField)
        {
            MemberInfo memberInfo = _getAccessedMemberInfo(fieldExpression);
            _mapMember(memberInfo, new JsonFieldMapping<object>(memberInfo, jsonObjectField));

            return this;
        }

        public JsonObjectMapping<T> Field<TField>(Expression<Func<T, TField>> fieldExpression, Action<JsonFieldMapping<TField>> mappingExpression)
        {
            MemberInfo memberInfo = _getAccessedMemberInfo<TField>(fieldExpression);

            JsonFieldMapping<TField> fieldMapping = new JsonFieldMapping<TField>(memberInfo);
            mappingExpression(fieldMapping);

            _mapMember(memberInfo, fieldMapping);

            return this;
        }

        public JsonObjectMapping<T> ExceptField(Expression<Func<T, object>> fieldExpression)
        {
            MemberInfo memberInfo = _getAccessedMemberInfo(fieldExpression);

            if (_exludes.Contains(memberInfo))
            {
                throw new Exception("The member '" + memberInfo.Name + "' is already excluded.");
            }

            if (FieldMappings.ContainsKey(memberInfo))
            {
                this.FieldMappings.Remove(memberInfo);
            }

            _exludes.Add(memberInfo);
            return this;
        }

        private MemberInfo _getAccessedMemberInfo<TField>(Expression<Func<T, TField>> expression)
        {
            Expression current = expression;
            while (current != null)
            {
                if (current.NodeType == ExpressionType.MemberAccess)
                {
                    MemberExpression memberExpression = (MemberExpression)current;
                    if (memberExpression.Member is FieldInfo || memberExpression.Member is PropertyInfo)
                    {
                        return memberExpression.Member;
                    }
                }
                else if (current.NodeType == ExpressionType.Convert)
                {
                    current = (current as UnaryExpression).Operand;
                }
                else if (current.NodeType == ExpressionType.Lambda)
                {
                    current = (current as LambdaExpression).Body;
                }
                else
                {
                    break;
                }
            }

            throw new Exception("This expression does not define a property or field access.");
        }

        private void _mapMember(MemberInfo memberInfo, JsonFieldMappingBase fieldMapping)
        {
            if (!_exludes.Contains(memberInfo))
            {
                if (this.FieldMappings.ContainsKey(memberInfo))
                {
                    this.FieldMappings[memberInfo] = fieldMapping;
                }
                else
                {
                    // Mismatch could have occured (due to different MemberInfo.ReflectedType)
                    MemberInfo overriden = null;
                    foreach (MemberInfo key in this.FieldMappings.Keys)
                    {
                        if (key.Name == memberInfo.Name)
                        {
                            overriden = key;
                            break;
                        }
                    }
                    
                    // Remove deprecated entry
                    if (overriden != null)
                    {
                        this.FieldMappings.Remove(overriden);
                    }

                    this.FieldMappings.Add(memberInfo, fieldMapping);
                }
            }
        }
    }
}

#endif