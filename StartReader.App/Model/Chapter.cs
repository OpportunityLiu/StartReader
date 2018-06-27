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
    class Chapter : ChapterDataDetailed
    {
        private Chapter() { }

        public Chapter(Book book, int index, ChapterDataBrief item)
            : this(book.Id, index, item)
        {
            this.Book = book;
        }

        public Chapter(int bookId, int index, ChapterDataBrief item)
        {
            this.BookId = bookId;
            this.Index = index;
            Update(item);
        }

        public void Update(ChapterDataBrief chapter)
        {
            if (chapter is Chapter db)
            {
                Debug.Assert(db.BookId == BookId);
                Debug.Assert(db.Index == Index);
                Debug.Assert(db.Book == Book || db.Book is null || Book is null);
            }

            Title = chapter.Title;
            VolumeTitle = chapter.VolumeTitle.CoalesceNullOrWhiteSpace(VolumeTitle);
            UpdateTime = chapter.UpdateTime ?? UpdateTime;
            if (chapter.WordCount >= 0)
                WordCount = chapter.WordCount;
            Key = chapter.Key;
            Preview = chapter.Preview.CoalesceNullOrWhiteSpace(Preview);
            if (chapter is ChapterDataDetailed detailed)
            {
                Content = detailed.Content.CoalesceNullOrWhiteSpace(Content);
            }
        }

        [ForeignKey("Book")]
        public int BookId { get; set; }
        [Required]
        public Book Book { get; set; }

        [Required]
        public int Index { get; set; }
    }
}
