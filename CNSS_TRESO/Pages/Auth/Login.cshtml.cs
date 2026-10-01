using CNSS_TRESO.Application.DTOs;
using CNSS_TRESO.Application.Exercises;
using CNSS_TRESO.Common;
using CNSS_TRESO.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using IAuthenticationService = CNSS_TRESO.Application.Contracts.IAuthenticationService;

namespace CNSS_TRESO.Pages.Auth;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IExerciseCatalog _exerciseCatalog;

    public LoginModel(IAuthenticationService authenticationService, IExerciseCatalog exerciseCatalog)
    {
        _authenticationService = authenticationService;
        _exerciseCatalog = exerciseCatalog;
    }

    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public IReadOnlyList<ExerciseInfo> Exercises { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadExercisesAsync(cancellationToken);
    }

    public async Task<IActionResult> OnGetLogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Auth/Login");
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadExercisesAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(Input.Username) || string.IsNullOrWhiteSpace(Input.Password))
        {
            ErrorMessage = "Veuillez saisir votre identifiant et votre mot de passe.";
            return Page();
        }

        var chosen = Exercises.FirstOrDefault(e => e.DatabaseName == Input.Exercise);
        if (chosen is null)
        {
            ErrorMessage = "Veuillez sélectionner un exercice valide.";
            return Page();
        }

        var result = await _authenticationService.AuthenticateAsync(
            new AuthenticationRequest(Input.Username, Input.Password),
            cancellationToken);

        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error ?? "Échec de connexion.";
            return Page();
        }

        var role = result.AccessLevel == AccessLevel.Administrator ? "Administrator" : "Agent";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.UserId!.Value.ToString()),
            new(ClaimTypes.Name, result.Username!),
            new(ClaimTypes.GivenName, result.FullName!),
            new(ClaimTypes.Role, role),
            new(ClaimTypesExt.CostCenter, result.CostCenter ?? string.Empty),
            new(ClaimTypesExt.Matricule, result.Username!),
            new(ClaimTypesExt.Exercise, chosen.Year.ToString()),
            new(ClaimTypesExt.DatabaseSuffix, chosen.DatabaseName),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(identity));

        return RedirectToPage("/Index");
    }

    private async Task LoadExercisesAsync(CancellationToken cancellationToken)
    {
        Exercises = await _exerciseCatalog.ListAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(Input.Exercise))
        {
            Input.Exercise = Exercises.FirstOrDefault()?.DatabaseName ?? string.Empty;
        }
    }

    public sealed class LoginInputModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Exercise { get; set; } = string.Empty;
    }
}
