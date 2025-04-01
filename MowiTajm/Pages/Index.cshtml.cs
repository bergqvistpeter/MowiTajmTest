using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MowiTajm.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Redirecta anv�ndaren till huvudsidan f�r s�kning p� filmtitlar
            return RedirectToPage("/Movies/Index");
        }
    }
}
