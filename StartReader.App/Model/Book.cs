using Opportunity.MvvmUniverse.Collections;
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
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public ObservableList<BookSource> Sources { get; } = new ObservableList<BookSource>();

        [Required]
        public ObservableList<Chapter> ChaptersData { get; } = new ObservableList<Chapter>();
    }
}
