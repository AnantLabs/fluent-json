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

using System.Collections.Generic;

namespace FluentJson.Processing
{
    internal class ReferenceStore
    {
        /// <summary>
        /// Get reference by object
        /// </summary>
        private Dictionary<object, double> _to;

        /// <summary>
        /// Get object from reference
        /// </summary>
        private Dictionary<double, object> _from;

        internal ReferenceStore()
        {
            _to = new Dictionary<object, double>();
            _from = new Dictionary<double, object>();
        }

        /// <summary>
        /// Returns the object the given reference points to.
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        internal object FollowReference(double reference)
        {
            return _from[reference];
        }

        /// <summary>
        /// Indicates wether this store contains a reference to the given object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal bool HasReferenceTo(object value)
        {
            return _to.ContainsKey(value);
        }

        /// <summary>
        /// Returns the reference value for the given object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal double GetReferenceTo(object value)
        {
            return _to[value];
        }

        /// <summary>
        /// Creates a reference for the given object.
        /// </summary>
        /// <param name="value"></param>
        internal void Reference(object value)
        {
            _to.Add(value, _to.Count);
            _from.Add(_from.Count, value);
        }
    }
}
