using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using StartReader.App.Model;

namespace StartReader.App.Migrations
{
    [DbContext(typeof(BookShelf))]
    partial class BookShelfModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.5");

            modelBuilder.Entity("StartReader.App.Model.Book", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AlternativeTitle");

                    b.Property<string>("Author");

                    b.Property<int?>("CurrentSourceId");

                    b.Property<string>("Description");

                    b.Property<string>("Key");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.HasIndex("Author");

                    b.HasIndex("CurrentSourceId");

                    b.HasIndex("Title");

                    b.ToTable("Books");
                });

            modelBuilder.Entity("StartReader.App.Model.BookSource", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("BookId");

                    b.Property<string>("BookKey");

                    b.Property<string>("ProviderId");

                    b.Property<string>("SourcePFN");

                    b.HasKey("Id");

                    b.HasIndex("BookId");

                    b.ToTable("BookSource");
                });

            modelBuilder.Entity("StartReader.App.Model.Chapter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("BookId");

                    b.Property<string>("BookKey");

                    b.Property<string>("Content");

                    b.Property<string>("Discriminator");

                    b.Property<string>("Key");

                    b.Property<string>("Preview");

                    b.Property<int?>("SourceId");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.HasIndex("BookId");

                    b.HasIndex("SourceId");

                    b.ToTable("Chapter");

                    b.HasDiscriminator().HasValue("Chapter");
                });

            modelBuilder.Entity("StartReader.App.Model.Book", b =>
                {
                    b.HasOne("StartReader.App.Model.BookSource", "CurrentSource")
                        .WithMany()
                        .HasForeignKey("CurrentSourceId");
                });

            modelBuilder.Entity("StartReader.App.Model.BookSource", b =>
                {
                    b.HasOne("StartReader.App.Model.Book", "Book")
                        .WithMany("Sources")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("StartReader.App.Model.Chapter", b =>
                {
                    b.HasOne("StartReader.App.Model.Book", "Book")
                        .WithMany("Chapters")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("StartReader.App.Model.BookSource", "Source")
                        .WithMany()
                        .HasForeignKey("SourceId");
                });
        }
    }
}
