### Faker

#### Introduction
Faker is a C# project designed to simulate various data generation tasks, including purchases, affiliates, products, and more. It integrates with services like Bogus for data generation, webhooks for connecting to other providers, and a service bus for event sourcing.

#### Cloning the Repository
To clone the repository, run the following command:
```bash
git clone https://github.com/DavElizG/Faker.git
cd Faker
```

#### Technologies Used
- C#
- Docker
- Bogus
- Azure Service Bus
- ASP.NET Core

#### Using Webhooks to Connect to Another Provider
The project includes a service (`ErrorLogService`) that logs failed purchases and sends them to a specified webhook endpoint. Here is how it works:
```csharp
public class ErrorLogService : IErrorLogService
{
    private readonly HttpClient _httpClient;

    public ErrorLogService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task LogFailedPurchaseAsync(Purchase purchase, string errorMessage, bool isRetriable)
    {
        var failedPurchase = new
        {
            id = purchase.Id,
            cardNumber = purchase.Card?.CreditCardNumber,
            purchaseDate = purchase.PurchaseDate.ToString("o"),
            amount = purchase.Amount,
            status = purchase.Status.ToString(),
            errorMessage = errorMessage,
            isRetriable = isRetriable,
            createdAt = DateTime.UtcNow.ToString("o")
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(failedPurchase), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("https://your-webhook-url/FailedPurchase", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error sending failed purchase: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception sending failed purchase: {ex.Message}");
        }
    }
}
```

#### Using Bogus
Bogus is used to generate fake data for various entities such as affiliates, products, and cards. Here is an example of how to use Bogus to generate affiliates:
```csharp
public class FakeAffiliateGeneratorService : IAffiliateGeneratorService
{
    private readonly List<Affiliate> _affiliates = new List<Affiliate>();

    public void GenerateAffiliates(int count)
    {
        var faker = new Faker<Affiliate>()
            .RuleFor(a => a.Id, f => Guid.NewGuid())
            .RuleFor(a => a.Name, f => f.Company.CompanyName())
            .RuleFor(a => a.Address, f => f.Address.FullAddress())
            .RuleFor(a => a.PhoneNumber, f => f.Phone.PhoneNumber())
            .RuleFor(a => a.Email, f => f.Internet.Email())
            .RuleFor(a => a.Website, f => f.Internet.Url());

        _affiliates.AddRange(faker.Generate(count));
    }

    public List<Affiliate> GetAffiliates() => _affiliates;
}
```

#### Working with a Service Bus
The project uses Azure Service Bus to send and receive messages for various events, such as purchase events. Here is an example of how the `EventSourceService` sends a purchase event:
```csharp
public class EventSourceService : IEventSource
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<EventSourceService> _logger;

    public EventSourceService(IConfiguration configuration, ILogger<EventSourceService> logger)
    {
        var connectionString = configuration["AzureServiceBus:ConnectionString"];
        var queueName = configuration["AzureServiceBus:QueueName"];

        _client = new ServiceBusClient(connectionString);
        _sender = _client.CreateSender(queueName);
        _logger = logger;
    }

    public async Task SendPurchaseEventAsync(Purchase purchase, bool isSuccess)
    {
        var eventMessage = new
        {
            Id = purchase.Id,
            ProductId = purchase.ProductId,
            // Other properties...
            IsSuccess = isSuccess
        };

        var messageContent = JsonSerializer.Serialize(eventMessage, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        ServiceBusMessage message = new ServiceBusMessage(messageContent);

        try
        {
            _logger.LogInformation("Sending message to Service Bus: {MessageContent}", messageContent);
            await _sender.SendMessageAsync(message);
            _logger.LogInformation("Message sent successfully to Service Bus.");
        }
        catch (ServiceBusException sbEx)
        {
            _logger.LogError(sbEx, "ServiceBusException: Error sending message: {MessageContent}", messageContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message: {MessageContent}", messageContent);
        }
    }
}
```
