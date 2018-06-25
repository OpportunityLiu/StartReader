using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using StartReader.DataExchange.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartReader.App.Model
{
    class BookShelf : DbContext
    {
        public static BookShelf Create()
        {
            var r = new BookShelf();
            r.Database.EnsureCreated();
            return r;
        }

        public BookShelf() { }

        public DbSet<Book> Books { get; private set; }
        public DbSet<Chapter> Chapters { get; private set; }
        public DbSet<BookSource> BookSources { get; private set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=BookShelf.db");
#if DEBUG
            optionsBuilder
                .ConfigureWarnings(warnings => warnings
                    .Throw(
                        RelationalEventId.QueryClientEvaluationWarning,
                        RelationalEventId.AmbientTransactionWarning,
                        RelationalEventId.PossibleUnintendedUseOfEqualsWarning)
                    .Throw(
                        CoreEventId.IncludeIgnoredWarning,
                        CoreEventId.SensitiveDataLoggingEnabledWarning));
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookSource>().HasOne(s => s.Book).WithMany(b => b.Sources).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<BookSource>().HasIndex(s => s.BookKey);
            modelBuilder.Entity<BookSource>().HasIndex(s => new { s.ExtensionId, s.PackageFamilyName });

            modelBuilder.Entity<Book>().Property<string>("coverUri").HasColumnName(nameof(Book.CoverUri));
            modelBuilder.Entity<Book>().HasIndex(b => new { b.Title, b.Author }).IsUnique();

            modelBuilder.Entity<Chapter>().HasOne(c => c.Book).WithMany(b => b.ChaptersData).HasForeignKey("BookId").OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Chapter>().HasKey(nameof(Chapter.Index), "BookId");
        }
    }
}
