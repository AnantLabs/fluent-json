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
    /// Abstract class for an encoding instruction. Implementations can direct the process.
    /// </summary>
    internal abstract class EncodingInstruction { }

    /// <summary>
    /// Tells the process to encode the given value.
    /// </summary>
    internal class DoEncode : EncodingInstruction
    {
        internal object Value { get; private set; }
        internal Type KnownType { get; private set; }

        internal JsonTokenSequence Output { get; set; }

        internal DoEncode(object value, Type knownType)
        {
            this.Value = value;
            this.KnownType = knownType;
        }
    }

    /// <summary>
    /// Tells the process to do a async encode for the given value.
    /// </summary>
    internal class DoParallelEncode : DoEncode
    {
        internal DoParallelEncode(object value, Type knownType)
            : base(value, knownType)
        {

        }
    }


    /// <summary>
    /// Tells the process to synchronize all currently async encoding taksks.
    /// </summary>
    internal class SyncParallelEncode : EncodingInstruction { }

    /// <summary>
    /// Tells the process that the current encoding context is invalid.
    /// </summary>
    internal class ContextInvalidated : EncodingInstruction { }
}
