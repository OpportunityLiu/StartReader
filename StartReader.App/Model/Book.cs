using Opportunity.MvvmUniverse.Collections;
using StartReader.App.Extensiton;
using StartReader.DataExchange.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartReader.App.Model
{
    class Book : BookDataDetailed
    {
        private Book() { }

        public Book(BookDataBrief book, DataSource dataSource)
        {
            Title = book.Title;
            Author = book.Author;
            ExtensionId = dataSource.ExtensionId;
            PackageFamilyName = dataSource.PackageFamilyName;
            ChaptersData = new ObservableList<Chapter>();
            Update(book, dataSource);
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public ObservableList<Chapter> ChaptersData { get; private set; }

        [Required]
        public string PackageFamilyName { get; set; }

        [Required]
        public string ExtensionId { get; set; }

        public DataSource FindSource()
            => DataSourceManager.Instance.Sources.FirstOrDefault(item =>
                item.IsAvailable && item.PackageFamilyName == PackageFamilyName && item.ExtensionId == ExtensionId);

        public void Update(BookDataBrief book, DataSource dataSource)
        {
            Debug.Assert(!(book is Book));
            Debug.Assert(book.Title == Title);
            Debug.Assert(book.Author == Author);
            Debug.Assert(dataSource.ExtensionId == ExtensionId);
            Debug.Assert(dataSource.PackageFamilyName == PackageFamilyName);

            LastestChapter = book.LastestChapter;
            Tags = book.Tags;

            AlternativeTitle = book.AlternativeTitle.CoalesceNullOrWhiteSpace(AlternativeTitle);
            if (Description.CoalesceNullOrEmpty("").Length <= book.Description.CoalesceNullOrWhiteSpace("").Length)
                Description = book.Description;
            if (!book.CoverData.IsNullOrEmpty())
                CoverData = book.CoverData;
            if (book.CoverUri != null)
                CoverUri = book.CoverUri;
            IsFinished = book.IsFinished;
            if (book.WordCount >= 0)
                WordCount = book.WordCount;
            Key = book.Key ?? Key;

            if (book is BookDataDetailed detailed && !detailed.Chapters.IsNullOrEmpty() && ChaptersData != null)
            {
                Description = book.Description.CoalesceNullOrWhiteSpace(Description);
                var newChpData = new List<Chapter>();
                var i = 0;
                foreach (var item in detailed.Chapters)
                {
                    newChpData.Add(new Chapter(this, i, item));
                    i++;
                }
                ChaptersData.Update(newChpData,
                    (o, n) => o.Index.CompareTo(n.Index),
                    (existChp, newChp) => existChp.Update(newChp));
            }
        }
    }
}
