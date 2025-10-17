using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignaturePad.Data;
using SignaturePad.Models;
using System.Reflection.Metadata;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace SignaturePad.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SignatureList()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveSignature(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Json(new { success = false, message = "No image data provided" });

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();

                var signature = new Signature
                {
                    ImageData = imageBytes
                };

                _context.Signatures.Add(signature);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var signatures = await _context.Signatures
            .Select(s => new
            {
                s.Id,
                ImageBase64 = s.ImageData != null
                    ? "data:image/png;base64," + Convert.ToBase64String(s.ImageData)
                    : null
            })
            .ToListAsync();

            return Json(signatures);
        }
        public IActionResult ImageToPdf()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        [HttpPost]
        public IActionResult ExportToPdf(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                ViewBag.Message = "Please upload an image first.";
                return View("Index");
            }

            using var msImage = new MemoryStream();
            imageFile.CopyTo(msImage);
            msImage.Position = 0;

            using var pdfStream = new MemoryStream();
            using (var pdfDoc = new iTextSharp.text.Document(PageSize.A4, 0, 0, 0, 0))
            {
                PdfWriter.GetInstance(pdfDoc, pdfStream);
                pdfDoc.Open();

                var image = iTextSharp.text.Image.GetInstance(msImage.ToArray());
                image.ScaleAbsolute(pdfDoc.PageSize.Width, pdfDoc.PageSize.Height);
                image.SetAbsolutePosition(0, 0);
                pdfDoc.Add(image);
                pdfDoc.Close();
            }

            var pdfBytes = pdfStream.ToArray();
            string pdfFileName = Path.GetFileNameWithoutExtension(imageFile.FileName) + ".pdf";

            return File(pdfBytes, "application/pdf", pdfFileName);
        }
    }
}
