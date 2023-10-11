using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIChat.Pages
{
    public class VideoMetadata
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public string Insights { get; set; }
    }

    public class YourDbContext : DbContext
    {
        public DbSet<VideoMetadata> VideoMetadatas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "your_connection_string_here";
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        [BindProperty]
        public IFormFile VideoFile { get; set; }

        [BindProperty]
        public string VideoTitle { get; set; }

        [BindProperty]
        public string VideoDescription { get; set; }

        public List<VideoMetadata> Videos { get; set; }

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            Videos = LoadVideosFromDatabase();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await UploadAndProcessVideoAsync(VideoFile, VideoTitle, VideoDescription);
            return RedirectToPage();
        }

        private List<VideoMetadata> LoadVideosFromDatabase()
        {
            using (var dbContext = new YourDbContext())
            {
                return dbContext.VideoMetadatas.ToList();
            }
        }

        private async Task UploadAndProcessVideoAsync(IFormFile videoFile, string title, string description)
        {
            var videoUrl = await UploadToBlobStorage(videoFile);
            var insights = await ProcessWithVideoIndexer(videoUrl, videoFile.FileName);
            await StoreMetadataInDatabase(title, description, videoUrl, insights);
        }

        private async Task<string> UploadToBlobStorage(IFormFile videoFile)
        {
            string connectionString = _configuration.GetConnectionString("BlobStorage");
            string containerName = "your_container_name";
            var blobServiceClient = new BlobServiceClient(connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(videoFile.FileName);

            using (var stream = videoFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.ToString();
        }

        private async Task<string> ProcessWithVideoIndexer(string videoUrl, string videoName)
        {
            string apiKey = _configuration["VideoIndexer:ApiKey"];
            string accountId = _configuration["VideoIndexer:AccountId"];
            string location = _configuration["VideoIndexer:Location"];
            string apiUrl = $"https://api.videoindexer.ai/{location}/Accounts/{accountId}/Videos?accessToken={apiKey}&name={videoName}&videoUrl={videoUrl}";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(apiUrl, null);
                var content = await response.Content.ReadAsStringAsync();
                var videoIndex = JsonSerializer.Deserialize<JsonElement>(content);
                var insights = videoIndex.GetProperty("insights").ToString();

                return insights;
            }
        }

        private async Task StoreMetadataInDatabase(string title, string description, string videoUrl, string insights)
        {
            var videoMetadata = new VideoMetadata
            {
                Title = title,
                Description = description,
                VideoUrl = videoUrl,
                Insights = insights
            };

            using (var dbContext = new YourDbContext())
            {
                dbContext.VideoMetadatas.Add(videoMetadata);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}