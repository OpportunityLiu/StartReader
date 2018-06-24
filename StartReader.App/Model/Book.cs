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
    class Book : BookDataBrief
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public BookSource CurrentSource { get; set; }

        public List<BookSource> Sources { get; set; } = new List<BookSource>();

        public List<Chapter> Chapters { get; set; } = new List<Chapter>();
    }
}
