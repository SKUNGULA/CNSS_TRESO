namespace CNSS_TRESO.Application.Contracts;

public interface IPasswordHasher
{
    string Hash(string plainPassword);
    bool Verify(string plainPassword, string hashedPassword);
}
