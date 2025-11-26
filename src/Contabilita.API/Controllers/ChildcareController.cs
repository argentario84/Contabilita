using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Contabilita.API.DTOs;
using Contabilita.Core.Entities;
using Contabilita.Core.Interfaces;

namespace Contabilita.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChildcareController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ChildcareController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("week")]
    public async Task<ActionResult<WeeklyChildcareDto>> GetWeek([FromQuery] DateTime? weekStartDate = null)
    {
        var userId = GetUserId();
        var startDate = weekStartDate ?? GetCurrentWeekStart();

        var slots = await _unitOfWork.ChildcareSlots.GetByUserIdAndWeekAsync(userId, startDate);

        return Ok(new WeeklyChildcareDto
        {
            WeekStartDate = startDate,
            Slots = slots.Select(MapToDto).ToList()
        });
    }

    [HttpPost("slot")]
    public async Task<ActionResult<ChildcareSlotDto>> CreateSlot([FromBody] CreateChildcareSlotDto model)
    {
        var userId = GetUserId();

        // Verifica che il caregiver appartenga all'utente
        var caregiver = await _unitOfWork.Caregivers.GetByIdAsync(model.CaregiverId);
        if (caregiver == null || caregiver.UserId != userId)
        {
            return BadRequest("Caregiver non valido");
        }

        // Controlla se esiste già uno slot per questa posizione
        var existingSlot = await _unitOfWork.ChildcareSlots.GetBySlotAsync(
            userId, model.WeekStartDate, model.DayOfWeek, model.TimeSlot);

        if (existingSlot != null)
        {
            // Aggiorna lo slot esistente
            existingSlot.CaregiverId = model.CaregiverId;
            await _unitOfWork.ChildcareSlots.UpdateAsync(existingSlot);
            await _unitOfWork.SaveChangesAsync();

            // Ricarica con il caregiver
            existingSlot = await _unitOfWork.ChildcareSlots.GetBySlotAsync(
                userId, model.WeekStartDate, model.DayOfWeek, model.TimeSlot);

            return Ok(MapToDto(existingSlot!));
        }

        var slot = new ChildcareSlot
        {
            DayOfWeek = model.DayOfWeek,
            TimeSlot = model.TimeSlot,
            WeekStartDate = model.WeekStartDate.Date,
            CaregiverId = model.CaregiverId,
            UserId = userId
        };

        await _unitOfWork.ChildcareSlots.AddAsync(slot);
        await _unitOfWork.SaveChangesAsync();

        // Ricarica con il caregiver
        slot = (await _unitOfWork.ChildcareSlots.GetBySlotAsync(
            userId, model.WeekStartDate, model.DayOfWeek, model.TimeSlot))!;

        return CreatedAtAction(nameof(GetWeek), new { weekStartDate = model.WeekStartDate }, MapToDto(slot));
    }

    [HttpDelete("slot/{id}")]
    public async Task<ActionResult> DeleteSlot(int id)
    {
        var userId = GetUserId();
        var slot = await _unitOfWork.ChildcareSlots.GetByIdAsync(id);

        if (slot == null || slot.UserId != userId)
        {
            return NotFound();
        }

        await _unitOfWork.ChildcareSlots.DeleteAsync(slot);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("week")]
    public async Task<ActionResult<WeeklyChildcareDto>> UpdateWeek([FromBody] BulkUpdateChildcareDto model)
    {
        var userId = GetUserId();

        // Elimina tutti gli slot della settimana
        await _unitOfWork.ChildcareSlots.DeleteByWeekAsync(userId, model.WeekStartDate);

        // Crea i nuovi slot
        foreach (var slotDto in model.Slots)
        {
            var caregiver = await _unitOfWork.Caregivers.GetByIdAsync(slotDto.CaregiverId);
            if (caregiver == null || caregiver.UserId != userId)
            {
                continue;
            }

            var slot = new ChildcareSlot
            {
                DayOfWeek = slotDto.DayOfWeek,
                TimeSlot = slotDto.TimeSlot,
                WeekStartDate = model.WeekStartDate.Date,
                CaregiverId = slotDto.CaregiverId,
                UserId = userId
            };

            await _unitOfWork.ChildcareSlots.AddAsync(slot);
        }

        await _unitOfWork.SaveChangesAsync();

        // Restituisci la settimana aggiornata
        var slots = await _unitOfWork.ChildcareSlots.GetByUserIdAndWeekAsync(userId, model.WeekStartDate);

        return Ok(new WeeklyChildcareDto
        {
            WeekStartDate = model.WeekStartDate,
            Slots = slots.Select(MapToDto).ToList()
        });
    }

    [HttpPost("week/copy")]
    public async Task<ActionResult<WeeklyChildcareDto>> CopyWeek(
        [FromQuery] DateTime sourceWeek,
        [FromQuery] DateTime targetWeek)
    {
        var userId = GetUserId();

        // Prendi gli slot della settimana sorgente
        var sourceSlots = await _unitOfWork.ChildcareSlots.GetByUserIdAndWeekAsync(userId, sourceWeek);

        // Elimina gli slot della settimana target
        await _unitOfWork.ChildcareSlots.DeleteByWeekAsync(userId, targetWeek);

        // Copia gli slot
        foreach (var sourceSlot in sourceSlots)
        {
            var newSlot = new ChildcareSlot
            {
                DayOfWeek = sourceSlot.DayOfWeek,
                TimeSlot = sourceSlot.TimeSlot,
                WeekStartDate = targetWeek.Date,
                CaregiverId = sourceSlot.CaregiverId,
                UserId = userId
            };

            await _unitOfWork.ChildcareSlots.AddAsync(newSlot);
        }

        await _unitOfWork.SaveChangesAsync();

        // Restituisci la settimana copiata
        var slots = await _unitOfWork.ChildcareSlots.GetByUserIdAndWeekAsync(userId, targetWeek);

        return Ok(new WeeklyChildcareDto
        {
            WeekStartDate = targetWeek,
            Slots = slots.Select(MapToDto).ToList()
        });
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        throw new UnauthorizedAccessException();

    private static DateTime GetCurrentWeekStart()
    {
        var today = DateTime.Today;
        var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        return today.AddDays(-diff);
    }

    private static ChildcareSlotDto MapToDto(ChildcareSlot slot) => new()
    {
        Id = slot.Id,
        DayOfWeek = slot.DayOfWeek,
        TimeSlot = slot.TimeSlot,
        WeekStartDate = slot.WeekStartDate,
        CaregiverId = slot.CaregiverId,
        CaregiverName = slot.Caregiver?.Name ?? "",
        CaregiverColor = slot.Caregiver?.Color
    };
}
