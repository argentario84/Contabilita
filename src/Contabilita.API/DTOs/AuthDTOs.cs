using System.ComponentModel.DataAnnotations;

namespace Contabilita.API.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public decimal InitialBudget { get; set; }
    public decimal MonthlyIncome { get; set; }
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public decimal InitialBudget { get; set; }
    public decimal MonthlyIncome { get; set; }

    // Budget Planning
    public decimal? SavingsGoalAmount { get; set; }
    public decimal? SavingsGoalPercentage { get; set; }
    public bool UseSavingsPercentage { get; set; }
    public decimal? ExtraFixedExpenses { get; set; }
    public decimal BudgetAlertThreshold { get; set; }
}

public class UpdateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public decimal? InitialBudget { get; set; }
    public decimal? MonthlyIncome { get; set; }

    // Budget Planning
    public decimal? SavingsGoalAmount { get; set; }
    public decimal? SavingsGoalPercentage { get; set; }
    public bool? UseSavingsPercentage { get; set; }
    public decimal? ExtraFixedExpenses { get; set; }
    public decimal? BudgetAlertThreshold { get; set; }
}
