namespace RabbitMQDemo.Client.Models
{
	public class ClientRequest
	{
		public string? ClientId { get; set; }
		public string Method { get; set; } = "Print";
		public PrintVM Payload { get; set; } 

		// Add these for ping request/response
		public string? ReplyTo { get; set; }
		public string? CorrelationId { get; set; }
	}
	public class ClientPingResponse
	{
		public string CorrelationId { get; set; } = "";
		public string Status { get; set; } = "";
	}
}
