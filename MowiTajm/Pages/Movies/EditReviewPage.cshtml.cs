using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MowiTajm.Data;
using MowiTajm.Models;

namespace MowiTajm.Pages.Movies
{
    public class EditReviewPageModel : PageModel
    {
        private readonly ApplicationDbContext _database;
        private readonly IUserService _userService;

        public EditReviewPageModel(ApplicationDbContext database, IUserService userService)
        {
            _database = database;
            _userService = userService;
        }

        [BindProperty]
        public Review Review { get; set; } = new();

        public bool IsUserSignedIn => User.Identity?.IsAuthenticated ?? false;
        public string DisplayName { get; set; } = string.Empty;

        ////////////////////////////////////////////////////////////////////////////////////////////

        // Metod som k�rs n�r sidan laddas
        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                // Nullcheck innan vi tilldelar Review det h�mtade v�rdet
                Review? review = await _database.Reviews.FindAsync(id);
                if (review == null) { return NotFound(); }
                Review = review;

                // H�mta anv�ndarkontext
                var userContext = await _userService.GetUserContextAsync(User);

                // H�mter anv�ndarens visningsnamn eller s�tter det till "Ok�nd anv�ndare"
                DisplayName = userContext?.DisplayName ?? "Ok�nd anv�ndare";

                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid h�mtning av recension: {ex.Message}");
                ViewData["ErrorMessage"] = "Ett fel uppstod vid h�mtning av recensionen. F�rs�k igen senare.";
                return Page();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        // Metod som k�rs n�r formul�ret postas
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Kontrollerar att recensionen finns i databasen
                if (Review == null) { return NotFound(); }

                // Kontrollerar att formul�ret �r korrekt ifyllt
                if (!ModelState.IsValid) { return Page(); }

                Review.DateTime = DateTime.Now;

                // Uppdaterar databasen med den nya recensionen
                _database.Reviews.Update(Review);
                await _database.SaveChangesAsync();

                // Returnerar anv�ndaren till MovieDetailsPage och filmen som recensionen tillh�r
                return RedirectToPage("MovieDetailsPage", new { imdbID = Review.ImdbID });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid uppdatering av recension: {ex.Message}");
                ViewData["ErrorMessage"] = "Ett fel uppstod vid uppdatering av recensionen. F�rs�k igen senare.";
                return Page();
            }
        }
    }
}
