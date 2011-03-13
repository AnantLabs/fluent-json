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

namespace FluentJson.Mapping
{
    /// <summary>
    /// Used internally for storing references while encoding or decoding.
    /// </summary>
    internal class ReferenceStore
    {
        /// <summary>
        /// object to reference
        /// </summary>
        private Dictionary<object, double> _references;

        /// <summary>
        /// reference to object
        /// </summary>
        private Dictionary<double, object> _objects;

        internal ReferenceStore()
        {
            _references = new Dictionary<object, double>();
            _objects = new Dictionary<double, object>();
        }

        /// <summary>
        /// Creates a numerical reference to the given object.
        /// </summary>
        /// <param name="value"></param>
        internal void StoreObject(object value)
        {
            if (!_references.ContainsKey(value))
            {
                _references.Add(value, _references.Count);
                _objects.Add(_objects.Count, value);
            }
        }

        /// <summary>
        /// Sees if a numerical reference is stored for the given object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal bool HasReferenceTo(object value)
        {
            return _references.ContainsKey(value);
        }

        /// <summary>
        /// Gets the numerical reference to the given object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal double GetReferenceTo(object value)
        {
            return _references[value];
        }

        /// <summary>
        /// Sees if this numerical value actually points to an object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal bool IsReference(double value)
        {
            return _objects.ContainsKey(value);
        }

        /// <summary>
        /// Resolves the object from the given numerical reference.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal object GetFromReference(double value)
        {
            return _objects[value];
        }
    }
}
