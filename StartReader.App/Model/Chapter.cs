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
        [ForeignKey("Book")]
        public int BookId { get; set; }
        [Required]
        public Book Book { get; set; }

        public int Index { get; set; }

        [ForeignKey("Source")]
        public int SourceId { get; set; }
        [Required]
        public BookSource Source { get; set; }
    }
}
