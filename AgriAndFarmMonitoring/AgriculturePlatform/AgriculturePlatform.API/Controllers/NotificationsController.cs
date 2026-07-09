using AgriculturePlatform.Application.DTOs;
using AgriculturePlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/farms/{farmId}/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationsController(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    private (int? AdminId, int? WorkerId) GetUserIdentifiers()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        int? adminId = null;
        int? workerId = null;

        if (int.TryParse(userIdStr, out int parsedId))
        {
            if (role == "Admin")
            {
                adminId = parsedId;
            }
            else if (role == "Worker")
            {
                workerId = parsedId;
            }
        }
        return (adminId, workerId);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllNotifications(int farmId)
    {
        var (adminId, workerId) = GetUserIdentifiers();
        var notifications = await _notificationRepository.GetRecentByUserAsync(farmId, adminId, workerId, 50); // Just return 50 recent
        
        var dtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            FarmId = n.FarmId,
            AdminId = n.AdminId,
            WorkerId = n.WorkerId,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            IsRead = n.IsRead,
            ActionUrl = n.ActionUrl,
            Metadata = n.Metadata,
            CreatedAt = n.CreatedAt
        });

        // The Angular UI expects an array directly, not wrapped in { Success, Data }
        return Ok(dtos);
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications(int farmId)
    {
        var (adminId, workerId) = GetUserIdentifiers();
        var notifications = await _notificationRepository.GetUnreadByUserAsync(farmId, adminId, workerId);
        
        var dtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            FarmId = n.FarmId,
            AdminId = n.AdminId,
            WorkerId = n.WorkerId,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            IsRead = n.IsRead,
            ActionUrl = n.ActionUrl,
            Metadata = n.Metadata,
            CreatedAt = n.CreatedAt
        });

        return Ok(dtos);
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentNotifications(int farmId, [FromQuery] int limit = 50)
    {
        var (adminId, workerId) = GetUserIdentifiers();
        var notifications = await _notificationRepository.GetRecentByUserAsync(farmId, adminId, workerId, limit);
        
        var dtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            FarmId = n.FarmId,
            AdminId = n.AdminId,
            WorkerId = n.WorkerId,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            IsRead = n.IsRead,
            ActionUrl = n.ActionUrl,
            Metadata = n.Metadata,
            CreatedAt = n.CreatedAt
        });

        return Ok(new { Success = true, Data = dtos });
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int farmId, int id)
    {
        // Ideally check if user has access to this notification.
        await _notificationRepository.MarkAsReadAsync(id);
        return Ok(new { Success = true, Message = "Notification marked as read." });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(int farmId)
    {
        var (adminId, workerId) = GetUserIdentifiers();
        await _notificationRepository.MarkAllAsReadAsync(farmId, adminId, workerId);
        return Ok(new { Success = true, Message = "All notifications marked as read." });
    }
}
