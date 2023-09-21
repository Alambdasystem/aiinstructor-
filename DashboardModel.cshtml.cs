using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YourApp.Pages
{
    public class DashboardModel : PageModel
    {
        public string VideoUrl { get; set; }

        public List<string> QuizQuestions { get; set; } = new List<string> { "Question 1", "Question 2", "Question 3" };

        public List<string> AIAssistantMessages { get; set; } = new List<string> { "AI Message 1", "AI Message 2", "AI Message 3" };
    }
}
