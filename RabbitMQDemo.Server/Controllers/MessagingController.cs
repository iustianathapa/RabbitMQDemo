using Microsoft.AspNetCore.Mvc;
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
        public IActionResult SendMessage([FromBody] ClientRequest request)
        {
            if (string.IsNullOrEmpty(request.ClientId))
            {
                request.ClientId = _config["ClientId"];
            }

            string routingKey = $"client.{request.ClientId}";

            // Ensure queue exists
            _rabbit.QueueDeclareAndBind(routingKey);

            // Publish
            _rabbit.Publish(routingKey, request);

            return Ok(new { Message = $"Message sent to {request.ClientId}" });
        }
    }
}