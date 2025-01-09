using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NetCentric_PDFMetadata.BLL.Interfaces;
using NetCentric_PDFMetadata.DAL.Data;
using NetCentric_PDFMetadata.DAL.Models;
using Newtonsoft.Json;


namespace NetCentric_PDFMetadata.BLL.Repositories
{

    public class PdfService : IPdfService
    {

        private readonly ApplicationDbContext _context;

        public PdfService(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Preocces the File and Upload
        public async Task<List<PdfMetadata>> ProcessFilesAsync(List<IFormFile> files, string uploadPath, string outputPath)
        {
            var pdfMetadataList = new List<PdfMetadata>();
            var allowedExtensions = new[] { ".pdf" };

            files = files.Where(file => allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower())).ToList();
            if (!files.Any()) return pdfMetadataList;

            Directory.CreateDirectory(uploadPath);
            Directory.CreateDirectory(outputPath);

            foreach (var file in files)
            {
                string filePath = Path.Combine(uploadPath, file.FileName);
                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var metadata = ExtractPdfMetadata(filePath);
                metadata.Author = string.IsNullOrEmpty(metadata.Author) ? "Unknown" : metadata.Author;
                metadata.Title = string.IsNullOrEmpty(metadata.Title) ? "Untitled" : metadata.Title;
                metadata.CreationDate = string.IsNullOrEmpty(metadata.CreationDate) ? "Null" : metadata.CreationDate;
                metadata.IsComplete = !string.IsNullOrEmpty(metadata.Title) &&
                                       !string.IsNullOrEmpty(metadata.Author) &&
                                       metadata.CreationDate != "Null" &&
                                       metadata.NumberOfPages > 0 &&
                                       metadata.Author != "Unknown" &&
                                       metadata.Title != "Untitled";

                pdfMetadataList.Add(metadata);

                _context.PdfMetadata.Add(metadata);
            }

            await _context.SaveChangesAsync();

            return pdfMetadataList;
        }
        #endregion


        #region Extract the data from PDF
        public PdfMetadata ExtractPdfMetadata(string filePath)
        {
            try
            {
                using var pdfReader = new PdfReader(filePath);
                using var pdfDocument = new PdfDocument(pdfReader);

                var info = pdfDocument.GetDocumentInfo();
                string creationDate = info.GetMoreInfo("CreationDate");
                DateTime parsedDate = DateTime.MinValue;

                if (!string.IsNullOrEmpty(creationDate))
                {
                    string datePart = creationDate.Split('+')[0].Replace("D:", "");
                    if (DateTime.TryParseExact(datePart, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out parsedDate))
                    {

                        creationDate = parsedDate.ToString("dd/MM/yyyy 'at' hh:mm tt");
                    }
                    else
                    {
                        creationDate = "Invalid date format";
                    }
                }

                return new PdfMetadata
                {
                    FileName = Path.GetFileName(filePath),
                    Title = info.GetTitle() ?? "Untitled",
                    Author = info.GetAuthor() ?? "Unknown",
                    CreationDate = creationDate,
                    NumberOfPages = pdfDocument.GetNumberOfPages()
                };
            }
            catch (Exception)
            {
                return new PdfMetadata
                {
                    FileName = Path.GetFileName(filePath),
                    Title = null,
                    Author = null,
                    CreationDate = null,
                    NumberOfPages = 0
                };
            }
        }
        #endregion

        #region Create the pdf Report
        public async Task<string> GeneratePdfMetadataReportAsync(List<PdfMetadata> pdfMetadataList, string outputPath, int totalPages)
        {
            string reportPath = Path.Combine(outputPath, "pdf_Report.json");

            PdfReport report;
            if (File.Exists(reportPath))
            {
                var existingReportJson = await System.IO.File.ReadAllTextAsync(reportPath);
                report = JsonConvert.DeserializeObject<PdfReport>(existingReportJson);
            }
            else
            {
                report = new PdfReport
                {
                    FilesProcessed = new List<PdfMetadata>(),
                    FilesWithMissingMetadata = null,
                    TotalPages = 0
                };
            }

            report.FilesProcessed.AddRange(pdfMetadataList);

            report.TotalPages += pdfMetadataList.Sum(m => m.NumberOfPages);

            string reportJson = JsonConvert.SerializeObject(report, Formatting.Indented);

            await System.IO.File.WriteAllTextAsync(reportPath, reportJson);

            return reportPath;
        }
        #endregion

        #region Delete the file
        public async Task<bool> DeletePdfFileAsync(int pdfId, string uploadPath)
        {
            try
            {
                var pdfMetadata = await _context.PdfMetadata.FirstOrDefaultAsync(pdf => pdf.Id == pdfId);
                if (pdfMetadata == null)
                {
                    return false;
                }

                string filePath = Path.Combine(uploadPath, pdfMetadata.FileName);
              
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _context.PdfMetadata.Remove(pdfMetadata);
                await _context.SaveChangesAsync();
                return true; 
            }
            catch (Exception)
            {
                return false; 
            }
        }
        #endregion


    }
}
