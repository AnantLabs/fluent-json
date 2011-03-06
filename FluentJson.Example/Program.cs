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

namespace FluentJson.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Book input = new Book();
            input.title = "Around the world in 80 days";
            input.pageCount = 342;

            input.author = new Author();
            input.author.forname = "Jules";
            input.author.surname = "Verne";
            input.author.bookCount = 11;

            input.tags = new List<string> { "traveling", "adventure" };

            IJsonEncoder<Book> encoder = Json.EncoderFor<Book>(config => config
                .MapObject(map => map
                    .AutoMap()
                )
                .MapObject<Author>(map =>
                    map.AutoMap()
                )
                .Tidy(true)
            );

            IJsonDecoder<Book> decoder = Json.DecoderFor<Book>(config => config
                .MapObject(map => map
                    .AutoMap()
                )
                .MapObject<Author>(map =>
                    map.AutoMap()
                )
            );

            Console.WriteLine("Encoded:");
            Console.ReadLine();
            Console.WriteLine(encoder.Encode(input));
            Console.ReadLine();
            Console.WriteLine("Decoded and Encoded:");
            Console.WriteLine(encoder.Encode(decoder.Decode(encoder.Encode(input))));
            Console.ReadLine();
        }
    }

    class Book
    {
        public string title { get; set; }
        public IList<string> tags { get; set; }
        public int pageCount { get; set; }

        public Author author { get; set; }
    }

    class Author : Person
    {
        public short bookCount { get; set; }
    }

    class Person
    {
        public string forname { get; set; }
        public string surname { get; set; }
    }
}
