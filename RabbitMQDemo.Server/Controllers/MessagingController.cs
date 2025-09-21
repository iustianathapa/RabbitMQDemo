using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RabbitMQDemo.Client.Models;
using RabbitMQDemo.Shared;

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
        public IActionResult SendKOT([FromBody] KotBotPayload request)
        {
            if (string.IsNullOrEmpty(request.ClientId))
                return BadRequest("ClientId is required");

            string queueName = $"{request.ClientId}";

            _rabbit.QueueDeclareAndBind(queueName);

            // Build payload as per ClientRequest
          

            var clientRequest = new ClientRequest
            {
                ClientId = request.ClientId,
                Method = "Print",
                Payload = request.payload
			};

            _rabbit.Publish(queueName, clientRequest);

            return Ok(new { Message = "KOT sent to client", Client = request.ClientId });
        }

        [HttpPost("Ping")]
        public async Task<IActionResult> Ping(string  clientId)
        {
            string queueName = clientId;
			string replyQueue = $"reply.{Guid.NewGuid()}";
			string correlationId = Guid.NewGuid().ToString();

			_rabbit.QueueDeclareAndBind(queueName);
            _rabbit.QueueDeclareAndBind(replyQueue);
			var pingRequest = new ClientPingRequest
			{
				ClientId = clientId,
				Method = "Ping",
				CorrelationId = correlationId,
				ReplyTo = replyQueue
			};
			_rabbit.Publish(queueName, pingRequest);

			// Wait for response (simple implementation: TaskCompletionSource)
			var tcs = new TaskCompletionSource<string>();
			_rabbit.Consume(replyQueue, async msg =>
			{
				var response = JsonSerializer.Deserialize<ClientPingResponse>(msg);
				if (response?.CorrelationId == correlationId)
				{
					tcs.TrySetResult(response.Status);
				}
				await Task.CompletedTask;
			});
			var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));
			if (completed == tcs.Task && tcs.Task.Result == "Ok")
				return Ok("Client is connected");
			else
				return StatusCode(504, "No response from client");
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
	public class ClientPingRequest
	{
		public string ClientId { get; set; } = "";
		public string Method { get; set; } = "Ping";
		public string CorrelationId { get; set; } = "";
		public string ReplyTo { get; set; } = "";
	}
	public class ClientPingResponse
	{
		public string CorrelationId { get; set; } = "";
		public string Status { get; set; } = "";
	}
}