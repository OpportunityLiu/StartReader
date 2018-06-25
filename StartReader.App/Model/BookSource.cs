using StartReader.App.Extensiton;
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

        [Required]
        public Book Book { get; set; }

        [Required]
        public string PackageFamilyName { get; set; }

        [Required]
        public string ExtensionId { get; set; }

        [Required]
        public string BookKey { get; set; }

        public DataSource FindSource()
        {
            return DataSourceManager.Instance.Sources.FirstOrDefault(item => item.IsAvailable && item.PackageFamilyName == PackageFamilyName && item.ExtensionId == ExtensionId);
        }
    }
}
