using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MowiTajm.Models;

namespace MowiTajm.Pages.Movies
{
    public class MovieDetailsPageModel : PageModel
    {
        private readonly MovieService _movieService;
        private readonly IUserService _userService;
        private readonly ReviewService _reviewService;

        public MovieDetailsPageModel(MovieService movieService, IUserService userService, ReviewService reviewService)
        {
            _movieService = movieService;
            _userService = userService;
            _reviewService = reviewService;
        }

        public UserContext UserContext { get; set; } = new UserContext();
        public MovieFull Movie { get; set; } = new();

        [BindProperty]
        public Review Review { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();

        [BindProperty]
        public int FilterValue { get; set; } // Nummer som kontrollerer vilket filter som anv�nds
        public double MowiTajmRating { get; set; } // Genomsnittlig review f�r filmen baserat p� reviews p� MowiTajm
        public bool IsUserSignedIn => User.Identity?.IsAuthenticated ?? false;
        public string DateSortText = "";
        public List<Review> FilteredReviews { get; set; } = new();
        public bool IsStarFilterActive { get; set; } = false;

        ////////////////////////////////////////////////////////////////////////////////////////////

        // Metod som h�mtar filmens detaljer och recensioner fr�n databasen
        public async Task OnGetAsync(string imdbID)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(imdbID))
                {
                    await LoadMovieAndUserDataAsync(imdbID);

                    Review.ImdbID = imdbID; // Spara filmens ID i recensionen
                    FilterValue = 6; // Standard: Senaste
                    DateSortText = "Senaste";

                    // Anropa metoden f�r att filtrera recensioner baserat p� datum
                    Reviews = Reviews.OrderByDescending(r => r.DateTime).ToList();
                }
                else
                {
                    ViewData["ErrorMessage"] = "Filmens ID saknas eller �r ogiltigt.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid h�mtning av film och recensioner: {ex.Message}");
                ViewData["ErrorMessage"] = "Ett fel uppstod vid h�mtning av filmdetaljer eller recensioner.";
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        // Metod som l�gger till en recension i databasen
        public async Task<IActionResult> OnPostAddReview()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // H�mta filmdetaljer och recensioner igen om valideringen misslyckas
                    await LoadMovieAndUserDataAsync(Review.ImdbID);
                    return Page();
                }

                // Spara aktuellt datum och tid
                Review.DateTime = DateTime.Now;

                // Anropa ReviewService f�r att l�gga till recensionen
                await _reviewService.AddReviewAsync(Review);

                // Ladda om sidan och dess inneh�ll
                return RedirectToPage("MovieDetailsPage", new { imdbId = Review.ImdbID });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid till�gg av recension: {ex.Message}");
                ViewData["ErrorMessage"] = "Ett fel uppstod vid till�gg av recensionen.";
                return Page();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        // Metod som filtrerar recensioner baserat p� antal stj�rnor
        public async Task<IActionResult> OnPostStarFilter()
        {
            try
            {
                await LoadMovieAndUserDataAsync(Review.ImdbID);

                // Rensa listan med filtrerade recensioner
                FilteredReviews.Clear();

                // Filtrera recensionerna baserat p� stj�rnorna
                if (FilterValue >= 1 && FilterValue <= 5)
                {
                    FilteredReviews = Reviews.Where(r => r.Rating == FilterValue).ToList();
                    IsStarFilterActive = true;
                }
                else { IsStarFilterActive = false; }

                // Sortera de filtrerade recensionerna baserat p� datum
                DateSortText = "Senaste";
                FilteredReviews = FilteredReviews.OrderByDescending(r => r.DateTime).ToList();

                // �terg� till sidan med uppdaterad information
                TempData["ScrollToReviews"] = true;

                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid filtrering av recensioner: {ex.Message}");
                ViewData["ErrorMessage"] = "Ett fel uppstod vid filtrering av recensioner.";
                return Page();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        // Metod som sorterar recensioner baserat p� datum
        public async Task<IActionResult> OnPostDateFilter()
        {
            try
            {
                await LoadMovieAndUserDataAsync(Review.ImdbID);

                // L�s in f�reg�ende FilterValue om det finns, annars s�tt ett defaultv�rde (6 = "Senaste")
                if (TempData["FilterValue"] is int prevFilterValue) { FilterValue = prevFilterValue; }
                else { FilterValue = 6; }

                // Toggla FilterValue: om den �r 6 (nyaste) blir den 7 (�ldsta), annars blir den 6
                FilterValue = (FilterValue == 6) ? 7 : 6;

                // S�tt DateSortText baserat p� den nya FilterValue
                DateSortText = (FilterValue == 6) ? "Senaste" : "�ldsta";

                // Sortera recensionerna baserat p� FilterValue
                if (FilterValue == 6) { Reviews = Reviews.OrderByDescending(r => r.DateTime).ToList(); }
                else { Reviews = Reviews.OrderBy(r => r.DateTime).ToList(); }

                // Spara v�rdena i TempData f�r n�sta anrop
                TempData["FilterValue"] = FilterValue;
                TempData["DateSortOrder"] = DateSortText;
                TempData["ScrollToReviews"] = true; // S�tt flaggan f�r scroll

                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid sortering av recensioner: {ex.Message}");
                ViewData["ErrorMessage"] = "Ett fel uppstod vid sortering av recensioner.";
                return Page();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        // Metod som tar bort en recension fr�n databasen
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _reviewService.DeleteReviewAsync(id);
                await LoadMovieAndUserDataAsync(Review.ImdbID);

                // Ladda om sidan och dess inneh�ll
                return RedirectToPage("MovieDetailsPage", new { imdbId = Review.ImdbID });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid borttagning av recension: {ex.Message}");
                ViewData["ErrorMessage"] = "Ett fel uppstod vid borttagning av recensionen.";
                return Page();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        // Hj�lpmetod f�r att ladda filmens detaljer och anv�ndardata
        private async Task LoadMovieAndUserDataAsync(string imdbID)
        {
            try
            {
                // H�mta anv�ndardata fr�n service
                UserContext = await _userService.GetUserContextAsync(User);

                // H�mta filmen, recensioner och genomsnittlig rating fr�n service
                (Movie, Reviews, MowiTajmRating) = await _movieService.GetMovieDetailsAsync(imdbID);

                // S�tt MowiTajmRating i ViewData f�r att anv�ndas p� sidan
                ViewData["MowiTajmRating"] = MowiTajmRating;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid inl�sning av film och anv�ndardata: {ex.Message}");
                ViewData["ErrorMessage"] = "Ett fel uppstod vid h�mtning av film och anv�ndardata.";
            }
        }
    }
}
