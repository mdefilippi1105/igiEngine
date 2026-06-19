using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VideoRecorder.Database;
using VideoRecorder.Models;

namespace VideoRecorder.Controllers;

public class UserController : Controller
{
    private readonly VideoRecorderContext _context;
    private readonly ILogger<UserController> _logger;
    private readonly PasswordHasher<User> _hasher;

    public UserController(VideoRecorderContext context, ILogger<UserController> logger)
    {
        _logger = logger;
        _context = context;
    }
    
    public IActionResult Index()
    {
        var users = _context.User.ToList();
        return View(users);
    }
    
    public IActionResult Create()
    {
        return View(); //show the cshtml
    }
    
    // this method only answers POST requests and rejects the
    //  POST unless it has forgery token from razor page <form>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user)
    {
        // never store the plain text. salt and hash it > then save into PasswordHash
        user.PasswordHash = _hasher.HashPassword(user, user.PasswordHash);
        _context.Add(user);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

