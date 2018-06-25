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
        public DbSet<Book> Books { get; private set; }

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
                        CoreEventId.SensitiveDataLoggingEnabledWarning,
                        CoreEventId.ModelValidationWarning));
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookSource>().HasOne(s => s.Book).WithMany(b => b.Sources).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Book>().HasOne(b => b.CurrentSource);
            modelBuilder.Entity<Book>().Property<string>("coverUri").HasColumnName(nameof(Book.CoverUri));
            modelBuilder.Entity<Book>().HasIndex(b => new { b.Title, b.Author }).IsUnique();

            modelBuilder.Entity<Book>().HasMany(b => b.ChaptersData).WithOne(c => c.Book).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
