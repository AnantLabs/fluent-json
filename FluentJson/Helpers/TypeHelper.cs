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

namespace FluentJson.Helpers
{
    public class TypeHelper
    {
        /// <summary>
        /// Sees if the given type is threatable as another type.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <param name="asType">Type to be threated as.</param>
        /// <returns></returns>
        static public bool IsThreatableAs(Type type, Type asType)
        {
            // Works for most cases
            if (asType.IsAssignableFrom(type)) return true;
            
            // Account for generics
            if (type.IsGenericType)
            {
                Type typeReduced = type.GetGenericTypeDefinition();
                Type asTypeReduced = asType;

                if (asType.IsGenericType)
                {
                    asTypeReduced = asType.GetGenericTypeDefinition();
                }

                // Verify again
                if (asTypeReduced.IsAssignableFrom(typeReduced))
                {
                    // Verify arguments
                    Type[] typeArguments = type.GetGenericArguments();
                    Type[] asTypeArguments = asType.GetGenericArguments();

                    // Lengths should be equal
                    if (typeArguments.Length != asTypeArguments.Length) return false;

                    bool isSuccess = true;
                    for(int i = 0; i < typeArguments.Length; i++)
                    {
                        // Arguments for type should be actual types
                        if (typeArguments[0].IsGenericParameter)
                        {
                            isSuccess = false;
                            break;
                        }

                        // Arguments for "as" type should be generic parameters
                        if (!asTypeArguments[0].IsGenericParameter)
                        {
                            isSuccess = false;
                            break;
                        }
                    }

                    if (isSuccess)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Sees if the given type is a basic type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public bool IsBasic(Type type)
        {
            return type.IsPrimitive || type == typeof(string);
        }

        /// <summary>
        /// Sees if the given type is numerical. Decimals do not count as a number.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public bool IsNumerical(Type type)
        {
            return (type.IsPrimitive) && type != typeof(bool) && type != typeof(char);
        }

        /// <summary>
        /// Sees if the given type is a list.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public bool IsList(Type type)
        {
            return IsThreatableAs(type, typeof(IList)) || IsThreatableAs(type, typeof(IList<>));
        }

        /// <summary>
        /// Sees if the given is a dictionary.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public bool IsDictionary(Type type)
        {
            return IsThreatableAs(type, typeof(IDictionary)) || IsThreatableAs(type, typeof(IDictionary<,>));
        }

        /// <summary>
        /// Sees if the given type is a generic type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public bool IsGeneric(Type type)
        {
            return type.GetGenericArguments().Length > 0;
        }

        /// <summary>
        /// Sees if the given type is a primitive type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public bool IsParallelBeneficial(Type type)
        {
            return !IsBasic(type) && type != typeof(object);
        }
    }
}
