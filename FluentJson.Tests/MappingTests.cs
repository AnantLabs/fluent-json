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

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using FluentJson;
using FluentJson.Mapping;

namespace FluentJson.Tests
{
    [TestClass]
    public class MappingTests
    {
        [TestMethod]
        public void TestSingleClass()
        {
            Book input = new Book();
            input.title = "My book";
            input.publicationDate = new DateTime(1900, 12, 4);

            string json = Json.EncoderFor<Book>(config => config
                .Map(map => map
                    .AllFields()
                    .Field<DateTime>(book => book.publicationDate, field => field
                        .EncodeAs<string>(publicationDate => publicationDate.ToString())
                    )
                )
                .UseTidy(true)
            ).Encode(input);

            Book output = Json.DecoderFor<Book>(config => config
                .Map(map => map
                    .AllFields()
                    .Field<DateTime>(book => book.publicationDate, field => field
                        .DecodeAs<string>(publicationDate => DateTime.Parse(publicationDate))
                    )
                )
            ).Decode(json);

            Assert.AreEqual(input, output);
        }

        class Book 
        {
            public string title;
            public DateTime publicationDate;

            override public bool Equals(object obj)
            {
                Book book = (Book)obj;
                return this.title == book.title && this.publicationDate.Equals(book.publicationDate);
            }
        }

        [TestMethod]
        public void TestReferencing()
        {
            RelatedBook input = new RelatedBook();
            input.title = "My book";

            RelatedBook a = new RelatedBook();
            a.title = "Book A";

            RelatedBook b = new RelatedBook();
            b.title = "Book B";

            input.relatedTo = new List<RelatedBook> { a, b };
            a.relatedTo = new List<RelatedBook> { b, input };

            string json = Json.EncoderFor<RelatedBook>(config => config
                .Map(map => map
                    .AllFields()
                    .Field<DateTime>(book => book.publicationDate, field => field
                        .EncodeAs<string>(publicationDate => publicationDate.ToString())
                    )
                    .UseReferencing(true)
                )
                .UseTidy(true)
            ).Encode(input);

            RelatedBook output = Json.DecoderFor<RelatedBook>(config => config
                .Map(map => map
                    .AllFields()
                    .Field<DateTime>(book => book.publicationDate, field => field
                        .DecodeAs<string>(publicationDate => DateTime.Parse(publicationDate))
                    )
                    .UseReferencing(true)
                )
            ).Decode(json);

            Assert.AreEqual(input, output);
        }

        class RelatedBook : Book
        {
            public IList<RelatedBook> relatedTo;
        }
    }
}
