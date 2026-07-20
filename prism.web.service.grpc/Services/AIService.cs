using Grpc.Core;

namespace prism.web.service.grpc.Services
{
    public class AIService(ILogger<AIService> logger) : Greeter.GreeterBase
    {
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            logger.LogInformation("The message is received from {Name}", request.Name);

            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
    }
}
