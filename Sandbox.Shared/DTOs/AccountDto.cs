namespace Sandbox.Shared.DTOs;

public class AccountDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string WalletId { get; set; } = string.Empty;
}

public class CreateAccountDto
{
    public string Email { get; set; } = string.Empty;
}