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

namespace FluentJson.Processing
{
    /// <summary>
    /// Abstract class for an decoding instruction. Implementations can direct the process.
    /// </summary>
    internal abstract class DecodingInstruction { }

    /// <summary>
    /// Tells the process to decode the given sequence.
    /// </summary>
    internal class DoDecode : DecodingInstruction
    {
        internal object Value { get; set; }
        internal Type KnownType { get; private set; }

        internal JsonTokenSequence Input { get; set; }

        internal DoDecode(JsonTokenSequence input, Type knownType)
        {
            this.Input = input;
            this.KnownType = knownType;
        }
    }

    /// <summary>
    /// Tells the process to use the given Node as the next in chain.
    /// </summary>
    internal class ProvideNextNode : DecodingInstruction
    {
        internal IDecodingNode NextNode { get; private set; }

        internal ProvideNextNode(IDecodingNode nextNode)
        {
            this.NextNode = nextNode;
        }
    }
}
