# Dapper Fluent Query Helper
Dapper helper for writing fluent-mode queries.

 - .Net Standard 2.1.
 - Only requires Dapper.

# Example of use
```
IEnumerable<BooksInfo> books = new DapperFluentQuery()
 .Select(FQ.Book.Id, FQ.Book.Title, FQ.Book.Date, FQ.Book.Price, FQ.Author.Name)
 .From<Book>()
 .Join<Author>(FQ.Book.AuthorId, JoinOperator.Equals, FQ.Author.Id)
 .Where(w => w.And(
      w.Filter(FQ.Book.Category, Operator.NotLike, "Terror"),
      w.Filter(FQ.Author.Reviews, Operator.NotNull),
      w.Or(
        w.Filter(FQ.Book.Price, Operator.Between, 10, 400),
        w.Filter(FQ.Book.Price, Operator.IsNull)
        )
      ))
 .OrderBy(FQ.Book.Date)
 .Query<BooksInfo>(DBConnection);
```
Resulting in this query:
```
 SELECT Book.Id, Book.Title, Book.Date, Book.Price, Author.Name
  FROM Book
  JOIN Author ON (Book.AuthorId = Author.Id)
  WHERE 
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

# Pre-requisites

It needs entities with properties for linking to table fields (FQ.Book.Id), for example:
```
public class Book
{
  public string Id {get; private set;} = nameof(Book) + "." + nameof(Id);
  ... 
}
```
Thus, I recommend generators for mapping the DB table info into classes like [Codverter](https://codverter.com/src/sqltoclass "Codverter").

# License

Dapper Fluent Query Helper is licensed under [Apache 2.0.](https://github.com/jiman14/DapperFluentQueryHelper/blob/main/LICENSE "Apache 2.0 License")
