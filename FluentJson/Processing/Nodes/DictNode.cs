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

#if NET40
using System.Dynamic;
#endif

using FluentJson.Helpers;

namespace FluentJson.Processing.Nodes
{
    class DictNode : IEncodingNode, IDecodingNode
    {
        private Type _dictType;
        private Type _genericArgument;

        internal DictNode()
        {

        }

        internal DictNode(Type dictType, Type genericArgument)
        {
            _dictType = dictType;
            _genericArgument = genericArgument;
        }

        public IEnumerable<EncodingInstruction> ExecuteEncode(EncodingContext context)
        {
            #if NET40
            IDictionary<string, object> expando = null;
            IDictionary dict = null;
            #else
            IDictionary dict = (IDictionary)context.Value;
            #endif

            #if NET40
            if(_dictType == typeof(ExpandoObject))
            {
                expando = (IDictionary<string, object>)context.Value;
            }
            else
            {
                dict = (IDictionary)context.Value;
            }
            #endif
            
            bool parallel = false;

            // See if this list would benefit from parallel processing
            if (context.Process.IsParallel && _genericArgument != null && !context.Process.RequiresReferencing && TypeHelper.IsParallelBeneficial(_genericArgument))
            {
                #if NET40
                foreach (object el in (dict != null ? (ICollection)dict.Values : (ICollection)expando.Values))
                #else
                foreach (object el in dict.Values)
                #endif
                {
                    // Do parallel
                    yield return new DoParallelEncode(el, _genericArgument);
                }

                parallel = true;
            }

            context.Output += JsonToken.BeginObject;

            object key;
            object value;

            #if NET40
            IEnumerator enumerator = dict != null ? dict.GetEnumerator() : (IEnumerator)expando.GetEnumerator();
            #else
            IDictionaryEnumerator enumerator = dict.GetEnumerator();
            #endif
            for (int index = 0; enumerator.MoveNext(); index++)
            {
                #if NET40
                if (expando != null)
                {
                    KeyValuePair<string, object> pair = (KeyValuePair<string, object>)enumerator.Current;
                    key = pair.Key;
                    value = pair.Value;
                }
                else
                {
                    DictionaryEntry entry = (DictionaryEntry)enumerator.Current;
                    key = entry.Key;
                    value = entry.Value;
                }
                #else
                key = enumerator.Key;
                value = enumerator.Value;
                #endif

                if (index > 0)
                {
                    context.Output += JsonToken.ValueSeperator;
                }
                else if (parallel)
                {
                    // Wait untill all parallel tasks are finished.
                    yield return new SyncParallelEncode();
                }

                context.Output += JsonToken.Parse(key, JsonTokenType.String);
                context.Output += JsonToken.NameSeperator;

                DoEncode instruction = new DoEncode(value, _genericArgument);
                yield return instruction;

                context.Output += instruction.Output;
            }

            context.Output += JsonToken.EndObject;
        }

        public IEnumerable<DecodingInstruction> ExecuteDecode(DecodingContext context)
        {
            context.Input -= JsonToken.BeginObject;

            IDictionary dict;

            #if NET40
            IDictionary<string, object> expando = null;
            #endif

            if (_dictType == null)
            {
                #if NET40
                dict = null;
                expando = new ExpandoObject();
                #else
                dict = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeof(string), _genericArgument));
                #endif
            }
            else if(_dictType.IsInterface)
            {
                if (_genericArgument != null)
                    dict = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeof(string), _genericArgument));
                else
                    dict = (IDictionary)Activator.CreateInstance(typeof(Dictionary<string, object>));

            }
            else
            {
                // Create the desired instance directly
                dict = (IDictionary)Activator.CreateInstance(_dictType);
            }

            for (int index = 0; context.Input.Peek() != JsonToken.EndObject; index++)
            {
                if (index > 0)
                {
                    context.Input -= JsonToken.ValueSeperator;
                }

                string key = context.Input.Pop().Value<string>();

                context.Input -= JsonToken.NameSeperator;

                DoDecode instruction = new DoDecode(context.Input, _genericArgument);
                yield return instruction;

                #if NET40
                if (dict == null)
                {
                    expando.Add(key, instruction.Value);
                }
                else
                {
                    dict.Add(key, instruction.Value);
                }
                #else
                dict.Add(key, instruction.Value);
                #endif
            }

            context.Input -= JsonToken.EndObject;

            #if NET40
            if (dict == null)
            {
                context.Value = expando;
            }
            else
            {
                context.Value = dict;
            }
            #else
            context.Value = dict;
            #endif
        }
    }
}
