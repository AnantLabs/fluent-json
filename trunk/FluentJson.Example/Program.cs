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
using System.Globalization;

using FluentJson.Mapping;

namespace FluentJson.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            // Construct the encoder
            IJsonEncoder<Book> encoder = Json.EncoderFor<Book>(config => config
                .Map(map => map
                    .AllFields()
                    .Field<DateTime>(book => book.pubDate, pubDate => pubDate
                        .EncodeAs<string>(date => date.ToShortDateString())
                    )
                )
                .MapType<Author>(map => map
                    .AllFields()
                )
                .Tidy(true)
            );

            // Construct input
            Book input = new Book();
            input.title = "Around the world in 80 days";
            input.tags = new List<string> { "traveling", "adventure" };
            input.pubDate = DateTime.Today;
            input.numPages = 342;

            input.author = new Author();
            input.author.forname = "Jules";
            input.author.surname = "Verne";

            // ENCODE
            string json = encoder.Encode(input);

            Console.WriteLine(json);
            Console.ReadLine();

            // Construct the decoder
            IJsonDecoder<Book> decoder = Json.DecoderFor<Book>(config => config
                .Map(map => map
                    .AllFields()
                    .Field<DateTime>(book => book.pubDate, pubDate => pubDate
                        .DecodeAs<string>(date => DateTime.Parse(date))
                    )
                )
                .MapType<Author>(map => map
                    .AllFields()
                )
            );

            // DECODE
            Book output = decoder.Decode(json);

            Console.WriteLine("Encoded pubdate == Decoded pubdate");
            Console.WriteLine(input.pubDate == output.pubDate);
            Console.ReadLine();
        }
    }

    class Book
    {
        public string title { get; set; }
        public DateTime pubDate { get; set; }
        public int numPages { get; set; }
        public IList<string> tags { get; set; }

        public Author author { get; set; }
    }

    class Author
    {
        public string forname { get; set; }
        public string surname { get; set; }
    }
}
