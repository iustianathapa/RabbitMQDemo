namespace RabbitMQDemo.Contracts
{
    public class ClientRequest
    {
        public string? ClientId { get; set; } // optional
        public string Method { get; set; } = "Print"; // e.g. "KOT", "Ping"
        public PrintVM Payload { get; set; } = new();

        // 🔑 New Fields
        public string? Token { get; set; }   // HMS token for auth
        public bool IsPing { get; set; }     // true if this is a connection ping
    }
}