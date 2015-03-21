# Details #

## Decoding a collection ##
```
IJsonDecoder<IList<Book>> decoder = Json.DecoderFor<IList<Book>>(config => config
    .MapType<Book>(map => map
        .AllFields()
    )
);
```