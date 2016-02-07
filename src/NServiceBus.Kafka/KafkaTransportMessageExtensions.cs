namespace NServiceBus.Transports.Kafka
{
    using System;
    using System.Linq;
    using Unicast;

    static class KafkaTransportMessageExtensions
    {
        public static void FillKafkaProperties(TransportMessage message, DeliveryOptions options, dynamic properties)
        {
            properties.MessageId = message.Id;

            if (!String.IsNullOrEmpty(message.CorrelationId))
            {
                properties.CorrelationId = message.CorrelationId;
            }

            if (message.TimeToBeReceived < TimeSpan.MaxValue)
            {
                properties.Expiration = message.TimeToBeReceived.TotalMilliseconds.ToString();
            }

            properties.SetPersistent(message.Recoverable);

            properties.Headers = message.Headers.ToDictionary(p => p.Key, p => (object)p.Value);

            if (message.Headers.ContainsKey(Headers.EnclosedMessageTypes))
            {
                properties.Type = message.Headers[Headers.EnclosedMessageTypes].Split(new[]
                {
                    ','
                }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            }

            if (message.Headers.ContainsKey(Headers.ContentType))
            {
                properties.ContentType = message.Headers[Headers.ContentType];
            }
            else
            {
                properties.ContentType = "application/octet-stream";
            }

            var replyToAddress = options.ReplyToAddress ?? message.ReplyToAddress;
            if (replyToAddress != null)
            {
                properties.ReplyTo = replyToAddress.Queue;
            }
        }
    }
}