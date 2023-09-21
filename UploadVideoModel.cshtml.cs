using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YourApp.Pages
{
    public class UploadVideoModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;

        public UploadVideoModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult OnPost(IFormFile videoFile)
        {
            if (videoFile == null || videoFile.Length == 0)
            {
                return BadRequest("Please select a valid video file.");
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + videoFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                videoFile.CopyTo(stream);
            }

            return RedirectToPage("/Dashboard", new { videoUrl = $"/uploads/{uniqueFileName}" });
        }
    }
}
