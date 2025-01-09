using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCentric_PDFMetadata.DAL.Models
{
    public class PdfMetadata
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string? CreationDate { get; set; }
        public int NumberOfPages { get; set; }
        public bool IsComplete { get; set; }
    }
}
