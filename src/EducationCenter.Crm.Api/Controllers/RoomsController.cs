using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Api.Contracts;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Domain.Identity;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Crm.Api.Controllers;

[ApiController]
[Authorize(Policy = AppRoles.Staff)]
[Route("api/rooms")]
public sealed class RoomsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public RoomsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<RoomResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var rooms = await _dbContext.Rooms
            .Select(r => new RoomResponse(r.Id, r.Name, r.Capacity))
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<IReadOnlyCollection<RoomResponse>>.Ok(rooms));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RoomResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms
            .Where(r => r.Id == id)
            .Select(r => new RoomResponse(r.Id, r.Name, r.Capacity))
            .FirstOrDefaultAsync(cancellationToken);

        if (room is null)
        {
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy phòng học yêu cầu."));
        }

        return Ok(ApiResponse<RoomResponse>.Ok(room));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RoomResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRoomRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(ApiResponse<object>.Fail("Tên phòng học không được để trống."));
        }

        if (request.Capacity <= 0)
        {
            return BadRequest(ApiResponse<object>.Fail("Sức chứa phòng học phải lớn hơn 0."));
        }

        var isNameExists = await _dbContext.Rooms.AnyAsync(r => r.Name == request.Name.Trim(), cancellationToken);
        if (isNameExists)
        {
            return BadRequest(ApiResponse<object>.Fail("Tên phòng học đã tồn tại trong hệ thống."));
        }

        var room = new Room
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Capacity = request.Capacity,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new RoomResponse(room.Id, room.Name, room.Capacity);
        return Ok(ApiResponse<RoomResponse>.Ok(response, "Tạo phòng học thành công."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RoomResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoomRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(ApiResponse<object>.Fail("Tên phòng học không được để trống."));
        }

        if (request.Capacity <= 0)
        {
            return BadRequest(ApiResponse<object>.Fail("Sức chứa phòng học phải lớn hơn 0."));
        }

        var room = await _dbContext.Rooms.FindAsync(new object[] { id }, cancellationToken);
        if (room is null)
        {
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy phòng học yêu cầu để cập nhật."));
        }

        var isNameExists = await _dbContext.Rooms
            .AnyAsync(r => r.Name == request.Name.Trim() && r.Id != id, cancellationToken);
        if (isNameExists)
        {
            return BadRequest(ApiResponse<object>.Fail("Tên phòng học đã tồn tại trong hệ thống."));
        }

        room.Name = request.Name.Trim();
        room.Capacity = request.Capacity;
        room.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new RoomResponse(room.Id, room.Name, room.Capacity);
        return Ok(ApiResponse<RoomResponse>.Ok(response, "Cập nhật phòng học thành công."));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms.FindAsync(new object[] { id }, cancellationToken);
        if (room is null)
        {
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy phòng học yêu cầu để xóa."));
        }

        // Kiểm tra xem phòng học có đang được dùng trong lịch học (ClassSchedules) nào không
        var hasClassSchedules = await _dbContext.ClassSchedules.AnyAsync(cs => cs.RoomId == id, cancellationToken);
        if (hasClassSchedules)
        {
            return BadRequest(ApiResponse<object>.Fail("Không thể xóa phòng học này vì đang được gán lịch học của lớp."));
        }

        // Kiểm tra trong ScheduleOccurrences (các buổi học cụ thể)
        var hasOccurrences = await _dbContext.ScheduleOccurrences.AnyAsync(so => so.RoomId == id, cancellationToken);
        if (hasOccurrences)
        {
            return BadRequest(ApiResponse<object>.Fail("Không thể xóa phòng học này vì đang được dùng cho buổi học thực tế."));
        }

        _dbContext.Rooms.Remove(room);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<object>.Ok(null, "Xóa phòng học thành công."));
    }
}

public sealed record RoomResponse(Guid Id, string Name, int Capacity);

public sealed class CreateRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
}

public sealed class UpdateRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
}


