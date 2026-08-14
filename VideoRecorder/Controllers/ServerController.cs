using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoRecorder.Database;
using VideoRecorder.Models;

namespace VideoRecorder.Controllers;

public class ServerController : Controller
{
    private readonly VideoRecorderContext _context;
    private readonly ILogger<UserController> _logger;

    public ServerController(VideoRecorderContext context, ILogger<UserController> logger)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        var servers =
            _context.Server
                .Include(s => s.Cameras)
                .FirstOrDefault();
        return View(servers);
    }
    
    // show the blank page first
    [HttpGet]
    public IActionResult AddServer()
    {
        return View(new Server());   // just show the empty form
    }

    public async Task<IActionResult> AddServer(Server server)
    {
        if (ModelState.IsValid) // check if the model is valid
        {
            _context.Add(server);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Server has been added.";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Server is invalid.");
            return View(new Server());
        }
        
    }
}