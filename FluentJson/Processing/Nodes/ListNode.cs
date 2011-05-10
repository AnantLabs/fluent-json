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

using FluentJson.Helpers;

namespace FluentJson.Processing.Nodes
{
    class ListNode : IEncodingNode, IDecodingNode
    {
        private Type _listType;
        private Type _genericArgument;

        internal ListNode()
        {

        }

        internal ListNode(Type listType, Type genericArgument)
        {
            _listType = listType;
            _genericArgument = genericArgument;
        }

        public IEnumerable<EncodingInstruction> ExecuteEncode(EncodingContext context)
        {
            IList list = (IList)context.Value;
            bool parallel = false;

            // See if this list would benefit from parallel processing
            if (context.Process.IsParallel && _genericArgument != null && !context.Process.RequiresReferencing && TypeHelper.IsParallelBeneficial(_genericArgument))
            {
                foreach (object el in list)
                {
                    // Do parallel
                    yield return new DoParallelEncode(el, _genericArgument);
                }

                parallel = true;
            }

            context.Output += JsonToken.BeginArray;

            for (int index = 0; index < list.Count; index++)
            {
                object element = list[index];

                if (index > 0)
                {
                    context.Output += JsonToken.ValueSeperator;
                }
                else if (parallel)
                {
                    // Wait untill all parallel tasks are finished.
                    yield return new SyncParallelEncode();
                }

                DoEncode instruction = new DoEncode(element, _genericArgument);
                yield return instruction;

                context.Output += instruction.Output;

            }

            context.Output += JsonToken.EndArray;
        }

        public IEnumerable<DecodingInstruction> ExecuteDecode(DecodingContext context)
        {
            context.Input -= JsonToken.BeginArray;

            IList list;
            if (_listType == null || _listType.IsInterface || _listType.IsArray)
            {
                if (_genericArgument == null)
                {
                    list = new List<object>();
                }
                else
                {
                    list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(_genericArgument));
                }
            }
            else
            {
                // Create the desired instance directly
                list = (IList)Activator.CreateInstance(_listType);
            }

            for (int index = 0; context.Input.Peek() != JsonToken.EndArray; index++)
            {
                if (index > 0)
                {
                    context.Input -= JsonToken.ValueSeperator;
                }

                DoDecode instruction = new DoDecode(context.Input, _genericArgument);
                yield return instruction;

                list.Add(instruction.Value);
            }

            context.Input -= JsonToken.EndArray;

            if (_listType == null || !_listType.IsArray)
            {
                context.Value = list;
            }
            else
            {
                Array array = Array.CreateInstance(_listType.GetElementType(), list.Count);
                list.CopyTo(array, 0);

                context.Value = array;
            }
        }
    }
}
