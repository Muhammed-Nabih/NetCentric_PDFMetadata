using Microsoft.EntityFrameworkCore;
using NetCentric_PDFMetadata.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCentric_PDFMetadata.DAL.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<PdfMetadata> PdfMetadata { get; set; }
    }
}
