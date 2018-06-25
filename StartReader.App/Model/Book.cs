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
    class Book : BookDataDetailed
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public BookSource CurrentSource { get; set; }

        [Required]
        public List<BookSource> Sources { get; private set; }

        [Required]
        public List<Chapter> ChaptersData { get; private set; }
    }
}
