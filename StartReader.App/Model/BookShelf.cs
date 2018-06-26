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
            r.Database.Migrate();
            return r;
        }

        public static async Task InitAsync()
        {
            using (var r = new BookShelf())
            {
                await r.Database.MigrateAsync();
                var book = await r.Books.Include(b => b.Sources).Include(b => b.ChaptersData).FirstOrDefaultAsync();
            }
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
            modelBuilder.Entity<BookSource>()
                .HasOne(s => s.Book)
                .WithMany(b => b.Sources)
                .HasForeignKey(s => s.BookId)
                .HasPrincipalKey(b => b.Id)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<BookSource>()
                .HasIndex(s => s.BookKey);
            modelBuilder.Entity<BookSource>()
                .HasIndex(s => new { s.PackageFamilyName, s.ExtensionId });

            modelBuilder.Entity<Book>()
                .Property<string>("coverUri")
                .HasColumnName(nameof(Book.CoverUri));
            modelBuilder.Entity<Book>()
                .HasIndex(b => new { b.Title, b.Author })
                .IsUnique();

            modelBuilder.Entity<Chapter>()
                .HasOne(c => c.Book)
                .WithMany(b => b.ChaptersData)
                .HasForeignKey(c => c.BookId)
                .HasPrincipalKey(b => b.Id)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Chapter>()
                .HasOne(c => c.Source)
                .WithMany()
                .HasForeignKey(c => c.SourceId)
                .HasPrincipalKey(s => s.Id)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Chapter>()
                .HasKey(c => new { c.BookId, c.Index });
        }
    }
}
