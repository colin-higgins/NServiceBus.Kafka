namespace NServiceBus.Transports.Kafka
{
    using System;
    using Routing;

    class KafkaSubscriptionManager : IManageSubscriptions
    {
        public IManageKafkaConnections ConnectionManager { get; set; }

        public string EndpointQueueName { get; set; }

        public IRoutingTopology RoutingTopology { get; set; }

        public void Subscribe(Type eventType, Address publisherAddress)
        {
            //using (var connection = ConnectionManager.GetAdministrationConnection())
            //using (var channel = connection.CreateModel())
            //{
            //    RoutingTopology.SetupSubscription(channel, eventType, EndpointQueueName);
            //}
        }

        public void Unsubscribe(Type eventType, Address publisherAddress)
        {
            //using (var connection = ConnectionManager.GetAdministrationConnection())
            //using (var channel = connection.CreateModel())
            //{
            //    RoutingTopology.TeardownSubscription(channel, eventType, EndpointQueueName);
            //}
        }
    }
}