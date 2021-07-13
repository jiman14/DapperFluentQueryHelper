# Dapper Fluent Query Helper
Dapper helper for writing fluent-mode queries.

 - .Net Standard 2.1.
 - Only requires Dapper.

# Example
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

# Simple, faster

- Fluent query mode close to SQL syntax.
- Property type cache.
- Dynamic parameter generation with DB types.
- Simple and easy to extend less than **four hundred code lines**.

# Pre-requisites

It needs entities with properties for linking to table fields (FQ.Book.Id), like:
```
public class Book
{
  public string Id {get; private set;} = nameof(Book) + "." + nameof(Id);
  ... 
}
```
Thus, I recommend generators for mapping the DB table info into classes.

