using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RabbitMQDemo.Client.Models;
using RabbitMQDemo.Shared;
using RabbitMQDemo.Contracts;

namespace RabbitMQDemo.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagingController : ControllerBase
    {
        private readonly RabbitMQService _rabbit;
        private readonly IConfiguration _config;

        public MessagingController(RabbitMQService rabbit, IConfiguration config)
        {
            _rabbit = rabbit;
            _config = config;
        }

        [HttpPost("Send")]
        public IActionResult SendMessage([FromBody] KotBotPayload request)
        {
            if (request == null || string.IsNullOrEmpty(request.ClientId))
                return BadRequest("ClientId is required.");

            // ✅ Token validation
            string serverToken = _config["Server:Token"] ?? string.Empty;
            if (request.ApiToken != serverToken)
                return Unauthorized(new { Message = "Invalid token" });

            string queueName = $"client.{request.ClientId}";

            _rabbit.QueueDeclareAndBind(queueName);
            _rabbit.Publish(queueName, request);

            return Ok(new { Message = "KOT sent successfully" });
        }

        // 🔹 New Endpoint for PING
        [HttpPost("Ping")]
        public IActionResult Ping([FromBody] ClientRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ClientId))
                return BadRequest("ClientId is required.");

            // ✅ Token validation
            string serverToken = _config["Server:Token"] ?? string.Empty;
            if (request.Token != serverToken)
                return Unauthorized(new { Message = "Invalid token" });

            request.IsPing = true; // force it as ping
            request.Method = "Ping";

            string queueName = $"client.{request.ClientId}";
            _rabbit.QueueDeclareAndBind(queueName);
            _rabbit.Publish(queueName, request);

            return Ok(new { Message = "Ping request sent" });
        }
    }
}
