using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RabbitMQDemo.Shared
{
    public class RabbitMQService : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQService(string host = "localhost")
        {
            var factory = new ConnectionFactory()
            {
                HostName = host,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void QueueDeclareAndBind(string queueName)
        {
            _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queueName, "direct_exchange", queueName);
        }

        public void Publish(string routingKey, object message)
        {
            _channel.ExchangeDeclare("direct_exchange", ExchangeType.Direct, durable: true);
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            _channel.BasicPublish("direct_exchange", routingKey, null, body);
        }

        public void Consume(string queue, Func<string, Task> onMessage)
        {
            QueueDeclareAndBind(queue);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (sender, ea) =>
            {
                var msg = Encoding.UTF8.GetString(ea.Body.ToArray());
                await onMessage(msg);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queue, autoAck: false, consumer: consumer);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}