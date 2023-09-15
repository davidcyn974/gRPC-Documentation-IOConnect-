using Grpc.Core;
using GrpcService2;
using System;

namespace GrpcService2.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<PersonReply> GetOperation(PersonRequest request, ServerCallContext context)
        {
            Console.WriteLine("GetOperation");
            if (request.Id == 1)
            {
                return Task.FromResult(new PersonReply
                {
                    Id = 1,
                    Name = "Paul",
                    Address = "Mayotte",
                    Message = ""
                });
            }
            //else :
            return Task.FromResult(new PersonReply
            {
                Id = 0,
                Name = "",
                Address = "",
                Message = "Not Found"
            }); 
        }

        public override Task<Int32Response> PostOperation(PersonRequest request, ServerCallContext context)
        {
            Console.WriteLine("PostOperation");
            return Task.FromResult(new Int32Response
            {
                Value = 2
            });
        }
    }
}