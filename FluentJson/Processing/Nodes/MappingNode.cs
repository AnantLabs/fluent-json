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

using FluentJson.Mapping;

namespace FluentJson.Processing.Nodes
{
    class MappingNode : IEncodingNode, IDecodingNode
    {
        JsonTypeMappingBase _mapping;

        internal MappingNode(JsonTypeMappingBase mapping)
        {
            _mapping = mapping;
        }

        public IEnumerable<EncodingInstruction> ExecuteEncode(EncodingContext context)
        {
            if (_mapping.UsesReferencing)
            {
                if (context.Process.References.HasReferenceTo(context.Value))
                {
                    double reference = context.Process.References.GetReferenceTo(context.Value);
                    context.Output += JsonToken.Parse(reference);

                    yield break;
                }
                else
                {
                    context.Process.References.Reference(context.Value);
                }
            }

            context.Output += JsonToken.BeginObject;

            Dictionary<string, JsonFieldMappingBase>.Enumerator fields = _mapping.FieldMappings.GetEnumerator();
            for (int index = 0; fields.MoveNext(); index++ )
            {
                JsonFieldMappingBase fieldMapping = fields.Current.Value;

                if (index > 0)
                {
                    context.Output += JsonToken.ValueSeperator;
                }

                object value = fieldMapping.Get(context.Value);

                context.Output += JsonToken.Parse(fieldMapping.JsonField, JsonTokenType.String);
                context.Output += JsonToken.NameSeperator;

                DoEncode instruction = new DoEncode(fieldMapping.Encode(value), fieldMapping.DesiredType);
                yield return instruction;

                context.Output += instruction.Output;
            }

            context.Output += JsonToken.EndObject;
        }

        public IEnumerable<DecodingInstruction> ExecuteDecode(DecodingContext context)
        {
            if (_mapping.UsesReferencing && context.Input.Peek().Type == JsonTokenType.Number)
            {
                double reference = context.Input.Pop().Value<double>();
                context.Value = context.Process.References.FollowReference(reference);

                yield break;
            }

            context.Input -= JsonToken.BeginObject;

            object reflected = Activator.CreateInstance(context.KnownType);

            if (_mapping.UsesReferencing)
            {
                context.Process.References.Reference(reflected);
            }

            for (int index = 0; context.Input.Peek() != JsonToken.EndObject; index++)
            {
                if (index > 0)
                {
                    context.Input -= JsonToken.ValueSeperator;
                }

                string key = context.Input.Pop().Value<string>();
                JsonFieldMappingBase fieldMapping = _mapping.FieldMappings[key];

                context.Input -= JsonToken.NameSeperator;

                DoDecode instruction = new DoDecode(context.Input, fieldMapping.DesiredType);
                yield return instruction;

                object value = fieldMapping.Decode(instruction.Value);
                fieldMapping.Set(reflected, value);
            }

            context.Input -= JsonToken.EndObject;
            context.Value = reflected;
        }
    }
}
