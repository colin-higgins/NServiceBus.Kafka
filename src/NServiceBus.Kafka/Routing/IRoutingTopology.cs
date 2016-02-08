namespace NServiceBus.Transports.Kafka.Routing
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Topology for routing messages on the transport
    /// </summary>
    public interface IRoutingTopology
    {
        /// <summary>
        /// Set up subscription for subscriber to the specified type
        /// </summary>
        /// <param name="channel">Kafka channel to operate on</param>
        /// <param name="type">Type to handle with subscriber</param>
        /// <param name="subscriberName">Subscriber name</param>
        Task SetupSubscription(object channel, Type type, string subscriberName);
        /// <summary>
        /// Stop subscription for subscriber to the specified type
        /// </summary>
        /// <param name="channel">Kafka channel to operate on</param>
        /// <param name="type">Type to handle with subscriber</param>
        /// <param name="subscriberName">Subscriber name</param>
        Task TeardownSubscription(object channel, Type type, string subscriberName);
        /// <summary>
        /// Publish message of the specified type
        /// </summary>
        /// <param name="channel">Kafka channel to operate on</param>
        /// <param name="type">Type to handle with subscriber</param>
        /// <param name="message">Message to publish</param>
        /// <param name="properties">Kafka properties of the message to publish</param>
        Task Publish(object channel, Type type, TransportMessage message, object properties);
        /// <summary>
        /// Send message to the specified endpoint
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="address"></param>
        /// <param name="message"></param>
        /// <param name="properties"></param>
        Task Send(object channel, Address address, TransportMessage message, object properties);

        /// <summary>
        /// Performs any initialisation logic needed (eg creating exchanges and bindings)
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="main"></param>
        Task Initialize(object channel, string main);
    }
}