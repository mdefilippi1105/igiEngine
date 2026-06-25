using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
        _hasher = new PasswordHasher<User>();
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
    // edit users
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await _context.User.FindAsync(id);
        if (user == null)
        {
            TempData["Error"] = "Could not find user.";
            return NotFound();
        }
        return View(user);
    }

    [HttpPost] // only answer's post requests
    //this method runs async (await the db) User model binding
    // asp.net reads the posted form fields and auto builds
    // a user object from them.
    public async Task<IActionResult> SaveEditUser(User user)
    {
        // if invalid; return and show the user the error of their ways
        if (!ModelState.IsValid) 
        {
            return View("Edit", user);
        }
        
        //grab a real existing user from the db. find by the User GUID via form user selection
        var existingUser = await _context.User.FindAsync(user.Id);
        
        if (existingUser == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("Index");
        }
        
        existingUser.Username = user.Username;
        existingUser.Role = user.Role;
        existingUser.Email = user.Email;
        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.PhoneNumber = user.PhoneNumber;
        existingUser.Department = user.Department;
        existingUser.IsEnabled = user.IsEnabled;
        
        // this is the password guard. if the field is not empty,
        // it will generate the new password hash. if the field is
        // empty, then keep the existing hash.
        if (!string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            existingUser.PasswordHash = _hasher.HashPassword(existingUser, user.PasswordHash);
        }
        
        //save the changes
        await _context.SaveChangesAsync();
        TempData["Success"] = "User updated successfully!";
        return RedirectToAction(nameof(Index));
    }
}

