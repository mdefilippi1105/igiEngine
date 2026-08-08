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
    private readonly IWebHostEnvironment _env;

    public UserController(VideoRecorderContext context, 
        ILogger<UserController> logger,
        IWebHostEnvironment env)
    {
        _logger = logger;
        _context = context;
        _hasher = new PasswordHasher<User>();
        _env = env;
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
    public async Task<IActionResult> Create(User user, IFormFile? photo)
    {
        user.PhotoPath = await SavePhotoAsync(photo);
        // never store the plain text. salt and hash it > then save into PasswordHash
        user.PasswordHash = _hasher.HashPassword(user, user.PasswordHash);
        
        _context.Add(user);
        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }
    
    // this method will save the photo. only this controller calls it.
    // the string is the web path.
    private async Task<string?> SavePhotoAsync(IFormFile? photo)
    {
        if (photo == null || photo.Length == 0)
            return null;
        
        var folder = Path.Combine(_env.WebRootPath, "uploads", "users");
        Directory.CreateDirectory(folder);
        
        // create a guid and photofilename as fileName
        var fileName = Guid.NewGuid() + Path.GetExtension(photo.FileName);
        
        //combine for full path
        var fullPath = Path.Combine(folder, fileName);
        
        //create and dispose stream later
        await using var stream = System.IO.File.Create(fullPath);
        await photo.CopyToAsync(stream);
        
        return "/uploads/users/" + fileName;
    }
    
    // delete the user
    public async Task<IActionResult> RemoveUser(Guid id)
    {
        try
        {
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Nothing to delete!";
                return NotFound();
            }
            _context.Remove(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = "User removed from database.";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            TempData["Error"] = "Could not remove User.";
        }
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
    public async Task<IActionResult> SaveEditUser(User user, IFormFile? photo)
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
        
        //write the file to disk and hands back its webpath
        var newPath = await SavePhotoAsync(photo);
        
        // only overwrite when a real file comes in
        // point DB at new file, skip if new path is null so keep the old photo
        if (newPath != null)
            existingUser.PhotoPath = newPath;
        
        
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

