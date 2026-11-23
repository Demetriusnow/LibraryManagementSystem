using Library.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.DAL.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Author> Authors => Set<Author>();

    public DbSet<Book> Books => Set<Book>();

    public DbSet<BookCopy> BookCopies => Set<BookCopy>();

    public DbSet<Reader> Readers => Set<Reader>();

    public DbSet<Loan> Loans => Set<Loan>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    public DbSet<NotificationRecord> Notifications => Set<NotificationRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(author => author.Id);
            entity.Property(author => author.Name)
                .IsRequired()
                .HasMaxLength(200);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(book => book.Id);
            entity.Property(book => book.Title)
                .IsRequired()
                .HasMaxLength(300);
            entity.Property(book => book.ISBN)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(book => book.Category)
                .IsRequired()
                .HasMaxLength(100);
            entity.HasIndex(book => book.ISBN)
                .IsUnique();

            entity.HasOne(book => book.Author)
                .WithMany()
                .HasForeignKey(book => book.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BookCopy>(entity =>
        {
            entity.HasKey(copy => copy.Id);
            entity.Property(copy => copy.InventoryNumber)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(copy => copy.Status)
                .HasConversion<string>()
                .HasMaxLength(30);
            entity.HasIndex(copy => copy.InventoryNumber)
                .IsUnique();

            entity.HasOne<Book>()
                .WithMany()
                .HasForeignKey(copy => copy.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Reader>(entity =>
        {
            entity.HasKey(reader => reader.Id);
            entity.Property(reader => reader.FullName)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(reader => reader.Email)
                .IsRequired()
                .HasMaxLength(200);
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(loan => loan.Id);
            entity.Property(loan => loan.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.HasOne<BookCopy>()
                .WithMany()
                .HasForeignKey(loan => loan.BookCopyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(reservation => reservation.Id);
            entity.Property(reservation => reservation.Status)
                .HasConversion<string>()
                .HasMaxLength(30);
            entity.Property(reservation => reservation.NotificationMessage)
                .HasMaxLength(500);

            entity.HasOne<BookCopy>()
                .WithMany()
                .HasForeignKey(reservation => reservation.BookCopyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationRecord>(entity =>
        {
            entity.HasKey(record => record.Id);
            entity.Property(record => record.Message)
                .IsRequired()
                .HasMaxLength(500);
        });
    }
}
