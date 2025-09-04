using Microsoft.AspNetCore.Mvc;
using RabbitMQDemo.Shared;
using RabbitMQDemo.Contracts;
using System.Threading;

namespace RabbitMQDemo.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagingController : ControllerBase
    {
        private readonly RabbitMQService _rabbit;
        private static int _clientCounter = 0; // in-memory counter for auto IDs
        private static object _lock = new object();

        public MessagingController(RabbitMQService rabbit)
        {
            _rabbit = rabbit;
        }

        [HttpPost("Send")]
        public IActionResult SendMessage([FromBody] ClientRequest request)
        {
            string clientId;

            if (!string.IsNullOrEmpty(request.ClientId))
            {
                // Use provided client ID
                clientId = request.ClientId;
            }
            else
            {
                // Generate new client ID
                lock (_lock)
                {
                    clientId = $"client{++_clientCounter}";
                }
            }

            string routingKey = clientId; // the queue name / routing key

            // Ensure queue exists
            _rabbit.QueueDeclareAndBind(routingKey);

            // Publish the message
            _rabbit.Publish(routingKey, request);

            return Ok(new { AssignedClientId = clientId, Message = "Message sent" });
        }
    }
}