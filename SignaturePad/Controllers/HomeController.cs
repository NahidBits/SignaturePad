using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignaturePad.Data;
using SignaturePad.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

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
        public async Task<IActionResult> SaveSignature([FromBody] SignatureRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ImageData))
                    return Json(new { success = false, message = "No image data provided" });

                var base64Data = Regex.Replace(request.ImageData, "^data:image\\/png;base64,", string.Empty);

                byte[] bytes = Convert.FromBase64String(base64Data);

                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Signatures");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = $"signature_{DateTime.Now:yyyyMMddHHmmssfff}.png";
                var filePath = Path.Combine(folderPath, fileName);

                await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                var signature = new Signature
                {
                    ImagePath = "/Signatures/" + fileName
                };

                    _context.Signatures.Add(signature);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    path = signature.ImagePath
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
                    s.ImagePath
                })
                .ToListAsync();

            return Json(signatures);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
