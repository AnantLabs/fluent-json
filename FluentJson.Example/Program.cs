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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;

using FluentJson.Configuration;

namespace FluentJson.Example
{
    class Program
    {
        private static JsonConfiguration<Book> _configuration;

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
            _example1();
            _example2();
            _example3();
            _example4();
            _example5();
            _example6();
            _example7();
            _example8();
            _example9();
        }

        private static void _printJson(string json)
        {
            Console.WriteLine();
            Console.BackgroundColor = ConsoleColor.DarkMagenta;

            string structural = "{}[],:";

            Stack<bool> isObject = new Stack<bool>();
            bool isField = false;

            foreach (char c in json)
            {
                Console.ForegroundColor = ConsoleColor.White;

                if (c == '{' || c == '[')
                {
                    isObject.Push(c == '{');
                    isField = c == '{';
                }
                else if (c == '}' || c == ']')
                {
                    isObject.Pop();
                    isField = false;
                }
                else if (c == ',' && isObject.Peek())
                    isField = true;
                else if (c == ':')
                    isField = false;

                // Set color
                if (structural.Contains(c.ToString()))
                    Console.ForegroundColor = ConsoleColor.Cyan;
                else if (isField)
                    Console.ForegroundColor = ConsoleColor.Magenta;

                Console.Write(c);
            }

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
        }

