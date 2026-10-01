namespace CNSS_TRESO.Domain.Entities;

public sealed class Module
{
    public int Number { get; private set; }
    public string? Name { get; private set; }

    private Module() { }

    public Module(int number, string? name)
    {
        Number = number;
        Name = name;
    }
}
