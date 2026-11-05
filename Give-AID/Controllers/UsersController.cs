using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID;
using Give_AID.Models;

public class UsersController : Controller
{
    private readonly Give_AIDContext _context;

    public UsersController(Give_AIDContext context)
    {
        _context = context;
    }

    // GET: Users
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Users.ToListAsync());
    }

    // GET: Users/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var users = await _context.Users
            .FirstOrDefaultAsync(m => m.Id == id);
        if (users == null)
        {
            return NotFound();
        }

        return View(users);
    }

    // GET: Users/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Email,Password,Role,Phone,Address,Occupation,Organization")] Users users)
    {
        if (ModelState.IsValid)
        {
            _context.Add(users);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(users);
    }

    // GET: Users/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var users = await _context.Users.FindAsync(id);
        if (users == null)
        {
            return NotFound();
        }
        return View(users);
    }

    // POST: Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Email,Password,Role,Phone,Address,Occupation,Organization")] Users users)
    {
        if (id != users.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(users);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(users.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(users);
    }

    // GET: Users/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var users = await _context.Users
            .FirstOrDefaultAsync(m => m.Id == id);
        if (users == null)
        {
            return NotFound();
        }

        return View(users);
    }

    // POST: Users/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var users = await _context.Users.FindAsync(id);
        if (users != null)
        {
            _context.Users.Remove(users);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool UsersExists(int? id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}
