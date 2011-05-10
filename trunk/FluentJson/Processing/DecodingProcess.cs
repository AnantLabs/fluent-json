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

using System.Collections;
using System.Collections.Generic;

using FluentJson.Configuration;
using FluentJson.Helpers;
using FluentJson.Processing.Nodes;

namespace FluentJson.Processing
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DecodingProcess<T> : Process
    {
        internal JsonDecodingConfiguration<T> Configuration { get; private set; }

        internal DecodingProcess(JsonDecodingConfiguration<T> configuration) : base(configuration.Mappings.Values, configuration.UsesParallelProcessing)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Decodes the given json string to an object of type T.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        internal T Decode(string json)
        {
            DecodingContext context = new DecodingContext(this, new JsonTokenSequence(json), typeof(T));
            _decodeContext(context);

            return (T)context.Value;
        }

        private DecodingContext _decodeContext(DecodingContext context)
        {
            Queue<DecodingContext> locals = new Queue<DecodingContext>();
            Queue<IDecodingNode> executions = _buildExecutionQueue(context);

            while (executions.Count > 0)
            {
                // Execute next node
                IDecodingNode node = executions.Dequeue();

                foreach (DecodingInstruction instruction in node.ExecuteDecode(context))
                {
                    if (instruction is DoDecode)
                    {
                        DoDecode casted = (DoDecode)instruction;

                        if (locals.Count == 0)
                        {
                            // Recursive processing
                            locals.Enqueue(_decodeContext(new DecodingContext(this, casted.Input, casted.KnownType)));
                        }

                        casted.Value = locals.Dequeue().Value;
                    }
                    else if (instruction is ProvideNextNode)
                    {
                        ProvideNextNode casted = (ProvideNextNode)instruction;
                        executions = new Queue<IDecodingNode>();
                        executions.Enqueue(casted.NextNode);

                        #if !NET20
                        // > Plugin Point
                        if (Configuration.PluginPoint != null)
                        {
                            executions.Enqueue(new PluginPointNode(this.Configuration.PluginPoint));
                        }
                        #endif
                    }
                }
            }

            return context;
        }

        private Queue<IDecodingNode> _buildExecutionQueue(DecodingContext context)
        {
            Queue<IDecodingNode> executions = new Queue<IDecodingNode>();
            executions.Enqueue(new ResolveNextNode());

            if (context.KnownType != null && context.KnownType != typeof(object))
            {
                // > Mapping
                if (Configuration.Mappings.ContainsKey(context.KnownType))
                {
                    executions.Enqueue(new MappingNode(Configuration.Mappings[context.KnownType]));
                }
                // > IList<T>
                else if (TypeHelper.IsThreatableAs(context.KnownType, typeof(IList<>)))
                {
                    executions.Enqueue(new ListNode(context.KnownType, context.KnownType.GetGenericArguments()[0]));
                }
                // [] Array
                else if (context.KnownType.IsArray)
                {
                    executions.Enqueue(new ListNode(context.KnownType, context.KnownType.GetElementType()));
                }
                // > IList
                else if (TypeHelper.IsThreatableAs(context.KnownType, typeof(IList)))
                {
                    executions.Enqueue(new ListNode(context.KnownType, null));
                }
                // > IDictionary<T>
                else if (TypeHelper.IsThreatableAs(context.KnownType, typeof(IDictionary<,>)))
                {
                    executions.Enqueue(new DictNode(context.KnownType, context.KnownType.GetGenericArguments()[1]));
                }
                // > IDictionary
                else if (TypeHelper.IsThreatableAs(context.KnownType, typeof(IDictionary)))
                {
                    executions.Enqueue(new DictNode(context.KnownType, null));
                }
                // > Value ?
                else
                {
                    executions.Enqueue(new ValudeNode());
                }
            }

            #if !NET20
            // > Plugin Point
            if (Configuration.PluginPoint != null)
            {
                executions.Enqueue(new PluginPointNode(this.Configuration.PluginPoint));
            }
            #endif

            return executions;
        }
    }
}
