using Microsoft.AspNetCore.Authorization;
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
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoriesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll([FromQuery] TransactionType? type = null)
    {
        var userId = GetUserId();
        IEnumerable<Category> categories;

        if (type.HasValue)
        {
            categories = await _unitOfWork.Categories.GetByUserIdAndTypeAsync(userId, type.Value);
        }
        else
        {
            categories = await _unitOfWork.Categories.GetByUserIdAsync(userId);
        }

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var result = new List<CategoryDto>();
        foreach (var category in categories)
        {
            var spent = await _unitOfWork.Transactions.GetTotalByUserIdAndTypeAsync(
                userId, TransactionType.Expense, startOfMonth, endOfMonth);

            var categoryTransactions = await _unitOfWork.Transactions.GetByUserIdAndCategoryAsync(userId, category.Id);
            var spentThisMonth = categoryTransactions
                .Where(t => t.Date >= startOfMonth && t.Date <= endOfMonth && t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            result.Add(MapToDto(category, spentThisMonth));
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var userId = GetUserId();
        var category = await _unitOfWork.Categories.GetByIdAsync(id);

        if (category == null || category.UserId != userId)
        {
            return NotFound();
        }

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var categoryTransactions = await _unitOfWork.Transactions.GetByUserIdAndCategoryAsync(userId, category.Id);
        var spentThisMonth = categoryTransactions
            .Where(t => t.Date >= startOfMonth && t.Date <= endOfMonth && t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        return Ok(MapToDto(category, spentThisMonth));
    }

    [HttpGet("budget-summary")]
    public async Task<ActionResult<IEnumerable<CategoryBudgetSummaryDto>>> GetBudgetSummary()
    {
        var userId = GetUserId();
        var categories = await _unitOfWork.Categories.GetByUserIdAndTypeAsync(userId, TransactionType.Expense);

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var result = new List<CategoryBudgetSummaryDto>();
        foreach (var category in categories.Where(c => c.MonthlyBudget.HasValue))
        {
            var categoryTransactions = await _unitOfWork.Transactions.GetByUserIdAndCategoryAsync(userId, category.Id);
            var spentThisMonth = categoryTransactions
                .Where(t => t.Date >= startOfMonth && t.Date <= endOfMonth && t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            var remaining = category.MonthlyBudget!.Value - spentThisMonth;
            var percentageUsed = category.MonthlyBudget.Value > 0
                ? Math.Round(spentThisMonth / category.MonthlyBudget.Value * 100, 2)
                : 0;

            result.Add(new CategoryBudgetSummaryDto
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                CategoryColor = category.Color,
                MonthlyBudget = category.MonthlyBudget,
                SpentThisMonth = spentThisMonth,
                RemainingBudget = remaining,
                BudgetPercentageUsed = percentageUsed,
                IsOverBudget = remaining < 0
            });
        }

        return Ok(result.OrderByDescending(c => c.BudgetPercentageUsed));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto model)
    {
        var userId = GetUserId();

        var category = new Category
        {
            Name = model.Name,
            Description = model.Description,
            Color = model.Color,
            Icon = model.Icon,
            Type = model.Type,
            MonthlyBudget = model.MonthlyBudget,
            UserId = userId
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, MapToDto(category, 0));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] UpdateCategoryDto model)
    {
        var userId = GetUserId();
        var category = await _unitOfWork.Categories.GetByIdAsync(id);

        if (category == null || category.UserId != userId)
        {
            return NotFound();
        }

        if (model.Name != null) category.Name = model.Name;
        if (model.Description != null) category.Description = model.Description;
        if (model.Color != null) category.Color = model.Color;
        if (model.Icon != null) category.Icon = model.Icon;
        if (model.Type.HasValue) category.Type = model.Type.Value;
        if (model.MonthlyBudget.HasValue) category.MonthlyBudget = model.MonthlyBudget;

        await _unitOfWork.Categories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return Ok(MapToDto(category, 0));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var category = await _unitOfWork.Categories.GetByIdAsync(id);

        if (category == null || category.UserId != userId)
        {
            return NotFound();
        }

        await _unitOfWork.Categories.DeleteAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        throw new UnauthorizedAccessException();

    private static CategoryDto MapToDto(Category category, decimal spentThisMonth)
    {
        decimal? remaining = category.MonthlyBudget.HasValue
            ? category.MonthlyBudget.Value - spentThisMonth
            : null;

        decimal? percentageUsed = category.MonthlyBudget.HasValue && category.MonthlyBudget.Value > 0
            ? Math.Round(spentThisMonth / category.MonthlyBudget.Value * 100, 2)
            : null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Color = category.Color,
            Icon = category.Icon,
            Type = category.Type,
            MonthlyBudget = category.MonthlyBudget,
            SpentThisMonth = spentThisMonth,
            RemainingBudget = remaining,
            BudgetPercentageUsed = percentageUsed
        };
    }
}
