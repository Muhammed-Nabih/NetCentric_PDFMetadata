using Microsoft.AspNetCore.Http;
using NetCentric_PDFMetadata.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCentric_PDFMetadata.BLL.Interfaces
{
    public interface IPdfService
    {
        Task<List<PdfMetadata>> ProcessFilesAsync(List<IFormFile> files, string uploadPath, string outputPath);
        PdfMetadata ExtractPdfMetadata(string filePath);
        Task<string> GeneratePdfMetadataReportAsync(List<PdfMetadata> pdfMetadataList, string outputPath, int totalPages);
        Task<bool> DeletePdfFileAsync(int pdfId, string uploadPath);
    }
}
