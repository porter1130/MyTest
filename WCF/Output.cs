using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace WCF
{
    class Output
    {
        static void Main(string[] args)
        {
            try
            {
                BindingElement[] bindingElements = new BindingElement[2];
                bindingElements[0] = new TextMessageEncodingBindingElement();
                bindingElements[1] = new HttpTransportBindingElement();

                CustomBinding binding = new CustomBinding(bindingElements);
                using (Message message = Message.CreateMessage(binding.MessageVersion, "sendMessage", "Message Body"))
                {
                    IChannelFactory<IRequestChannel> factory = binding.BuildChannelFactory<IRequestChannel>(new BindingParameterCollection());
                    factory.Open();

                    IRequestChannel requestChannel = factory.CreateChannel(new EndpointAddress("http://localhost:9090/RequestReplyService"));
                    requestChannel.Open();
                    Message response = requestChannel.Request(message);

                    Console.WriteLine("Successful send message!");

                    Console.WriteLine("Receive a return message, action: {0}, body: {1}", response.Headers.Action, response.GetBody<String>());
                    requestChannel.Close();
                    factory.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally {
                Console.Read();
            }
        }
    }
}
