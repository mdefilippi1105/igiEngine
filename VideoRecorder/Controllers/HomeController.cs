using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using VideoRecorder.Database;
using VideoRecorder.Models;

namespace VideoRecorder.Controllers;

public class HomeController : Controller
{
    private readonly VideoRecorderContext _context;
    private readonly ILogger<UserController> _logger;
    private readonly PasswordHasher<User> _hasher;

    public HomeController(VideoRecorderContext context, ILogger<UserController> logger)
    {
        _logger = logger;
        _context = context;
        _hasher = new PasswordHasher<User>();
    }

    public IActionResult Index()
    {
        var users = _context.User.ToList();
        return View(users);
    }
}