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
using FluentJson.Configuration;

namespace FluentJson.Example
{
    class Program
    {
        private static Book bookA = new Book { 
            title = "Around the world in 80 days",
            type = BookType.Roman,
            pubDate = new DateTime(1892, 1, 11),
            numPages = 200,
            tags = new List<string> { "travel", "adventure" },
            author = new Author {
                forname = "Jules",
                surname = "Verne"
            }
        };

        private static Book bookB = new Book
        {
            title = "A Lover's Complaint",
            type = BookType.Poetry,
            pubDate = new DateTime(1612, 4, 2),
            numPages = 12,
            tags = null,
            author = new Author
            {
                forname = "William",
                surname = "Shakespeare"
            }
        };

        static void Main(string[] args)
        {
            _example2();
        }

        private static void _example1()
        {
            // Encode Book A
            IJsonEncoder<Book> encoder = Json.EncoderFor<Book>(config => config
                .Map(map => map
                    .AllFields()

                    // DateTime can't be encoded to json. Unless we convert it,
                    // or map it. Mapping is a poor choice. So conversion it is.
                    .Field<DateTime>(field => field.pubDate, pubDate => pubDate
                        // The expression below, will be called upon when encoding
                        // begins.
                        .EncodeAs<string>(value => value.ToShortDateString())
                    )

                    // BookType can't be encoded either, let's convert it too.
                    .Field<BookType>(field => field.type, type => type
                        // One way to encode enums, is to type cast to a number.
                        // Great for optimization.
                        .EncodeAs<int>(value => (int)value)
                        // Lets assume we would want to encode this field to a
                        // different json field.
                        .To("book_type")
                    )
                )

                // Since the author field is mapped too (because of AllFields()),
                // we have to map it's type too.
                .MapType<Author>(map => map
                    // Nothing special.
                    .AllFields()
                )
                .UseTidy(true)
            );

            string json = encoder.Encode(Program.bookA);
            Console.WriteLine("Example 1:");
            Console.WriteLine(json);
            Console.ReadLine();
        }

        private static void _example2()
        {
            // Re-using configurations
            JsonConfiguration<Book> configBase = Json.ConfigurationFor<Book>()
                .Map(map => map
                    .AllFields()
                    .Field<DateTime>(field => field.pubDate, pubDate => pubDate
                        .EncodeAs<string>(value => value.ToShortDateString())
                        .DecodeAs<string>(value => DateTime.Parse(value))
                    )
                    .Field<BookType>(field => field.type, type => type
                        .EncodeAs<int>(value => (int)value)
                        .DecodeAs<int>(value => (BookType)Enum.ToObject(typeof(BookType), value))
                    )
                )
                .MapType<Author>(map => map
                    .AllFields()
                );

            IJsonEncoder<Book> encoder = Json.EncoderFor<Book>(config => config
                .DeriveFrom(configBase)
                .UseTidy(true)
            );

            IJsonDecoder<Book> decoder = Json.DecoderFor<Book>(config => config
                .DeriveFrom(configBase)
            );

            string json = encoder.Encode(Program.bookB);
            Console.WriteLine("Example 2:");
            Console.WriteLine(json);
            Console.ReadLine();

            Book book = decoder.Decode(json);
            Console.WriteLine(book.title);
            Console.ReadLine();
        }

        private static void _example3()
        {
            //hmmmm 
        }
    }

    enum BookType
    {
        Novel, Roman, Poetry
    }

    class Book
    {
        public string title { get; set; }
        public BookType type { get; set; }
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
