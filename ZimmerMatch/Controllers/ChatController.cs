using Common.Dto.Chat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Service.Interfaces;
using System.Text.Json; 

namespace ZimmerMatch.Controllers 
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> GetAiAdvice([FromBody] JsonElement rawRequest)
        {
            try
            {

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var request = JsonSerializer.Deserialize<UserRequest>(rawRequest.GetRawText(), options);

                if (request == null || string.IsNullOrEmpty(request.Message))
                    return BadRequest("Message cannot be empty");

                var result = await _chatService.GetVacationAdviceAsync(request);

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}