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
public class CaregiversController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public CaregiversController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CaregiverDto>>> GetAll([FromQuery] bool activeOnly = false)
    {
        var userId = GetUserId();
        var caregivers = activeOnly
            ? await _unitOfWork.Caregivers.GetActiveByUserIdAsync(userId)
            : await _unitOfWork.Caregivers.GetByUserIdAsync(userId);

        return Ok(caregivers.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CaregiverDto>> GetById(int id)
    {
        var userId = GetUserId();
        var caregiver = await _unitOfWork.Caregivers.GetByIdAsync(id);

        if (caregiver == null || caregiver.UserId != userId)
        {
            return NotFound();
        }

        return Ok(MapToDto(caregiver));
    }

    [HttpPost]
    public async Task<ActionResult<CaregiverDto>> Create([FromBody] CreateCaregiverDto model)
    {
        var userId = GetUserId();

        var caregiver = new Caregiver
        {
            Name = model.Name,
            Relationship = model.Relationship,
            Color = model.Color,
            Phone = model.Phone,
            DisplayOrder = model.DisplayOrder,
            IsActive = true,
            UserId = userId
        };

        await _unitOfWork.Caregivers.AddAsync(caregiver);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = caregiver.Id }, MapToDto(caregiver));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CaregiverDto>> Update(int id, [FromBody] UpdateCaregiverDto model)
    {
        var userId = GetUserId();
        var caregiver = await _unitOfWork.Caregivers.GetByIdAsync(id);

        if (caregiver == null || caregiver.UserId != userId)
        {
            return NotFound();
        }

        if (model.Name != null) caregiver.Name = model.Name;
        if (model.Relationship != null) caregiver.Relationship = model.Relationship;
        if (model.Color != null) caregiver.Color = model.Color;
        if (model.Phone != null) caregiver.Phone = model.Phone;
        if (model.DisplayOrder.HasValue) caregiver.DisplayOrder = model.DisplayOrder.Value;
        if (model.IsActive.HasValue) caregiver.IsActive = model.IsActive.Value;

        await _unitOfWork.Caregivers.UpdateAsync(caregiver);
        await _unitOfWork.SaveChangesAsync();

        return Ok(MapToDto(caregiver));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var caregiver = await _unitOfWork.Caregivers.GetByIdAsync(id);

        if (caregiver == null || caregiver.UserId != userId)
        {
            return NotFound();
        }

        await _unitOfWork.Caregivers.DeleteAsync(caregiver);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        throw new UnauthorizedAccessException();

    private static CaregiverDto MapToDto(Caregiver caregiver) => new()
    {
        Id = caregiver.Id,
        Name = caregiver.Name,
        Relationship = caregiver.Relationship,
        Color = caregiver.Color,
        Phone = caregiver.Phone,
        DisplayOrder = caregiver.DisplayOrder,
        IsActive = caregiver.IsActive
    };
}
