using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CNSS_TRESO.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            return RedirectToPage("/Auth/Login");
        }
        return Page();
    }
}
