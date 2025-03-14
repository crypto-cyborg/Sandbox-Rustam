namespace Sandbox.Core.Entities;

public class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public Wallet Wallet { get; set; }
}