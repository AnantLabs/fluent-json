Json generator and parser written in C#. Besides basic json support, this library enables you to fluently map your custom types  to the json data format.

**Version 0.8.0 underway**

**New version 0.7.0:**

  * Major performance improvements (see example application).
  * **Multi-threaded encoding** (in .NET 4.0 using Task Parallel Library). Also see example application. Can give a descent performance increase when encoding large sets of complex objects.
  * A lot of fixes, mainly due to a complete rebuild.

Any suggestions or questions? Head to http://groups.google.com/group/fluent-json.

  * Fluently map your classes to the json data format.
  * Ensures strong type mapping.
  * Does not require class modifications.
  * Does not extend the json data format.
  * A single encoder or decoder can be used among multiple threads (thread-safe).
  * All functionality is provides from a single (static member) class.

### Fluent mapping: ###

```
JsonEncoder<Book> encoder = Json.EncoderFor<Book>(config => config
    .MapType<Book>(map => map

        .AllFields()

        // DateTime can't be encoded to native json. Conversion is required.
        .Field<DateTime>(field => field.pubDate, pubDate => pubDate
            .EncodeAs<string>(value => value.ToShortDateString())
        )

        // BookType can't be encoded either, let's convert it too.
        .Field<BookType>(field => field.type, type => type
            .EncodeAs<int>(value => (int)value)
            // Lets assume we would want to encode this field to a
            // different json field.
            .To("book_type")
        )
    )

    .MapType<Author>(map => map
        .AllFields()
    )

    .UseTidy(true)
);

Book book = new Book();
book.title = "Around the world in 80 days";
book.tags = new List<string> { "traveling", "adventure" };
book.pageCount = 342;
book.pubDate = DateTime.Now;

book.author = new Author();
book.author.forname = "Jules";
book.author.surname = "Verne";

string json = encoder.Encode(book);
```
