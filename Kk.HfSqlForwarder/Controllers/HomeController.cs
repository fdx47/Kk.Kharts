using Microsoft.AspNetCore.Mvc;

namespace HfSqlForwarder.Controllers;

public class HomeController : Controller
{
    [HttpGet("/admin")]
    public IActionResult Admin()
    {
        return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "admin", "index.html"), "text/html");
    }
    
    [HttpGet("/")]
    public IActionResult Index()
    {
        return Redirect("/admin");
    }
}
