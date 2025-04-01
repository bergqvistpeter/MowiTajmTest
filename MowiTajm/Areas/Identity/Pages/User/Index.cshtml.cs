using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MowiTajm.Data;
using MowiTajm.Models;

namespace MowiTajm.Areas.Identity.Pages.User
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;
        private readonly ReviewService _reviewService;

        public IndexModel(IUserService userService, ApplicationDbContext context, ReviewService reviewService)
        {
            _userService = userService;
            _context = context;
            _reviewService = reviewService;
        }

        // S�krar nullv�rde genom att s�tta en tom lista som standardv�rde
        public List<Review> Reviews { get; set; } = new List<Review>();

        ////////////////////////////////////////////////////////////////////////////////////////////

        // Metod som h�mtar recensioner fr�n databasen baserat p� inloggad anv�ndare
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // H�mta anv�ndarkontext
                var user = await _userService.GetUserContextAsync(User);

                var displayName = user.DisplayName; //  Viktigt, inte UserName

                // H�mta recensioner fr�n databasen baserat p� anv�ndarens visningsnamn
                Reviews = await _context.Reviews
                    .Where(r => r.Username == displayName) //  Matcha Username mot DisplayName
                    .ToListAsync();

                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid h�mtning av recensioner: {ex.Message}");
                ViewData["ErrorMessage"] = "Ett fel uppstod vid h�mtning av recensioner. F�rs�k igen senare.";
                return Page();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        // Metod som tar bort recensioner fr�n databasen
        public async Task<IActionResult> OnPostDeleteReviewAsync(int reviewId)
        {
            try
            {
                await _reviewService.DeleteReviewAsync(reviewId);
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid borttagning av recension: {ex.Message}");
                ViewData["ErrorMessage"] = "Ett fel uppstod vid borttagning av recensionen. F�rs�k igen senare.";
                return Page();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        // Metod som hanterar redigering av recensioner
        public async Task<IActionResult> OnPostEditReviewAsync(int reviewId)
        {
            try
            {
                // H�mta recensionen fr�n databasen med reviewId
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                {
                    return NotFound();
                }

                // Skicka vidare till EditReviewPage med den h�r recensionen
                return RedirectToPage("/Movies/EditReviewPage", new { id = review.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid redigering av recension: {ex.Message}");
                ViewData["ErrorMessage"] = "Ett fel uppstod vid redigering av recensionen. F�rs�k igen senare.";
                return Page();
            }
        }
    }
}


