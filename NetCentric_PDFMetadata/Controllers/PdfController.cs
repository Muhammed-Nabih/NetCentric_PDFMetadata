using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using NetCentric_PDFMetadata.BLL.Interfaces;
using NetCentric_PDFMetadata.DAL.Data;

namespace NetCentric_PDFMetadata.Controllers
{
    public class PdfController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IPdfService _pdfService;
        private readonly ApplicationDbContext _context;

        public PdfController(IWebHostEnvironment webHostEnvironment, IPdfService pdfService, ApplicationDbContext context)
        {
            _webHostEnvironment = webHostEnvironment;
            _pdfService = pdfService;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UploadFiles()
        {
            return View();
        }

        public IActionResult ViewUploadedFiles()
        {
            var pdfMetadataList = _context.PdfMetadata.ToList();
            return View(pdfMetadataList);
        }


        #region Process The Files When Upload
        [HttpPost]
        public async Task<IActionResult> ProcessFiles(List<IFormFile> files, string outputPath)
        {
            if (files == null || !files.Any())
            {
                ModelState.AddModelError("files", "You Must Upload at least one PDF file");
                return View("UploadFiles");
            }
            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            outputPath ??= Path.Combine(_webHostEnvironment.WebRootPath, "defaultDirectory");

            var pdfMetadataList = await _pdfService.ProcessFilesAsync(files, uploadPath, outputPath);
            if (!pdfMetadataList.Any())
            {
                ModelState.AddModelError("files", "Only PDF files");
                return View("UploadFiles");
            }

            int totalPages = pdfMetadataList.Sum(x => x.NumberOfPages);
            string reportPath = await _pdfService.GeneratePdfMetadataReportAsync(pdfMetadataList, outputPath, totalPages);
            ViewBag.ReportPath = reportPath;
            return RedirectToAction("ViewUploadedFiles");
        }

        #endregion

        #region Download the PDF
        public FileResult DownloadPdfMetadata(int id)
        {
            var metadata = _context.PdfMetadata.FirstOrDefault(m => m.Id == id);
            if (metadata == null)
            {
                return null;
            }

            string outputPath = Path.Combine(_webHostEnvironment.WebRootPath, $"{metadata.FileName}_Metadata.pdf");

            using (var writer = new PdfWriter(outputPath))
            using (var pdf = new PdfDocument(writer))
            using (var document = new Document(pdf))
            {
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "NetCentricLOGO", "ss.png");
                if (System.IO.File.Exists(imagePath))
                {
                    var image = new iText.Layout.Element.Image(iText.IO.Image.ImageDataFactory.Create(imagePath));
                    image.SetTextAlignment(TextAlignment.CENTER)
                         .SetWidth(200)
                         .SetHeight(100);
                    document.Add(image);
                }
                else
                {
                    document.Add(new Paragraph("Image not found").SetTextAlignment(TextAlignment.CENTER));
                }

                document.Add(new Paragraph("PDF Metadata Report")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(18));

                document.Add(new Paragraph("\n"));

                var table = new Table(2);
                table.SetWidth(500);
                table.AddHeaderCell("Key");
                table.AddHeaderCell("Value");

                table.AddCell("File Name");
                table.AddCell(metadata.FileName);

                table.AddCell("Title");
                table.AddCell(metadata.Title ?? "N/A");

                table.AddCell("Author");
                table.AddCell(metadata.Author ?? "N/A");

                table.AddCell("Creation Date");
                table.AddCell(metadata.CreationDate ?? "N/A");

                table.AddCell("Number of Pages");
                table.AddCell(metadata.NumberOfPages.ToString());

                document.Add(table);
                document.Add(new Paragraph("\n"));
                document.Add(new Paragraph("Created By Mohamed Nabih")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12));
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(outputPath);
            return File(fileBytes, "application/pdf", $"{metadata.FileName}_Metadata.pdf");
        }
        #endregion

        #region Delete the PDF
        [HttpPost]
        public async Task<IActionResult> DeleteFile(int pdfId)
        {
            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

            bool result = await _pdfService.DeletePdfFileAsync(pdfId, uploadPath);

            if (result)
            {
                TempData["Message"] = "File deleted successfully";
            }
            else
            {
                TempData["Message"] = "Failed to delete the file";
            }

            return RedirectToAction("ViewUploadedFiles");
        }
        #endregion
    }
}
