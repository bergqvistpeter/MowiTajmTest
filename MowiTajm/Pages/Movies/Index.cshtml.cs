using Microsoft.AspNetCore.Mvc.RazorPages;
using MowiTajm.Models;
using MowiTajm.Services;

namespace MowiTajm.Pages.Movies
{
    public class IndexModel : PageModel
    {
        private readonly OmdbService _omdbService;

        public IndexModel(OmdbService omdbService)
        {
            _omdbService = omdbService;
        }

        public List<MovieLite> Movies { get; set; } = new();
        public string TotalResults { get; set; } = "";
        public string SearchInput { get; set; } = "";
        public bool IsUserSignedIn => User.Identity?.IsAuthenticated ?? false;

        ////////////////////////////////////////////////////////////////////////////////////////////

        // Metod som h�mtar filmer fr�n OMDb baserat p� s�kstr�ng
        public async Task OnGetAsync(string searchInput)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(searchInput))
                {
                    // Spara s�kstr�ngen f�r att kunna anv�nda den i vyn
                    SearchInput = searchInput;

                    // Anropa API:et f�r att s�ka efter filmer och h�mta resultatet
                    var result = await _omdbService.SearchMoviesAsync(searchInput);

                    // H�mta filmlista att visa i s�kresultat. Skapa tom lista vid null
                    Movies = result.Search ?? new List<MovieLite>();

                    // F�r att kunna visa i vyn. Undvik null genom att anv�nda "0" som fallback
                    TotalResults = result.TotalResults ?? "0";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid h�mtning av filmer fr�n OMDb: {ex.Message}");
                ViewData["ErrorMessage"] = "Ett fel uppstod vid h�mtning av filmer. F�rs�k igen senare.";
            }
        }
    }
}
