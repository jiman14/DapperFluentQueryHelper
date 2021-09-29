# Dapper Fluent Query Helper
Dapper helper for writing fluent-mode queries.

 - .Net Standard 2.0. (Xamarin compatible)
 - Only requires Dapper.
 - Uses Linq expressions for preventing hand-written strings.

# Example of use
```
IEnumerable<BooksInfo> books = new DapperFluentQuery()
 .Select(() => Book.Id, Book.Title, Book.Date, Book.Price)
 .Select(() => FQ.Author.Name)
 .From<Book>()
 .Join(() => Book.AuthorId, JoinOperator.Equals, () => Author.Id)
 .Where(w => w.And(
      w.Filter(() => Author.Price < 400),
      w.Filter(() => Book.Category, Operator.NotLike, "Terror"),
      w.FilterIsNotNull(() => Author.Reviews),
      w.Or(
        w.FilterBetween(() => Book.Price, 10, 400),
        w.FilterIsNull(() => Book.Price)
        )
      ))
 .OrderBy(() => Book.Date)
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


# Simple, faster

- Fluent query mode close to SQL syntax.
- Property type cache.
- Dynamic parameter generation with DB types.
- Simple and easy to extend less than **four hundred code lines**.

# License

Dapper Fluent Query Helper is licensed under [Apache 2.0.](https://github.com/jiman14/DapperFluentQueryHelper/blob/main/LICENSE "Apache 2.0 License")
