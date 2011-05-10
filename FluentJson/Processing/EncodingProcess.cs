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

#if NET40
using System.Threading.Tasks;
using System.Dynamic;
#endif

using FluentJson.Helpers;
using FluentJson.Configuration;
using FluentJson.Processing.Nodes;

namespace FluentJson.Processing
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class EncodingProcess<T> : Process
    {
        internal JsonEncodingConfiguration<T> Configuration { get; private set; }

        internal EncodingProcess(JsonEncodingConfiguration<T> configuration) : base(configuration.Mappings.Values, configuration.UsesParallelProcessing)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Encodes the given object to a json string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal string Encode(T value)
        {
            EncodingContext root = new EncodingContext(this, value, typeof(T));
            _encodeContext(root);

            if (Configuration.UsesTidy)
                return root.Output.ToTidyString();
            else
                return root.Output.ToString();
        }

        private EncodingContext _encodeContext(EncodingContext context)
        {
            Queue<EncodingContext> locals = new Queue<EncodingContext>();

            #if NET40
            Queue<Task> parallel = new Queue<Task>();
            #endif

            Queue<IEncodingNode> executions = _buildExecutionQueue(context);

            while (executions.Count > 0)
            {
                // Execute next node
                IEncodingNode node = executions.Dequeue();

                foreach (EncodingInstruction instruction in node.ExecuteEncode(context))
                {
                    if (instruction.GetType() == typeof(DoEncode))
                    {
                        DoEncode casted = (DoEncode)instruction;

                        if (locals.Count == 0)
                        {
                            // Recursive processing
                            locals.Enqueue(_encodeContext(new EncodingContext(this, casted.Value, casted.KnownType)));
                        }

                        casted.Output = locals.Dequeue().Output;
                    }
                    #if NET40
                    else if (instruction.GetType() == typeof(DoParallelEncode))
                    {
                        DoParallelEncode casted = (DoParallelEncode)instruction;

                        EncodingContext inner = new EncodingContext(this, casted.Value, casted.KnownType);
                        locals.Enqueue(inner);

                        parallel.Enqueue(Task.Factory.StartNew(() =>
                            {
                                _encodeContext(inner);
                            }
                        ));
                    }
                    else if (instruction.GetType() == typeof(SyncParallelEncode))
                    {
                        Task.WaitAll(parallel.ToArray());
                        parallel = new Queue<Task>();
                    }
                    #endif
                    else if (instruction.GetType() == typeof(ContextInvalidated))
                    {
                        executions = _buildExecutionQueue(context);
                        executions.Dequeue(); // Removes plugin point
                    }
                }
            }

            return context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private Queue<IEncodingNode> _buildExecutionQueue(EncodingContext context)
        {
            Queue<IEncodingNode> executions = new Queue<IEncodingNode>();

            #if !NET20
            // > Plugin Point
            if (Configuration.PluginPoint != null)
            {
                executions.Enqueue(new PluginPointNode(this.Configuration.PluginPoint));
            }
            #endif

            if (context.KnownType != null && context.KnownType != typeof(object) && !TypeHelper.IsBasic(context.KnownType) && context.Value != null)
            {
                // > Mapping
                if (Configuration.Mappings.ContainsKey(context.KnownType))
                {
                    executions.Enqueue(new MappingNode(Configuration.Mappings[context.KnownType]));
                }
                // > IList<T>
                else if (TypeHelper.IsThreatableAs(context.KnownType, typeof(IList<>)))
                {
                    executions.Enqueue(new ListNode(null, context.KnownType.GetGenericArguments()[0]));
                }
                // > IDictionary<T>
                else if (TypeHelper.IsThreatableAs(context.KnownType, typeof(IDictionary<,>)))
                {
                    executions.Enqueue(new DictNode(null, context.KnownType.GetGenericArguments()[1]));
                }
                // [] Array
                else if (context.KnownType.IsArray)
                {
                    executions.Enqueue(new ListNode(context.KnownType, context.KnownType.GetElementType()));
                }
            }

            #if !NET20
            if (executions.Count == 0 || (Configuration.PluginPoint != null && executions.Count == 1))
            #else
            if (executions.Count == 0)
            #endif
            {
                if (context.Value is IList)
                {
                    executions.Enqueue(new ListNode());
                }
                else if (context.Value is IDictionary)
                {
                    executions.Enqueue(new DictNode());
                }
                #if NET40
                else if (context.Value is ExpandoObject)
                {
                    executions.Enqueue(new DictNode(typeof(ExpandoObject), null));
                }
                #endif
                else
                {
                    executions.Enqueue(new ValudeNode());
                }
            }

            return executions;
        }
    }
}
