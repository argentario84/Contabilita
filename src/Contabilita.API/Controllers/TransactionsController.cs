using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Contabilita.API.DTOs;
using Contabilita.Core.Entities;
using Contabilita.Core.Enums;
using Contabilita.Core.Interfaces;

namespace Contabilita.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public TransactionsController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAll(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] TransactionType? type = null)
    {
        var userId = GetUserId();
        IEnumerable<Transaction> transactions;

        if (startDate.HasValue && endDate.HasValue)
        {
            transactions = await _unitOfWork.Transactions.GetByUserIdAndDateRangeAsync(
                userId, startDate.Value, endDate.Value);
        }
        else
        {
            transactions = await _unitOfWork.Transactions.GetByUserIdAsync(userId);
        }

        if (categoryId.HasValue)
            transactions = transactions.Where(t => t.CategoryId == categoryId.Value);

        if (type.HasValue)
            transactions = transactions.Where(t => t.Type == type.Value);

        return Ok(transactions.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionDto>> GetById(int id)
    {
        var userId = GetUserId();
        var transaction = await _unitOfWork.Transactions.GetByIdWithDetailsAsync(id);

        if (transaction == null || transaction.UserId != userId)
        {
            return NotFound();
        }

        return Ok(MapToDto(transaction));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<TransactionSummaryDto>> GetSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null) return NotFound();

        var totalIncome = await _unitOfWork.Transactions.GetTotalByUserIdAndTypeAsync(
            userId, TransactionType.Income, startDate, endDate);
        var totalExpenses = await _unitOfWork.Transactions.GetTotalByUserIdAndTypeAsync(
            userId, TransactionType.Expense, startDate, endDate);

        var expenseTransactions = startDate.HasValue && endDate.HasValue
            ? await _unitOfWork.Transactions.GetByUserIdAndDateRangeAsync(userId, startDate.Value, endDate.Value)
            : await _unitOfWork.Transactions.GetByUserIdAsync(userId);

        var expensesByCategory = expenseTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Color })
            .Select(g => new CategorySummaryDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                CategoryColor = g.Key.Color,
                Total = g.Sum(t => t.Amount),
                Percentage = totalExpenses > 0 ? Math.Round(g.Sum(t => t.Amount) / totalExpenses * 100, 2) : 0
            })
            .OrderByDescending(c => c.Total)
            .ToList();

        var currentBudget = user.InitialBudget + totalIncome - totalExpenses;

        return Ok(new TransactionSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            Balance = totalIncome - totalExpenses,
            CurrentBudget = currentBudget,
            ExpensesByCategory = expensesByCategory
        });
    }

    [HttpGet("budget-planning")]
    public async Task<ActionResult<BudgetPlanningDto>> GetBudgetPlanning()
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null) return NotFound();

        // Get current month date range
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        // Calculate scheduled expenses total (monthly equivalent)
        var scheduledExpenses = await _unitOfWork.ScheduledExpenses.GetByUserIdAsync(userId);
        var scheduledExpensesTotal = scheduledExpenses
            .Where(se => se.IsActive)
            .Sum(se => se.Recurrence switch
            {
                RecurrenceType.Daily => se.Amount * 30,
                RecurrenceType.Weekly => se.Amount * 4,
                RecurrenceType.Monthly => se.Amount,
                RecurrenceType.Yearly => se.Amount / 12,
                _ => se.Amount
            });

        var extraFixedExpenses = user.ExtraFixedExpenses ?? 0;
        var totalFixedExpenses = scheduledExpensesTotal + extraFixedExpenses;

        // Calculate savings goal
        decimal savingsGoal;
        if (user.UseSavingsPercentage && user.SavingsGoalPercentage.HasValue)
        {
            savingsGoal = user.MonthlyIncome * (user.SavingsGoalPercentage.Value / 100);
        }
        else
        {
            savingsGoal = user.SavingsGoalAmount ?? 0;
        }

        // Available budget for variable expenses
        var availableBudget = user.MonthlyIncome - totalFixedExpenses - savingsGoal;

        // Get expenses this month (excluding scheduled expenses to avoid double counting)
        var monthExpenses = await _unitOfWork.Transactions.GetByUserIdAndDateRangeAsync(
            userId, startOfMonth, endOfMonth);

        var spentThisMonth = monthExpenses
            .Where(t => t.Type == TransactionType.Expense && t.ScheduledExpenseId == null)
            .Sum(t => t.Amount);

        var remainingBudget = availableBudget - spentThisMonth;
        var budgetPercentageUsed = availableBudget > 0
            ? Math.Round(spentThisMonth / availableBudget * 100, 2)
            : 0;

        var alertThreshold = user.BudgetAlertThreshold;
        var isOverThreshold = budgetPercentageUsed >= alertThreshold;
        var isOverBudget = spentThisMonth > availableBudget;

        return Ok(new BudgetPlanningDto
        {
            MonthlyIncome = user.MonthlyIncome,
            ScheduledExpensesTotal = scheduledExpensesTotal,
            ExtraFixedExpenses = extraFixedExpenses,
            TotalFixedExpenses = totalFixedExpenses,
            SavingsGoal = savingsGoal,
            AvailableBudget = availableBudget,
            SpentThisMonth = spentThisMonth,
            RemainingBudget = remainingBudget,
            BudgetPercentageUsed = budgetPercentageUsed,
            AlertThreshold = alertThreshold,
            IsOverThreshold = isOverThreshold,
            IsOverBudget = isOverBudget
        });
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<AdvancedStatisticsDto>> GetStatistics([FromQuery] int months = 12)
    {
        var userId = GetUserId();

        var endDate = DateTime.UtcNow;
        var startDate = new DateTime(endDate.AddMonths(-months + 1).Year, endDate.AddMonths(-months + 1).Month, 1);

        var transactions = await _unitOfWork.Transactions.GetByUserIdAndDateRangeAsync(userId, startDate, endDate);
        var txList = transactions.ToList();

        var italianMonths = new[] { "", "Gen", "Feb", "Mar", "Apr", "Mag", "Giu", "Lug", "Ago", "Set", "Ott", "Nov", "Dic" };

        // Monthly trend
        var monthlyTrend = new List<MonthlyTrendDto>();
        var monthLabels = new List<string>();

        for (int i = months - 1; i >= 0; i--)
        {
            var date = endDate.AddMonths(-i);
            var monthStart = new DateTime(date.Year, date.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthTx = txList.Where(t => t.Date >= monthStart && t.Date <= monthEnd).ToList();
            var income = monthTx.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var expenses = monthTx.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            var label = $"{italianMonths[date.Month]} {date.Year}";
            monthLabels.Add(label);
            monthlyTrend.Add(new MonthlyTrendDto
            {
                Year = date.Year,
                Month = date.Month,
                MonthLabel = label,
                Income = income,
                Expenses = expenses,
                Balance = income - expenses
            });
        }

        // Top categories by total expense
        var totalExpenses = txList.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var topCategories = txList
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Color })
            .Select(g => new CategorySummaryDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                CategoryColor = g.Key.Color,
                Total = g.Sum(t => t.Amount),
                Percentage = totalExpenses > 0 ? Math.Round(g.Sum(t => t.Amount) / totalExpenses * 100, 2) : 0
            })
            .OrderByDescending(c => c.Total)
            .Take(10)
            .ToList();

        // Category monthly trend (top 5)
        var categoryMonthlyTrend = topCategories.Take(5).Select(cat =>
        {
            var amounts = monthlyTrend.Select(m =>
            {
                var mStart = new DateTime(m.Year, m.Month, 1);
                var mEnd = mStart.AddMonths(1).AddDays(-1);
                return txList
                    .Where(t => t.CategoryId == cat.CategoryId && t.Type == TransactionType.Expense && t.Date >= mStart && t.Date <= mEnd)
                    .Sum(t => t.Amount);
            }).ToList();

            return new CategoryMonthlyTrendDto
            {
                CategoryId = cat.CategoryId,
                CategoryName = cat.CategoryName,
                CategoryColor = cat.CategoryColor,
                MonthlyAmounts = amounts
            };
        }).ToList();

        // Day of week stats
        var dayNames = new[] { "Dom", "Lun", "Mar", "Mer", "Gio", "Ven", "Sab" };
        var dayOfWeekStats = Enumerable.Range(0, 7).Select(d =>
        {
            var dayTx = txList.Where(t => t.Type == TransactionType.Expense && (int)t.Date.DayOfWeek == d).ToList();
            return new DayOfWeekStatsDto
            {
                DayName = dayNames[d],
                TotalExpense = dayTx.Sum(t => t.Amount),
                TransactionCount = dayTx.Count,
                AverageExpense = dayTx.Count > 0 ? Math.Round(dayTx.Sum(t => t.Amount) / dayTx.Count, 2) : 0
            };
        }).ToList();

        // Averages and YTD
        var avgMonthlyExpenses = monthlyTrend.Where(m => m.Expenses > 0).Select(m => m.Expenses).DefaultIfEmpty(0).Average();
        var avgMonthlyIncome = monthlyTrend.Where(m => m.Income > 0).Select(m => m.Income).DefaultIfEmpty(0).Average();
        var totalExpensesYTD = txList.Where(t => t.Type == TransactionType.Expense && t.Date.Year == endDate.Year).Sum(t => t.Amount);
        var totalIncomeYTD = txList.Where(t => t.Type == TransactionType.Income && t.Date.Year == endDate.Year).Sum(t => t.Amount);

        var bestMonth = monthlyTrend.OrderByDescending(m => m.Balance).FirstOrDefault()?.MonthLabel;
        var worstMonth = monthlyTrend.OrderBy(m => m.Balance).FirstOrDefault()?.MonthLabel;

        return Ok(new AdvancedStatisticsDto
        {
            MonthlyTrend = monthlyTrend,
            TopCategories = topCategories,
            CategoryMonthlyTrend = categoryMonthlyTrend,
            DayOfWeekStats = dayOfWeekStats,
            AverageMonthlyExpenses = Math.Round(avgMonthlyExpenses, 2),
            AverageMonthlyIncome = Math.Round(avgMonthlyIncome, 2),
            TotalExpensesYTD = totalExpensesYTD,
            TotalIncomeYTD = totalIncomeYTD,
            BestMonth = bestMonth,
            WorstMonth = worstMonth,
            MonthLabels = monthLabels
        });
    }

    [HttpPost]
    public async Task<ActionResult<TransactionDto>> Create([FromBody] CreateTransactionDto model)
    {
        var userId = GetUserId();

        var category = await _unitOfWork.Categories.GetByIdAsync(model.CategoryId);
        if (category == null || category.UserId != userId)
        {
            return BadRequest(new { Message = "Categoria non valida" });
        }

        var transaction = new Transaction
        {
            Amount = model.Amount,
            Description = model.Description,
            Date = model.Date,
            Type = model.Type,
            Notes = model.Notes,
            CategoryId = model.CategoryId,
            UserId = userId,
            ScheduledExpenseId = model.ScheduledExpenseId
        };

        await _unitOfWork.Transactions.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        transaction = await _unitOfWork.Transactions.GetByIdWithDetailsAsync(transaction.Id);
        return CreatedAtAction(nameof(GetById), new { id = transaction!.Id }, MapToDto(transaction));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TransactionDto>> Update(int id, [FromBody] UpdateTransactionDto model)
    {
        var userId = GetUserId();
        var transaction = await _unitOfWork.Transactions.GetByIdWithDetailsAsync(id);

        if (transaction == null || transaction.UserId != userId)
        {
            return NotFound();
        }

        if (model.CategoryId.HasValue)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(model.CategoryId.Value);
            if (category == null || category.UserId != userId)
            {
                return BadRequest(new { Message = "Categoria non valida" });
            }
            transaction.CategoryId = model.CategoryId.Value;
        }

        if (model.Amount.HasValue) transaction.Amount = model.Amount.Value;
        if (model.Description != null) transaction.Description = model.Description;
        if (model.Date.HasValue) transaction.Date = model.Date.Value;
        if (model.Type.HasValue) transaction.Type = model.Type.Value;
        if (model.Notes != null) transaction.Notes = model.Notes;

        await _unitOfWork.Transactions.UpdateAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        transaction = await _unitOfWork.Transactions.GetByIdWithDetailsAsync(transaction.Id);
        return Ok(MapToDto(transaction!));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var transaction = await _unitOfWork.Transactions.GetByIdAsync(id);

        if (transaction == null || transaction.UserId != userId)
        {
            return NotFound();
        }

        await _unitOfWork.Transactions.DeleteAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        throw new UnauthorizedAccessException();

    private static TransactionDto MapToDto(Transaction transaction) => new()
    {
        Id = transaction.Id,
        Amount = transaction.Amount,
        Description = transaction.Description,
        Date = transaction.Date,
        Type = transaction.Type,
        Notes = transaction.Notes,
        CategoryId = transaction.CategoryId,
        CategoryName = transaction.Category?.Name ?? string.Empty,
        CategoryColor = transaction.Category?.Color,
        ScheduledExpenseId = transaction.ScheduledExpenseId
    };
}
