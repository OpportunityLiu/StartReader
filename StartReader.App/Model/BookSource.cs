using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartReader.App.Model
{
    class BookSource
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Book Book { get; set; }

        public string SourcePFN { get; set; }
        public string ProviderId { get; set; }
        public string BookKey { get; set; }
    }
}
