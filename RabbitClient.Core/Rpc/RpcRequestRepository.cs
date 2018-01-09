using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitClient.Core.Messages;

namespace RabbitClient.Core.Rpc
{
    public class RpcRequestRepository
    {
        private static RpcRequestRepository _instance;
        private readonly ConcurrentDictionary<Guid, Action<RpcResponse>> _requests;

        private RpcRequestRepository()
        {
            _requests = new ConcurrentDictionary<Guid, Action<RpcResponse>>();
        }

        public static RpcRequestRepository GetInstance()
        {
            if (_instance == null)
                _instance = new RpcRequestRepository();
            return _instance;
        }

        public async Task<TResponse> RegisterRpcRequest<TResponse>(IBusMessage message, TimeSpan timeout, Action publishAction)
        {
            var response = default(TResponse);
            RpcError error = null;

            var setEvent = new ManualResetEvent(false);

            Action<IBusMessage> replyCallback = rpcReply =>
            {
                if (rpcReply is RpcError)
                {
                    error = (RpcError)rpcReply;
                    setEvent.Set();
                    return;
                }

                if (!(rpcReply is RpcResponse))
                {
                    throw new Exception("Response is not in the expected format");
                }

                // Deserialize the response
                var item = (RpcResponse)rpcReply;
                var deserialized = JsonConvert.DeserializeObject(item.JsonMessage, item.MessageType);

                if (!(deserialized is TResponse))
                {
                    throw new Exception("Response is not in the expected format");
                }

                response = (TResponse)deserialized;
                if (response == null)
                    throw new InvalidCastException(
                        string.Format(
                            "Could not cast the reply to the correct type. Expected '{0}', received '{1}'",
                            typeof(TResponse).FullName, rpcReply.GetType().FullName));

                setEvent.Set();
            };


            var added = _requests.TryAdd(message.MessageId, replyCallback);
            if (!added)
                throw new Exception(string.Format(
                    "The RPC Repository service already has a reply handler registered for request id '{0}'",
                    message.MessageId));

            var task = Task.Factory.StartNew(() =>
            {
                publishAction();
                setEvent.WaitOne(timeout);

                if (error != null)
                    throw new Exception("An error ocurred while processing the RPC request. See inner exception for details", error.Error);

                if (response == null)
                    throw new TimeoutException(string.Format(
                        "The RPC response timeout of {0} seconds has been reached.",
                        timeout.TotalSeconds));
                return response;
            });
            return await task;
        }

        public void HandleRcpResponse(RpcResponse response)
        {
            Action<RpcResponse> handler;
            var request = _requests.TryRemove(response.Request.RequestId, out handler);
            if (!request)
                throw new Exception(string.Format("No handler was registered for the RPC response for the request id '{0}'", response.Request.RequestId));

            handler(response);
        }
    }
}