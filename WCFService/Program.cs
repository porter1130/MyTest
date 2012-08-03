using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;

namespace WCFService
{
    class Program
    {
        static void Main(string[] args)
        {
            try {
                BindingElement[] bindingElements = new BindingElement[2];
                bindingElements[0] = new TextMessageEncodingBindingElement();
                bindingElements[1] = new HttpTransportBindingElement();

                CustomBinding binding = new CustomBinding(bindingElements);

                IChannelListener<IReplyChannel> listener=binding.BuildChannelListener<IReplyChannel>(new Uri("http://localhost:9090/RequestReplyService"),new BindingParameterCollection());
                listener.Open();

                IReplyChannel replyChannel = listener.AcceptChannel();
                replyChannel.Open();
                Console.WriteLine("starting to receive message....");

                RequestContext requestContext = replyChannel.ReceiveRequest();
                Console.WriteLine("Received a Message, action:{0},body:{1}", requestContext.RequestMessage.Headers.Action,
                                    requestContext.RequestMessage.GetBody<string>());
                Message message = Message.CreateMessage(binding.MessageVersion, "response", "response Message");
                requestContext.Reply(message);

                requestContext.Close();
                replyChannel.Close();
                listener.Close();

            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            finally {
                Console.Read();
            }
        }
    }
}
