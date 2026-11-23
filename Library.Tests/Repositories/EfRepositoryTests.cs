using FluentAssertions;
using Library.BLL.Services;
using Library.Core.Entities;
using Library.Core.Enums;
using Library.DAL.Data;
using Library.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Library.Tests.Repositories;

public class EfRepositoryTests
{
    private static LibraryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new LibraryDbContext(options);
    }

    [Fact]
    public async Task DbSeeder_WhenCalledTwice_ShouldSeedOnceWithoutDuplicates()
    {
        // Arrange
        await using var context = CreateDbContext();

        // Act
        await DbSeeder.SeedAsync(context);
        await DbSeeder.SeedAsync(context);

        // Assert
        (await context.Authors.CountAsync()).Should().Be(2);
        (await context.Books.CountAsync()).Should().Be(2);
        (await context.BookCopies.CountAsync()).Should().Be(2);
        (await context.Readers.CountAsync()).Should().Be(2);
        (await context.Loans.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task EfBookRepository_ShouldSupportCaseInsensitiveLookupAndTrackingSafeUpdate()
    {
        // Arrange
        await using var context = CreateDbContext();
        await DbSeeder.SeedAsync(context);

        var bookRepository = new EfBookRepository(context);
        var copyRepository = new EfBookCopyRepository(context);
        var bookService = new BookService(bookRepository, copyRepository);

        // Act
        var found = await bookRepository.GetByIsbnAsync("978-0134494166");
        var updated = await bookService.UpdateBookAsync(
            found!.Id,
            "Clean Architecture 2nd Edition",
            "978-0134494166",
            "Robert C. Martin",
            "Software Engineering",
            2020);

        // Assert
        updated.Title.Should().Be("Clean Architecture 2nd Edition");
        updated.PublishedYear.Should().Be(2020);

        var reloaded = await bookRepository.GetByIdAsync(found.Id);
        reloaded!.Title.Should().Be("Clean Architecture 2nd Edition");
    }

    [Fact]
    public async Task LoanAndReservationLifecycle_WithEfRepositories_ShouldPersistAcrossReads()
    {
        // Arrange
        await using var context = CreateDbContext();
        await DbSeeder.SeedAsync(context);

        var copyRepo = new EfBookCopyRepository(context);
        var loanRepo = new EfLoanRepository(context);
        var reservationRepo = new EfReservationRepository(context);
        var readerRepo = new EfReaderRepository(context);
        var notificationService = new EfNotificationService(context);

        var reservationService = new ReservationService(copyRepo, reservationRepo, notificationService);
        var loanService = new LoanService(copyRepo, loanRepo, reservationRepo, notificationService, readerRepo);

        // Act: verify overdue reader, reserve copy 2 for reader 2, return loan 1, check notifications, issue copy 2 to reader 2
        var overdueReaders = await loanService.GetOverdueReadersAsync();
        await reservationService.ReserveBookCopyAsync(bookCopyId: 2, readerId: 2);
        await loanService.ReturnLoanAsync(loanId: 1);
        var notifications = await notificationService.GetAllAsync();
        var reservedCopy = await copyRepo.GetByIdAsync(2);
        await loanService.IssueLoanAsync(bookCopyId: 2, readerId: 2);
        var borrowedCopy = await copyRepo.GetByIdAsync(2);

        // Assert
        overdueReaders.Should().ContainSingle(reader => reader.ReaderId == 1);
        reservedCopy!.Status.Should().Be(BookCopyStatus.Reserved);
        notifications.Should().ContainSingle(record => record.ReaderId == 2 && record.BookCopyId == 2);
        borrowedCopy!.Status.Should().Be(BookCopyStatus.Borrowed);
    }
}
