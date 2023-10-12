using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
            try
            {
                var connectionString = "your_connection_string_here";
                object connection = optionsBuilder.UseSqlServer(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error configuring database: {ex.Message}");
            }
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
            try
            {
                Videos = LoadVideosFromDatabase();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading videos from database: {ex.Message}");
                Videos = new List<VideoMetadata>(); // Initialize to empty list on error
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await UploadAndProcessVideoAsync(VideoFile, VideoTitle, VideoDescription);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading and processing video: {ex.Message}");
            }

            return RedirectToPage();
        }

        private List<VideoMetadata> LoadVideosFromDatabase()
        {
            try
            {
                using (var dbContext = new YourDbContext())
                {
                    return dbContext.VideoMetadatas.ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading videos from database: {ex.Message}");
                return new List<VideoMetadata>(); // Return empty list on error
            }
        }

        private async Task UploadAndProcessVideoAsync(IFormFile videoFile, string title, string description)
        {
            try
            {
                var videoUrl = await UploadToBlobStorage(videoFile);
                var insights = await ProcessWithVideoIndexer(videoUrl, videoFile.FileName);
                await StoreMetadataInDatabase(title, description, videoUrl, insights);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UploadAndProcessVideoAsync: {ex.Message}");
            }
        }

        private async Task<string> UploadToBlobStorage(IFormFile videoFile)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading to Blob Storage: {ex.Message}");
                return string.Empty; // Return empty string on error
            }
        }

        private async Task<string> ProcessWithVideoIndexer(string videoUrl, string videoName)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing with Video Indexer: {ex.Message}");
                return string.Empty; // Return empty string on error
            }
        }

        private async Task StoreMetadataInDatabase(string title, string description, string videoUrl, string insights)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error storing metadata in database: {ex.Message}");
            }
        }
    }
}