        //***********************************************************************************
        private static void _example1()
        {
            // Encode Book A
            JsonEncoder<Book> encoder = Json.EncoderFor<Book>(config => config
                .MapType<Book>(map => map
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

            Console.WriteLine("Example 1:");
            string json = encoder.Encode(Program.bookA);
            _printJson(json);
            Console.ReadLine();
        }

        //***********************************************************************************
        private static void _example2()
        {
            // Re-using configurations
            JsonConfiguration<Book> configBase = Json.ConfigurationFor<Book>()
                .MapType<Book>(map => map
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

            JsonEncoder<Book> encoder = Json.EncoderFor<Book>(config => config
                .DeriveFrom(configBase)
                .UseTidy(true)
            );

            JsonDecoder<Book> decoder = Json.DecoderFor<Book>(config => config
                .DeriveFrom(configBase)
            );

            string json = encoder.Encode(Program.bookB);
            Console.WriteLine("Example 2:");
            _printJson(json);
            Console.ReadLine();
        }

        //***********************************************************************************
        private static void _example3()
        {
            _setup();

            JsonEncoder<Book> encoder = Json.EncoderFor<Book>(config => config
               .DeriveFrom(_configuration)

               // This request will NOT override the existing mapping.
               // This makes extending the base configuration easy.
               .MapType<Book>(map => map
                   .FieldTo(field => field.numPages, "test")
                )

               .UseTidy(true)
           );

            string json = encoder.Encode(Program.bookB);
            Console.WriteLine("Example 3:");
            _printJson(json);
            Console.ReadLine();
        }

        private static void _setup()
        {
            _configuration = Json.ConfigurationFor<Book>()
                .MapType<Book>(map => map
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
        }

        //***********************************************************************************
        private static void _example4()
        {
            JsonEncoder<IDictionary<string, Author>> encoder = Json.EncoderFor<IDictionary<string, Author>>(config => config
                .MapType<Author>(map => map
                    .AllFields()
                )
                .UseTidy(true)
            );

            Dictionary<string, Author> input = new Dictionary<string, Author>();
            input.Add("author 1: ", new Author() { forname = "Jules", surname = "Verne" });
            input.Add("author 2: ", new Author() { forname = "Bob", surname = "Doe" });

            Console.WriteLine("Example 4:");
            string json = encoder.Encode(input);
            _printJson(json);
            Console.ReadLine();
        }

        //***********************************************************************************
        private static void _example5()
        {
            JsonEncoder<IDictionary<string, Author>> encoder = Json.EncoderFor<IDictionary<string, Author>>(config => config
                .AutoGenerate()
                .UseTidy(true)
            );

            Dictionary<string, Author> input = new Dictionary<string, Author>();
            input.Add("author 1: ", new Author() { forname = "Jules", surname = "Verne" });
            input.Add("author 2: ", new Author() { forname = "Bob", surname = "Doe" });


            Console.WriteLine("Example 5:");
            string json = encoder.Encode(input);
            _printJson(json);
            Console.ReadLine();
        }

        //***********************************************************************************
        private static void _example6()
        {
            Dictionary<string, Author> input = new Dictionary<string, Author>();
            input.Add("author 1: ", new Author() { forname = "Jules", surname = "Verne" });
            input.Add("author 2: ", new Author() { forname = "Bob", surname = "Doe" });

            Console.WriteLine("Example 6:");
            string json = Json.EncodeType<IDictionary<string, Author>>(input);
            _printJson(json);
            Console.ReadLine();

            object decoded = Json.Decode(json);
            IDictionary<string, Author> output = Json.DecodeType<IDictionary<string, Author>>(json);
        }

        //***********************************************************************************
        private static void _example7()
        {
            _setup();

            JsonEncoder<Book> encoder = Json.EncoderFor<Book>(config => config
               .DeriveFrom(_configuration)
               .UseTidy(true)
               .UsePluginPoint(_customEncode)
           );

            Console.WriteLine("Example 7:");
            string json = encoder.Encode(Program.bookB);
            Console.ReadLine();

            JsonDecoder<Book> decoder = Json.DecoderFor<Book>(config => config
                .DeriveFrom(_configuration)
                .UsePluginPoint(_customDecode)
            );

            _printJson(json);

            Book book = decoder.Decode(json);
            Console.ReadLine();
        }

        //***********************************************************************************
        private static void _example8()
        {
            dynamic input = new ExpandoObject();
            input.test = new ExpandoObject();
            input.test.a = true;
            input.test.b = new List<string> { "a", "b" };

            Console.WriteLine("Example 8:");
            string json = Json.EncoderFor<dynamic>(config => config
                .UsePluginPoint(_customEncode)).Encode(input);

            _printJson(json);
            Console.ReadLine();
        }

        private static object _customEncode(object value)
        {
            Console.WriteLine("Just before encoding: " + value);
            if (value is Author)
            {
                (value as Author).forname = "".PadLeft((value as Author).forname.Length, '#');
            }

            return value;
        }

        private static object _customDecode(object value)
        {
            Console.WriteLine("Just after decoding: " + value);
            return value;
        }

        //***********************************************************************************
        private static void _example9()
        {
            JsonEncoder<IList<Book>> encoder = Json.EncoderFor<IList<Book>>(config => config
                .WithMapping(_configuration.GetMapping<Book>())
                .WithMapping(_configuration.GetMapping<Author>())
            );

            JsonEncoder<IList<Book>> encoderParallel = Json.EncoderFor<IList<Book>>(config => config
                .WithMapping(_configuration.GetMapping<Book>())
                .WithMapping(_configuration.GetMapping<Author>())
                .UseParallelProcessing(true)
            );

            JsonDecoder<IList<Book>> decoder = Json.DecoderFor<IList<Book>>(config => config
                .WithMapping(_configuration.GetMapping<Book>())
                .WithMapping(_configuration.GetMapping<Author>())
            );

            List<Book> books = new List<Book>();
            for (int i = 0; i < 10000; i++)
            {
                books.Add(bookA);
            }

            Console.WriteLine("Example 9:");
            Console.WriteLine("Encoding 10.000 Books (multi-threaded)");
            Stopwatch sw = Stopwatch.StartNew();
            string json = encoderParallel.Encode(books);
            sw.Stop();
            Console.WriteLine("Time in ms: " + sw.ElapsedMilliseconds);
            Console.ReadLine();

            Console.WriteLine("Example 9:");
            Console.WriteLine("Encoding 10.000 Books");
            sw = Stopwatch.StartNew();
            encoder.Encode(books);
            sw.Stop();
            Console.WriteLine("Time in ms: " + sw.ElapsedMilliseconds);
            Console.ReadLine();

            Console.WriteLine("Decoding 10.000 Books");
            sw = Stopwatch.StartNew();
            decoder.Decode(json);
            sw.Stop();
            Console.WriteLine("Time in ms: " + sw.ElapsedMilliseconds);
            Console.ReadLine();
        }

        //***********************************************************************************
        private static void _example10()
        {

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
