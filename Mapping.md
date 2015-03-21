# Introduction #

Encoders and decoders in Fluent Json enable you to strongly type map your classes to the json data format. This mapping behaviour
can (and often should) be fully defined by you. An important design choice was not to add extensions to the json data format. Basic .Net datatypes such as enums and for instance DateTime, will fail to encode(unless mapped).

# Details #

## Constructing an encoder or decoder ##

```
IJsonEncoder<Book> encoder = Json.EncoderFor<Book>(config => config
    .Map(map => map)
);
```


## Mapping all fields at once ##

```
IJsonEncoder<Book> encoder = Json.EncoderFor<Book>(config => config
    .Map(map => map
        .AllFields()
    )
);
```

## Excluding certain fields ##

```
IJsonEncoder<Book> encoder = Json.EncoderFor<Book>(config => config
    .Map(map => map
        .AllFields()
        .ExceptField(book => book.author)
    )
);
```

## Maping to a specific field name ##

```
IJsonEncoder<Book> encoder = Json.EncoderFor<Book>(config => config
    .Map(map => map
        .FieldTo(book => book.author, "book_author")
    )
);
```

## Mapping to a different type ##

Mapping a DateTime property to an encoder:
```
IJsonEncoder<Book> encoder = Json.EncoderFor<Book>(config => config
    .Map(map => map
        .Field<DateTime>(book => book.pubDate, pubDate => pubDate
            .EncodeAs<string>(value => value.ToShortDateString())
        )
    )
);
```

Mapping a DateTime property to a decoder:
```
IJsonDecoder<Book> decoder = Json.DecoderFor<Book>(config => config
    .Map(map => map
        .Field<DateTime>(book => book.pubDate, pubDate => pubDate
            .DecodeAs<string>(value => DateTime.Parse(value))
        )
    )
);
```

### Handling enums ###

This is a handy way to handle enums:

```
IJsonEncoder<Book> encoder = Json.EncoderFor<Book>(config => config
    .Map(map => map
        .Field<BookType>(book => book.type, type => type
            .EncodeAs<int>(value => (int)value)
        )
    )
);
```

```
IJsonDecoder<Book> decoder = Json.DecoderFor<Book>(config => config
    .Map(map => map
        .Field<BookType>(book => book.type, type => type
            .DecodeAs<int>(value => (BookType)Enum.ToObject(typeof(BookType), value))
        )
    )
);
```

## Mapping references ##

If either desired or required (to prevent infinite encoding of references), you can configure the mapper to work with references.

```
IJsonEncoder<Book> encoder = Json.EncoderFor<Book>(config => config
    .Map(map => map
        .UseReferencing(true)
    )
);
```