# Dapper Fluent Query Helper
Dapper helper for writing fluent-mode queries.

 - .Net Standard 2.0. (Xamarin compatible)
 - Only requires Dapper.
 - Uses Linq expressions for preventing hand-written strings.

 This project have commands for:
 - Select, Update & Delete database operations.
 - A 'WHERE' statement can be added to each command.

 In addition a little MySQL database related project is added for managing connections and transactions.

# Select command example 
```
     new DSelect()
        .Select(nameof(Book.Id), nameof(Book.Title), nameof(Book.Date), nameof(Book.Price))
        .Select($"{nameof(Author)}.{nameof(Author.Name)}")
        .From<Book>()
        .Join(nameof(Book.Id), JoinOperator.Equals, $"{nameof(Author)}.{nameof(Author.Id)}")
        .Where(w => w.And(
            w.Filter($"{nameof(Author)}.{nameof(Author.Prive)}" < 400),
            w.Filter(nameof(Book.Category), Operator.NotLike, "Terror"),
            w.FilterIsNotNull($"{nameof(Author)}.{nameof(Author.Reviews)}"),
            w.Or(
            w.FilterBetween(nameof(Book.Price), 10, 400),
            w.FilterIsNull(nameof(Book.Price))
            )
            ))
        .OrderBy(nameof(Book.Date))
        .Query<BooksInfo>(DBConnection);    
```

Resulting in this query:
```
 SELECT Book.Id, Book.Title, Book.Date, Book.Price, Author.Name
  FROM Book
  JOIN Author ON (Book.AuthorId = Author.Id)
  WHERE 
        Book.Price < 400 AND 
        Book.Category not like "Terror" AND 
        Author.Reviews is not null AND
        (
          Book.Price BETWEEN 10 AND 400 OR
          Book.Price IS NULL
        )
  ORDER BY Book.Date
```

# Update command example
```
   // This method perform an update only on the specified properties: price and category.
   new DUpdate().Update(() => new Book { Price = 0, Category = "Free" })
        .Where(w => w.FilterIsNull(nameof(Book.Price)));

```

# Delete command example
```
    // This method deletes all the books with a price greater than 6000.
    new DDelete().Delete()
        .Where(w => w.Filter(nameof(Book.Price), JoinOperator.GreaterThan, 6000));

```

# MySQL transactions example
```
    // The transaction block perform a rollback if there is some problem in the middle of the process.
    BookList books = new SQLTransaction(connectionString)
       .Transaction<BookList>(tran => 
        {
            tran.Query...
            tran.Update...
            return tran.Query<BookList>(new DSelect())...
       ));

    // The non transaction block allows to reuse the connection.
    BookList books = new SQLTransaction(connectionString)
       .NonTransaction<BookList>(nontran => nontran.Query<BookList>(
            new DSelect()...
       ));
```

# Simple, faster

- Fluent query mode close to SQL syntax.
- Property type cache.
- Dynamic parameter generation with DB types.
- Simple and easy to extend less than **four hundred code lines**.

# License

Dapper Fluent Query Helper is licensed under [Apache 2.0.](https://github.com/jiman14/DapperFluentQueryHelper/blob/main/LICENSE "Apache 2.0 License")
