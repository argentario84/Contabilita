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
public class CalendarEventsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public CalendarEventsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CalendarEventDto>>> GetAll(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = GetUserId();
        IEnumerable<CalendarEvent> events;

        if (startDate.HasValue && endDate.HasValue)
        {
            events = await _unitOfWork.CalendarEvents.GetByUserIdOrSharedAndDateRangeAsync(
                userId, startDate.Value, endDate.Value);
        }
        else
        {
            events = await _unitOfWork.CalendarEvents.GetByUserIdOrSharedAsync(userId);
        }

        return Ok(events.Select(e => MapToDto(e, userId)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CalendarEventDto>> GetById(int id)
    {
        var userId = GetUserId();
        var calendarEvent = await _unitOfWork.CalendarEvents.GetByIdAsync(id);

        if (calendarEvent == null || (calendarEvent.UserId != userId && !calendarEvent.IsShared))
        {
            return NotFound();
        }

        return Ok(MapToDto(calendarEvent, userId));
    }

    [HttpPost]
    public async Task<ActionResult<CalendarEventDto>> Create([FromBody] CreateCalendarEventDto model)
    {
        var userId = GetUserId();

        var calendarEvent = new CalendarEvent
        {
            Title = model.Title,
            Description = model.Description,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            AllDay = model.AllDay,
            Color = model.Color,
            IsShared = model.IsShared,
            UserId = userId
        };

        await _unitOfWork.CalendarEvents.AddAsync(calendarEvent);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = calendarEvent.Id }, MapToDto(calendarEvent, userId));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CalendarEventDto>> Update(int id, [FromBody] UpdateCalendarEventDto model)
    {
        var userId = GetUserId();
        var calendarEvent = await _unitOfWork.CalendarEvents.GetByIdAsync(id);

        if (calendarEvent == null || calendarEvent.UserId != userId)
        {
            return NotFound();
        }

        if (model.Title != null) calendarEvent.Title = model.Title;
        if (model.Description != null) calendarEvent.Description = model.Description;
        if (model.StartDate.HasValue) calendarEvent.StartDate = model.StartDate.Value;
        if (model.EndDate.HasValue) calendarEvent.EndDate = model.EndDate;
        if (model.AllDay.HasValue) calendarEvent.AllDay = model.AllDay.Value;
        if (model.Color != null) calendarEvent.Color = model.Color;
        if (model.IsShared.HasValue) calendarEvent.IsShared = model.IsShared.Value;

        await _unitOfWork.CalendarEvents.UpdateAsync(calendarEvent);
        await _unitOfWork.SaveChangesAsync();

        return Ok(MapToDto(calendarEvent, userId));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var calendarEvent = await _unitOfWork.CalendarEvents.GetByIdAsync(id);

        if (calendarEvent == null || calendarEvent.UserId != userId)
        {
            return NotFound();
        }

        await _unitOfWork.CalendarEvents.DeleteAsync(calendarEvent);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        throw new UnauthorizedAccessException();

    private static CalendarEventDto MapToDto(CalendarEvent calendarEvent, string currentUserId) => new()
    {
        Id = calendarEvent.Id,
        Title = calendarEvent.Title,
        Description = calendarEvent.Description,
        StartDate = calendarEvent.StartDate,
        EndDate = calendarEvent.EndDate,
        AllDay = calendarEvent.AllDay,
        Color = calendarEvent.Color,
        IsShared = calendarEvent.IsShared,
        CreatedByName = calendarEvent.IsShared && calendarEvent.UserId != currentUserId
            ? $"{calendarEvent.User?.FirstName} {calendarEvent.User?.LastName}".Trim()
            : null
    };
}
