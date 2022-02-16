using System.Text;
using System.Text.Json;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;

            //RabbitMQ Connection Factory
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port  = int.Parse(_configuration["RabbitMQPort"])
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel  = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange : "trigger" , type: ExchangeType.Fanout);

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                Console.WriteLine("--> Connected to MessageBus");

            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"--> Could not connect to the Message bus: {ex.Message}");
            } 
        }

        public void PublishNewPlatform(PlatformPublishedDto platPublishedDto)
        {
            var message = JsonSerializer.Serialize(platPublishedDto);
            if(_connection.IsOpen)
            {
                Console.WriteLine("--> RabbitMQ connection open, sending message...");  

                SendMessage(message);  
            }
            else
            {
                Console.WriteLine("--> RabbitMQ connection close, Not sending...");    
            }
        }

        public void Dispose()
        {
            Console.WriteLine("--> MessageBus Dispose");
            if(_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish( exchange:"trigger", 
                                   routingKey:"",
                                   basicProperties:null,
                                   body:body);

            Console.WriteLine($"--> We hve sent: {message}");                                   
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMQ connection Shutdown");
        }    
    }
}