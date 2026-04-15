using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Contabilita.API.DTOs;
using Contabilita.Core.Entities;
using Contabilita.Core.Interfaces;

namespace Contabilita.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DebtCreditsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public DebtCreditsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DebtCreditDto>>> GetAll()
    {
        var userId = GetUserId();
        var items = await _unitOfWork.DebtCredits.GetByUserIdAsync(userId);
        return Ok(items.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DebtCreditDto>> GetById(int id)
    {
        var userId = GetUserId();
        var item = await _unitOfWork.DebtCredits.GetByIdAsync(id);
        if (item == null || item.UserId != userId) return NotFound();
        return Ok(MapToDto(item));
    }

    [HttpPost]
    public async Task<ActionResult<DebtCreditDto>> Create([FromBody] CreateDebtCreditDto model)
    {
        var userId = GetUserId();
        var item = new DebtCredit
        {
            Type = model.Type,
            PersonName = model.PersonName,
            Amount = model.Amount,
            Description = model.Description,
            DueDate = model.DueDate,
            Notes = model.Notes,
            UserId = userId
        };

        await _unitOfWork.DebtCredits.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToDto(item));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DebtCreditDto>> Update(int id, [FromBody] UpdateDebtCreditDto model)
    {
        var userId = GetUserId();
        var item = await _unitOfWork.DebtCredits.GetByIdAsync(id);
        if (item == null || item.UserId != userId) return NotFound();

        if (model.Type.HasValue) item.Type = model.Type.Value;
        if (model.PersonName != null) item.PersonName = model.PersonName;
        if (model.Amount.HasValue) item.Amount = model.Amount.Value;
        if (model.Description != null) item.Description = model.Description;
        if (model.DueDate.HasValue) item.DueDate = model.DueDate;
        if (model.Notes != null) item.Notes = model.Notes;

        await _unitOfWork.DebtCredits.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return Ok(MapToDto(item));
    }

    [HttpPost("{id}/settle")]
    public async Task<ActionResult<DebtCreditDto>> Settle(int id)
    {
        var userId = GetUserId();
        var item = await _unitOfWork.DebtCredits.GetByIdAsync(id);
        if (item == null || item.UserId != userId) return NotFound();

        item.IsSettled = true;
        item.SettledAt = DateTime.UtcNow;

        await _unitOfWork.DebtCredits.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return Ok(MapToDto(item));
    }

    [HttpPost("{id}/reopen")]
    public async Task<ActionResult<DebtCreditDto>> Reopen(int id)
    {
        var userId = GetUserId();
        var item = await _unitOfWork.DebtCredits.GetByIdAsync(id);
        if (item == null || item.UserId != userId) return NotFound();

        item.IsSettled = false;
        item.SettledAt = null;

        await _unitOfWork.DebtCredits.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return Ok(MapToDto(item));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var item = await _unitOfWork.DebtCredits.GetByIdAsync(id);
        if (item == null || item.UserId != userId) return NotFound();

        await _unitOfWork.DebtCredits.DeleteAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        throw new UnauthorizedAccessException();

    private static DebtCreditDto MapToDto(DebtCredit item)
    {
        var now = DateTime.UtcNow.Date;
        var dueDate = item.DueDate?.Date;
        int? daysUntilDue = dueDate.HasValue ? (int)(dueDate.Value - now).TotalDays : null;
        bool isOverdue = !item.IsSettled && dueDate.HasValue && dueDate.Value < now;

        return new DebtCreditDto
        {
            Id = item.Id,
            Type = item.Type,
            PersonName = item.PersonName,
            Amount = item.Amount,
            Description = item.Description,
            DueDate = item.DueDate,
            IsSettled = item.IsSettled,
            SettledAt = item.SettledAt,
            Notes = item.Notes,
            CreatedAt = item.CreatedAt,
            IsOverdue = isOverdue,
            DaysUntilDue = daysUntilDue
        };
    }
}
