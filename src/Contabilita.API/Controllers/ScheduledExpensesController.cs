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
public class ScheduledExpensesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ScheduledExpensesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduledExpenseDto>>> GetAll([FromQuery] bool? activeOnly = null)
    {
        var userId = GetUserId();
        IEnumerable<ScheduledExpense> expenses;

        if (activeOnly == true)
        {
            expenses = await _unitOfWork.ScheduledExpenses.GetActiveByUserIdAsync(userId);
        }
        else
        {
            expenses = await _unitOfWork.ScheduledExpenses.GetByUserIdAsync(userId);
        }

        return Ok(expenses.Select(e => MapToDto(e)));
    }

    [HttpGet("due")]
    public async Task<ActionResult<IEnumerable<ScheduledExpenseDto>>> GetDueExpenses()
    {
        var userId = GetUserId();
        var expenses = await _unitOfWork.ScheduledExpenses.GetDueExpensesAsync(userId, DateTime.Today);

        return Ok(expenses.Select(e => MapToDto(e)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ScheduledExpenseDto>> GetById(int id)
    {
        var userId = GetUserId();
        var expense = await _unitOfWork.ScheduledExpenses.GetByIdWithDetailsAsync(id);

        if (expense == null || expense.UserId != userId)
        {
            return NotFound();
        }

        return Ok(MapToDto(expense));
    }

    [HttpPost]
    public async Task<ActionResult<ScheduledExpenseDto>> Create([FromBody] CreateScheduledExpenseDto model)
    {
        var userId = GetUserId();

        var category = await _unitOfWork.Categories.GetByIdAsync(model.CategoryId);
        if (category == null || category.UserId != userId)
        {
            return BadRequest(new { Message = "Categoria non valida" });
        }

        var expense = new ScheduledExpense
        {
            Name = model.Name,
            Amount = model.Amount,
            Description = model.Description,
            Recurrence = model.Recurrence,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            NextDueDate = model.StartDate,
            IsActive = true,
            CategoryId = model.CategoryId,
            UserId = userId
        };

        await _unitOfWork.ScheduledExpenses.AddAsync(expense);
        await _unitOfWork.SaveChangesAsync();

        expense = await _unitOfWork.ScheduledExpenses.GetByIdWithDetailsAsync(expense.Id);
        return CreatedAtAction(nameof(GetById), new { id = expense!.Id }, MapToDto(expense));
    }

    [HttpPost("{id}/confirm")]
    public async Task<ActionResult<TransactionDto>> ConfirmExpense(int id, [FromBody] ConfirmScheduledExpenseDto model)
    {
        var userId = GetUserId();
        var expense = await _unitOfWork.ScheduledExpenses.GetByIdWithDetailsAsync(id);

        if (expense == null || expense.UserId != userId)
        {
            return NotFound();
        }

        if (!expense.IsActive)
        {
            return BadRequest(new { Message = "Questa spesa programmata non è più attiva" });
        }

        var transaction = new Transaction
        {
            Amount = model.ActualAmount ?? expense.Amount,
            Description = expense.Name,
            Date = DateTime.Today,
            Type = TransactionType.Expense,
            Notes = model.Notes ?? $"Spesa programmata confermata: {expense.Name}",
            CategoryId = expense.CategoryId,
            UserId = userId,
            ScheduledExpenseId = expense.Id
        };

        await _unitOfWork.Transactions.AddAsync(transaction);

        expense.NextDueDate = CalculateNextDueDate(expense.NextDueDate, expense.Recurrence);

        if (expense.EndDate.HasValue && expense.NextDueDate > expense.EndDate.Value)
        {
            expense.IsActive = false;
        }

        await _unitOfWork.ScheduledExpenses.UpdateAsync(expense);
        await _unitOfWork.SaveChangesAsync();

        var createdTransaction = await _unitOfWork.Transactions.GetByIdWithDetailsAsync(transaction.Id);

        return Ok(new TransactionDto
        {
            Id = createdTransaction!.Id,
            Amount = createdTransaction.Amount,
            Description = createdTransaction.Description,
            Date = createdTransaction.Date,
            Type = createdTransaction.Type,
            Notes = createdTransaction.Notes,
            CategoryId = createdTransaction.CategoryId,
            CategoryName = createdTransaction.Category?.Name ?? string.Empty,
            CategoryColor = createdTransaction.Category?.Color,
            ScheduledExpenseId = createdTransaction.ScheduledExpenseId
        });
    }

    [HttpPost("{id}/skip")]
    public async Task<ActionResult<ScheduledExpenseDto>> SkipExpense(int id)
    {
        var userId = GetUserId();
        var expense = await _unitOfWork.ScheduledExpenses.GetByIdWithDetailsAsync(id);

        if (expense == null || expense.UserId != userId)
        {
            return NotFound();
        }

        expense.NextDueDate = CalculateNextDueDate(expense.NextDueDate, expense.Recurrence);

        if (expense.EndDate.HasValue && expense.NextDueDate > expense.EndDate.Value)
        {
            expense.IsActive = false;
        }

        await _unitOfWork.ScheduledExpenses.UpdateAsync(expense);
        await _unitOfWork.SaveChangesAsync();

        return Ok(MapToDto(expense));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ScheduledExpenseDto>> Update(int id, [FromBody] UpdateScheduledExpenseDto model)
    {
        var userId = GetUserId();
        var expense = await _unitOfWork.ScheduledExpenses.GetByIdWithDetailsAsync(id);

        if (expense == null || expense.UserId != userId)
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
            expense.CategoryId = model.CategoryId.Value;
        }

        if (model.Name != null) expense.Name = model.Name;
        if (model.Amount.HasValue) expense.Amount = model.Amount.Value;
        if (model.Description != null) expense.Description = model.Description;
        if (model.Recurrence.HasValue) expense.Recurrence = model.Recurrence.Value;
        if (model.EndDate.HasValue) expense.EndDate = model.EndDate;
        if (model.IsActive.HasValue) expense.IsActive = model.IsActive.Value;

        await _unitOfWork.ScheduledExpenses.UpdateAsync(expense);
        await _unitOfWork.SaveChangesAsync();

        expense = await _unitOfWork.ScheduledExpenses.GetByIdWithDetailsAsync(expense.Id);
        return Ok(MapToDto(expense!));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var expense = await _unitOfWork.ScheduledExpenses.GetByIdAsync(id);

        if (expense == null || expense.UserId != userId)
        {
            return NotFound();
        }

        await _unitOfWork.ScheduledExpenses.DeleteAsync(expense);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private static DateTime CalculateNextDueDate(DateTime currentDate, RecurrenceType recurrence) => recurrence switch
    {
        RecurrenceType.Daily => currentDate.AddDays(1),
        RecurrenceType.Weekly => currentDate.AddDays(7),
        RecurrenceType.Monthly => currentDate.AddMonths(1),
        RecurrenceType.Yearly => currentDate.AddYears(1),
        _ => currentDate.AddMonths(1)
    };

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        throw new UnauthorizedAccessException();

    private static ScheduledExpenseDto MapToDto(ScheduledExpense expense) => new()
    {
        Id = expense.Id,
        Name = expense.Name,
        Amount = expense.Amount,
        Description = expense.Description,
        Recurrence = expense.Recurrence,
        StartDate = expense.StartDate,
        EndDate = expense.EndDate,
        NextDueDate = expense.NextDueDate,
        IsActive = expense.IsActive,
        CategoryId = expense.CategoryId,
        CategoryName = expense.Category?.Name ?? string.Empty,
        CategoryColor = expense.Category?.Color,
        IsDueToday = expense.NextDueDate.Date <= DateTime.Today && expense.IsActive
    };
}
