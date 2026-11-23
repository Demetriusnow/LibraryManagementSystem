using Library.Core.Entities;
using Library.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Library.DAL.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        LibraryDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        if (!await dbContext.Readers.AnyAsync(cancellationToken))
        {
            await dbContext.Readers.AddRangeAsync(
                new Reader
                {
                    FullName = "Ivan Petrenko",
                    Email = "ivan.petrenko@example.com"
                },
                new Reader
                {
                    FullName = "Olena Kovalenko",
                    Email = "olena.kovalenko@example.com"
                });

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (await dbContext.Books.AnyAsync(cancellationToken) ||
            await dbContext.Authors.AnyAsync(cancellationToken))
        {
            return;
        }

        var authorMartin = new Author { Name = "Robert C. Martin" };
        var authorEvans = new Author { Name = "Eric Evans" };

        await dbContext.Authors.AddRangeAsync(authorMartin, authorEvans);
        await dbContext.SaveChangesAsync(cancellationToken);

        var bookCleanArch = new Book
        {
            Title = "Clean Architecture",
            ISBN = "978-0134494166",
            Category = "Software Engineering",
            PublishedYear = 2017,
            AuthorId = authorMartin.Id,
            Author = authorMartin
        };

        var bookDdd = new Book
        {
            Title = "Domain-Driven Design",
            ISBN = "978-0321125217",
            Category = "Architecture",
            PublishedYear = 2003,
            AuthorId = authorEvans.Id,
            Author = authorEvans
        };

        await dbContext.Books.AddRangeAsync(bookCleanArch, bookDdd);
        await dbContext.SaveChangesAsync(cancellationToken);

        var copy1 = new BookCopy
        {
            BookId = bookCleanArch.Id,
            InventoryNumber = "INV-001",
            Status = BookCopyStatus.Available
        };

        var copy2 = new BookCopy
        {
            BookId = bookDdd.Id,
            InventoryNumber = "INV-002",
            Status = BookCopyStatus.Borrowed
        };

        await dbContext.BookCopies.AddRangeAsync(copy1, copy2);
        await dbContext.SaveChangesAsync(cancellationToken);

        var firstReader = await dbContext.Readers
            .OrderBy(reader => reader.Id)
            .FirstAsync(cancellationToken);

        var borrowDate = DateTime.UtcNow.AddDays(-16);

        await dbContext.Loans.AddAsync(
            new Loan
            {
                BookCopyId = copy2.Id,
                ReaderId = firstReader.Id,
                BorrowDate = borrowDate,
                DueDate = borrowDate.AddDays(14),
                Status = LoanStatus.Active
            },
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
