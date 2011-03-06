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
using System.Linq.Expressions;

using System.Reflection;

namespace FluentJson.Mapping
{
    public class JsonObjectMappingBase
    {
        internal Dictionary<MemberInfo, string> Mappings { get; private set; }
        internal JsonObjectMappingBase()
        {
            this.Mappings = new Dictionary<MemberInfo, string>();
        }
    }

    public class JsonObjectMapping<T> : JsonObjectMappingBase
    {
        private List<MemberInfo> _exludes;

        internal JsonObjectMapping()
        {
            _exludes = new List<MemberInfo>();
        }

        public JsonObjectMapping<T> Map(Expression<Func<T, object>> expression, string fieldName)
        {
            MemberInfo memberInfo = _getAccessedFieldOrProperty(expression);
            _mapMember(memberInfo, fieldName);

            return this;
        }

        public JsonObjectMapping<T> AutoMap()
        {
            List<MemberInfo> members = new List<MemberInfo>();
            members.AddRange(typeof(T).GetFields());
            members.AddRange(typeof(T).GetProperties());

            foreach(MemberInfo memberInfo in members)
            {
                _mapMember(memberInfo, memberInfo.Name);
            }

            return this;
        }

        public JsonObjectMapping<T> Exclude(Expression<Func<T, object>> expression)
        {
            MemberInfo memberInfo = _getAccessedFieldOrProperty(expression);

            if (_exludes.Contains(memberInfo))
            {
                throw new Exception("The member '" + memberInfo.Name + "' is already excluded.");
            }

            if (Mappings.ContainsKey(memberInfo))
            {
                this.Mappings.Remove(memberInfo);
            }

            _exludes.Add(memberInfo);
            return this;
        }

        private MemberInfo _getAccessedFieldOrProperty(Expression<Func<T, object>> expression)
        {
            if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression memberExpression = (MemberExpression)expression.Body;
                if (memberExpression.Member is FieldInfo || memberExpression.Member is PropertyInfo)
                {
                    return memberExpression.Member;
                }
            }

            throw new Exception("This expression does not define a property or field access.");
        }

        private void _mapMember(MemberInfo memberInfo, string fieldName)
        {
            if (!_exludes.Contains(memberInfo))
            {
                if (!this.Mappings.ContainsKey(memberInfo) && !this.Mappings.ContainsValue(fieldName))
                {
                    this.Mappings.Add(memberInfo, fieldName);
                }
                else
                {
                    if (this.Mappings.ContainsKey(memberInfo))
                    {
                        throw new Exception("The member '" + memberInfo.Name + "' is already mapped.");
                    }
                    else
                    {
                        throw new Exception("An existing member is already mapped to '" + fieldName + "'.");
                    }
                }
            }
        }
    }
}
