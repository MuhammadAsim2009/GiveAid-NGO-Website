
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Give_AID.Models;

public class UsersController : Controller
{
    private readonly Give_AIDContext _context;

    public UsersController(Give_AIDContext context)
    {
        _context = context;
    }

    // GET: USERSS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Users.ToListAsync());
    }

    // GET: USERSS/Details/5
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

    // GET: USERSS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: USERSS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

    // GET: USERSS/Edit/5
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

    // POST: USERSS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

    // GET: USERSS/Delete/5
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

    // POST: USERSS/Delete/5
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
