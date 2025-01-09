using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCentric_PDFMetadata.DAL.Models
{
    public class PdfReport
    {
        public List<PdfMetadata> FilesProcessed { get; set; }
        public List<string> FilesWithMissingMetadata { get; set; }
        public int TotalPages { get; set; }
    }
}
