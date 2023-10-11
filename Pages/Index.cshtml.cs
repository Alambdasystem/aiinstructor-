using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AIChat.Pages
{
    public class IndexModel : PageModel

        
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
        // list of all the videos
        public List<Video> Videos { get; set; } = new List<Video>();
        // list of all the videos
        //url of the video, description and title of video
        public class Video
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string VideoUrl { get; set; }
        }
    }
}