//namespace NServiceBus.Transports.Kafka
//{
//    using System;
//    using System.Collections;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Text;
//    using Logging;

//    class MessageConverter
//    {
      
//        public MessageConverter()
//        {
//            //messageIdStrategy = DefaultMessageIdStrategy;
//        }

//        public MessageConverter(Func<BasicDeliverEventArgs,string> messageIdStrategy)
//        {
//            //this.messageIdStrategy = messageIdStrategy;
//        }

//        public TransportMessage ToTransportMessage(BasicDeliverEventArgs message)
//        {
//            var properties = message.BasicProperties;

//            var messageId = messageIdStrategy(message);

//            var headers = DeserializeHeaders(message);

//            var result = new TransportMessage(messageId, headers)
//            {
//                Body = message.Body ?? new byte[0],
//            };

//            if (properties.IsReplyToPresent())
//            {
//                string replyToAddressNSBHeaders;
//                var nativeReplyToAddress = properties.ReplyTo;

//                if (headers.TryGetValue(Headers.ReplyToAddress, out replyToAddressNSBHeaders))
//                {
//                    if (replyToAddressNSBHeaders != nativeReplyToAddress)
//                    {
//                        Logger.WarnFormat("Missmatching replyto address properties found, the address specified by the NServiceBus headers '{1}' will override the native one '{0}'", nativeReplyToAddress, replyToAddressNSBHeaders);         
//                    }
//                }
//                else
//                {
//                    //promote the native address
//                    headers[Headers.ReplyToAddress] = nativeReplyToAddress;             
//                }
//            }

//            if (properties.IsCorrelationIdPresent())
//            {
//                result.CorrelationId = properties.CorrelationId;
//            }

//            //When doing native interop we only require the type to be set the "fullName" of the message
//            if (!result.Headers.ContainsKey(Headers.EnclosedMessageTypes) && properties.IsTypePresent())
//            {
//                result.Headers[Headers.EnclosedMessageTypes] = properties.Type;
//            }

//            if (properties.IsDeliveryModePresent())
//            {
//                result.Recoverable = properties.DeliveryMode == 2;
//            }


//            return result;
//        }
        
//        static string ValueToString(object value)
//        {
//            var returnValue = default(string);
//            if (value is string)
//            {
//                returnValue = value as string;
//            }
//            else if (value is byte[])
//            {
//                returnValue = Encoding.UTF8.GetString(value as byte[]);
//            }
//            else if (value is IDictionary<string, object>)
//            {
//                var dict = value as IDictionary<string, object>;
//                returnValue = String.Join(",", dict.Select(kvp => kvp.Key + "=" + ValueToString(kvp.Value)));
//            }
//            else if (value is IList)
//            {
//                var list = value as IList;
//                returnValue = String.Join(";", list.Cast<object>().Select(ValueToString));
//            }
//            return returnValue;
//        }

//        static ILog Logger = LogManager.GetLogger(typeof(MessageConverter));
//    }
//}