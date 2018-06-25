using StartReader.DataExchange.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartReader.App.Model
{
    class Chapter : ChapterDataDetailed
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Index { get; set; }

        [Required]
        public Book Book { get; set; }

        [Required]
        public BookSource Source { get; set; }
    }
}
