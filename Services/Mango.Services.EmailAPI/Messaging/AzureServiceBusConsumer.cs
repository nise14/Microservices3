using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;

namespace Mango.Services.EmailAPI.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
    private readonly string _serviceBusConnectionString;
    private readonly string _emailCartQueue;
    private readonly IConfiguration _configuration;
    private ServiceBusProcessor _emailCartProcessor;
    private readonly EmailService _emailService;

    private static readonly JsonSerializerOptions _propertyCase = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
    {
        _configuration = configuration;

        _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString")!;
        _emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue")!;

        var client = new ServiceBusClient(_serviceBusConnectionString);
        _emailCartProcessor = client.CreateProcessor(_emailCartQueue);
        _emailService = emailService;
    }

    public async Task Start()
    {
        _emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
        _emailCartProcessor.ProcessErrorAsync += ErrorHandler;

        await _emailCartProcessor.StartProcessingAsync();
    }

    public async Task Stop()
    {
        await _emailCartProcessor.StopProcessingAsync();
        await _emailCartProcessor.DisposeAsync();
    }

    private async Task OnEmailCartRequestReceived(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);

        CartDto objMessage = JsonSerializer.Deserialize<CartDto>(body, _propertyCase)!;

        try
        {
            await _emailService.EmailCartAndLog(objMessage);
            await args.CompleteMessageAsync(args.Message);
        }
        catch
        {
            throw;
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }
}