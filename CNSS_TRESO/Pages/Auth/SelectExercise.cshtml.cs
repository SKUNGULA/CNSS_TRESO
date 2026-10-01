using CNSS_TRESO.Application.Exercises;
using CNSS_TRESO.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace CNSS_TRESO.Pages.Auth;

[Authorize]
public class SelectExerciseModel : PageModel
{
    private readonly IExerciseCatalog _catalog;

    public SelectExerciseModel(IExerciseCatalog catalog) => _catalog = catalog;

    public IReadOnlyList<ExerciseInfo> Exercises { get; private set; } = [];

    [BindProperty]
    public string SelectedDatabase { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Exercises = await _catalog.ListAsync(cancellationToken);
        SelectedDatabase = Exercises.FirstOrDefault()?.DatabaseName ?? string.Empty;
    }

    public async Task<IActionResult> OnGetLogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Auth/Login");
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Exercises = await _catalog.ListAsync(cancellationToken);
        var chosen = Exercises.FirstOrDefault(e => e.DatabaseName == SelectedDatabase);
        if (chosen is null)
        {
            ErrorMessage = "Veuillez selectionner un exercice valide.";
            return Page();
        }

        // Reconstruit le principal en ajoutant Exercise + DatabaseSuffix
        var identity = (ClaimsIdentity)User.Identity!;
        var newClaims = identity.Claims
            .Where(c => c.Type != ClaimTypesExt.Exercise && c.Type != ClaimTypesExt.DatabaseSuffix)
            .ToList();
        newClaims.Add(new Claim(ClaimTypesExt.Exercise, chosen.Year.ToString()));
        newClaims.Add(new Claim(ClaimTypesExt.DatabaseSuffix, chosen.DatabaseName));

        var newIdentity = new ClaimsIdentity(newClaims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(newIdentity));

        return RedirectToPage("/Index");
    }
}
