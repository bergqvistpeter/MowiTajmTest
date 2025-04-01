using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MowiTajm.Data;
using MowiTajm.Models;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public IndexModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public IList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public Dictionary<string, string> UserRoles { get; set; } = new Dictionary<string, string>();
    public IList<Review> Reviews { get; set; } = new List<Review>();

    [BindProperty]
    public bool ShowReviews { get; set; } = false; // Standard: Visa anv�ndare f�rst

    ////////////////////////////////////////////////////////////////////////////////////////////

    // Metod som laddar anv�ndarna vid sidans f�rsta uppslag
    public async Task OnGetAsync()
    {
        try
        {
            await LoadUsersAsync(); // Standard: Ladda anv�ndare direkt
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fel vid laddning av anv�ndare: {ex.Message}");
            ViewData["ErrorMessage"] = "Ett fel uppstod vid laddning av anv�ndare. F�rs�k igen senare.";
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////

    // Metod som laddar om anv�ndarna n�r knappen "Hantera anv�ndare" trycks
    public async Task<IActionResult> OnPostLoadUsersAsync()
    {
        try
        {
            ShowReviews = false;
            await LoadUsersAsync();
            return Page();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fel vid omladdning av anv�ndare: {ex.Message}");
            ViewData["ErrorMessage"] = "Ett fel uppstod vid omladdning av anv�ndare. F�rs�k igen senare.";
            return Page();
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////

    // Metod som laddar recensionerna n�r knappen "Hantera recensioner" trycks
    public IActionResult OnPostLoadReviewsAsync()
    {
        try
        {
            ShowReviews = true;
            Reviews = _context.Reviews.ToList();
            return Page();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fel vid laddning av recensioner: {ex.Message}");
            ViewData["ErrorMessage"] = "Ett fel uppstod vid laddning av recensioner. F�rs�k igen senare.";
            return Page();
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////

    // Metod som laddar anv�ndarna och deras roller fr�n databasen
    private async Task LoadUsersAsync()
    {
        try
        {
            Users = _userManager.Users.ToList();
            foreach (var user in Users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                UserRoles[user.Id] = roles.FirstOrDefault() ?? "Ingen roll";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fel vid laddning av anv�ndare och deras roller: {ex.Message}");
            ViewData["ErrorMessage"] = "Ett fel uppstod vid laddning av anv�ndare och roller. F�rs�k igen senare.";
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////

    // Metod som uppdaterar en anv�ndares roll
    public async Task<IActionResult> OnPostUpdateRoleAsync(string userId, string newRole)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            var result = await _userManager.AddToRoleAsync(user, newRole);

            if (!result.Succeeded) return BadRequest("Fel vid �ndring av roll.");

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fel vid uppdatering av anv�ndarroll: {ex.Message}");
            ViewData["ErrorMessage"] = "Ett fel uppstod vid uppdatering av anv�ndarens roll. F�rs�k igen senare.";
            return Page();
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////

    // Metod som tar bort en anv�ndare fr�n databasen
    public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded) return BadRequest("Fel vid borttagning av anv�ndare.");
            }
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fel vid borttagning av anv�ndare: {ex.Message}");
            ViewData["ErrorMessage"] = "Ett fel uppstod vid borttagning av anv�ndaren. F�rs�k igen senare.";
            return Page();
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////

    // Metod som tar bort en recension fr�n databasen
    public async Task<IActionResult> OnPostDeleteReviewAsync(int reviewId)
    {
        try
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fel vid borttagning av recension: {ex.Message}");
            ViewData["ErrorMessage"] = "Ett fel uppstod vid borttagning av recensionen. F�rs�k igen senare.";
            return Page();
        }
    }
}