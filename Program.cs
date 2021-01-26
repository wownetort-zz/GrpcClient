using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(channel);

            while (true)
            {
                Console.WriteLine("1 - Simple");
                Console.WriteLine("2 - Stream");

                var result = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(result))
                {
                    continue;
                }

                if (int.TryParse(result, out int variable))
                {
                    switch (variable)
                    {
                        case 1:
                        {
                            await SimpleCall(client);
                            break;
                        }
                        case 2:
                        {
                            await StreamCall(client);
                            break;
                        }
                        default: continue;
                    }
                }
            }
        }

        private static async Task SimpleCall(Greeter.GreeterClient client)
        {
            var result = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(result))
            {
                return;
            }

            var reply = await client.SayHelloAsync(new HelloRequest {Name = result});
            Console.WriteLine("Greeting: " + reply.Message);
            Console.WriteLine();
        }

        private static async Task StreamCall(Greeter.GreeterClient client)
        {
            using var call = client.SayHelloStream();

            var readTask = Task.Run(async () =>
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine(response.Message);
                    Console.WriteLine();
                }
            });

            while (true)
            {
                var result = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(result))
                {
                    break;
                }

                await call.RequestStream.WriteAsync(new HelloRequest() {Name = result});
            }

            await call.RequestStream.CompleteAsync();
            await readTask;
        }
    }
}
