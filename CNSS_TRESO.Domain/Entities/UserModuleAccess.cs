namespace CNSS_TRESO.Domain.Entities;

public sealed class UserModuleAccess
{
    public string Username { get; private set; } = string.Empty;
    public int ModuleNumber { get; private set; }

    private UserModuleAccess() { }

    public UserModuleAccess(string username, int moduleNumber)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username is required.", nameof(username));
        Username = username.Trim();
        ModuleNumber = moduleNumber;
    }
}
