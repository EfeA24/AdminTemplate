using Microsoft.AspNetCore.Mvc;

namespace TrivaWebPage.Controllers;

public class HelpController : Controller
{
    public IActionResult Index() => View();
}
