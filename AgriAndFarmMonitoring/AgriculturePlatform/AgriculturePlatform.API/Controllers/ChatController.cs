using System;
using System.Threading.Tasks;
using AgriculturePlatform.Application.DTOs.AI;
using AgriculturePlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriculturePlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IChatRepository _chatRepository;

        public ChatController(IChatService chatService, IChatRepository chatRepository)
        {
            _chatService = chatService;
            _chatRepository = chatRepository;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessChat([FromBody] ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty.");
            }

            try
            {
                var userIdClaim = User.FindFirst("workerId") ?? User.FindFirst("adminId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    request.UserId = userId;
                }

                var response = await _chatService.ProcessChatAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                // In production, log the exception
                Console.WriteLine($"Error in ChatController: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetMySessions()
        {
            try
            {
                var userIdClaim = User.FindFirst("workerId") ?? User.FindFirst("adminId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized("User ID not found in token.");
                }

                var sessions = await _chatRepository.GetSessionsByUserIdAsync(userId);
                
                var dtos = new System.Collections.Generic.List<ChatSessionDto>();
                foreach(var s in sessions)
                {
                    dtos.Add(new ChatSessionDto
                    {
                        SessionId = s.SessionId,
                        CreatedAt = s.CreatedAt,
                        IsActive = s.IsActive,
                        // Simplistic snippet logic
                        Snippet = s.SessionId.Length > 8 ? "Chat " + s.SessionId.Substring(0, 8) + "..." : "Chat Session"
                    });
                }
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching sessions: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching your sessions.");
            }
        }

        [HttpGet("{sessionId}/messages")]
        public async Task<IActionResult> GetSessionMessages(string sessionId)
        {
            try
            {
                var userIdClaim = User.FindFirst("workerId") ?? User.FindFirst("adminId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized("User ID not found in token.");
                }

                var session = await _chatRepository.GetSessionAsync(sessionId);
                if (session == null || session.UserId != userId)
                {
                    return NotFound("Session not found or unauthorized.");
                }

                var messages = await _chatRepository.GetSessionMessagesAsync(sessionId);
                
                var dtos = new System.Collections.Generic.List<object>();
                foreach(var m in messages)
                {
                    if (!string.IsNullOrEmpty(m.Query))
                    {
                        dtos.Add(new {
                            role = "user",
                            text = m.Query,
                            timestamp = m.Timestamp
                        });
                    }
                    if (!string.IsNullOrEmpty(m.Response))
                    {
                        dtos.Add(new {
                            role = "assistant",
                            text = m.Response,
                            timestamp = m.Timestamp
                        });
                    }
                }
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching messages: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching messages.");
            }
        }
    }
}
