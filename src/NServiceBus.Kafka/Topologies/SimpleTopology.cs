namespace NServiceBus.Transports.Kafka.Topologies
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading.Tasks;
    using KafkaNet;
    using KafkaNet.Model;
    using KafkaNet.Protocol;
    using NServiceBus.Serialization;
    using Routing;

    public class SimpleTopology : IRoutingTopology
    {
        private IMessageSerializer serializer;

        public SimpleTopology(IMessageSerializer serializer)
        {
            this.serializer = serializer;
        }

        public Task Initialize(object channel, string main)
        {
            throw new NotImplementedException();
        }

        public Task Publish(object channel, Type type, TransportMessage message, object properties)
        {
            throw new NotImplementedException();
        }

        public async Task Send(object channel, Address address, TransportMessage message, object properties)
        {
            var options = new KafkaOptions(new Uri(address.Machine));
            var router = new BrokerRouter(options);
            var topic = address.Queue;

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(message, stream);

                throw new NotImplementedException();
                //var messageCalue = stream.Re

                using (var client = new Producer(router))
                {
                    await client.SendMessageAsync(topic, new[] { new Message(null) });
                }
            }
        }

        public Task SetupSubscription(object channel, Type type, string subscriberName)
        {
            throw new NotImplementedException();
        }

        public Task TeardownSubscription(object channel, Type type, string subscriberName)
        {
            throw new NotImplementedException();
        }

        private static MemoryStream SerializeToStream(object o)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, o);
            return stream;
        }

        private static object DeserializeFromStream(MemoryStream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object o = formatter.Deserialize(stream);
            return o;
        }
    }
}
