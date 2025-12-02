using System.Diagnostics;                    // Provides the Activity class used for tracking request IDs
using Microsoft.AspNetCore.Mvc;              // Provides Controller, IActionResult, and MVC attributes
using QuizApp.Models;                        // Includes the ErrorViewModel

namespace QuizApp.Controllers;

public class HomeController : Controller      // Controller for default site pages (Home, Privacy, etc.)
{
    private readonly ILogger<HomeController> _logger;   // Logger for recording information + errors

    // Constructor receives a logger automatically via Dependency Injection
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // Action for the home page (/Home/Index or /)
    public IActionResult Index()
    {
        return View();                       // Returns the Index.cshtml view
    }

    // Action for the Privacy page (/Home/Privacy)
    public IActionResult Privacy()
    {
        return View();                       // Returns the Privacy.cshtml view
    }

    // Displays an error page and prevents the error result from being cached
    [ResponseCache(Duration = 0, 
                   Location = ResponseCacheLocation.None, 
                   NoStore = true)]
    public IActionResult Error()
    {
        // Create a model with the current request's ID
        return View(new ErrorViewModel 
        { 
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
        });
    }
}
