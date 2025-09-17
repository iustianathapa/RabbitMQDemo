using Microsoft.AspNetCore.Mvc;
using RabbitMQDemo.Shared;
using RabbitMQDemo.Contracts;
using System.Collections.Generic;

namespace RabbitMQDemo.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagingController : ControllerBase
    {
        private readonly RabbitMQService _rabbit;

        public MessagingController(RabbitMQService rabbit)
        {
            _rabbit = rabbit;
        }

        /// <summary>
        /// Send KOT or other commands to a specific client
        /// </summary>
        [HttpPost("SendKOT")]
        public IActionResult SendKOT([FromBody] KOTRequest request)
        {
            if (string.IsNullOrEmpty(request.ClientId))
                return BadRequest("ClientId is required");

            string queueName = $"client.{request.ClientId}";

            _rabbit.QueueDeclareAndBind(queueName);

            // Build payload as per ClientRequest
            var payload = new Dictionary<string, string>
            {
                { "BillNo", request.BillNo },
                { "TableNo", request.TableNo },
                { "Waiter", request.Waiter },
                { "Items", request.Items } // format: "Item1,2;Item2,1;Item3,4"
            };

            var clientRequest = new ClientRequest
            {
                ClientId = request.ClientId,
                Method = "Print",
                Payload = payload
            };

            _rabbit.Publish(queueName, clientRequest);

            return Ok(new { Message = "KOT sent to client", Client = request.ClientId });
        }
    }

    /// <summary>
    /// DTO for sending KOT from HMS
    /// </summary>
    public class KOTRequest
    {
        public string ClientId { get; set; } = "";
        public string BillNo { get; set; } = "";
        public string TableNo { get; set; } = "";
        public string Waiter { get; set; } = "";
        public string Items { get; set; } = ""; // format: "Item1,2;Item2,1;Item3,4"
    }
}