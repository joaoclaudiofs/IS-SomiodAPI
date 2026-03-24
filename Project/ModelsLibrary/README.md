# ModelsLibrary - Somiod API Client Library

This library provides shared models and API client functions for interacting with the Somiod REST API.

## Components

### Models
- `Application` - Represents an application resource
- `Container` - Represents a container resource
- `Subscription` - Represents a subscription resource
- `ContentInstance` - Represents a content/data instance

### API Clients
- `SomiodApiClient` - Async/await API client (recommended for modern applications)
- `SomiodApiClientSync` - Synchronous API client (for .NET Framework WinForms apps)

### Response Wrapper
- `ApiResponse<T>` - Standard response wrapper containing:
  - `IsSuccess` - Boolean indicating if the request succeeded
  - `StatusCode` - HTTP status code
  - `Data` - The response data (generic type T)
  - `Message` - Success or error message

## Usage Examples

### Initialize the Client

```csharp
using ModelsLibrary;

// For synchronous operations (WinForms)
var apiClient = new SomiodApiClientSync("http://localhost:44316/api/somiod");

// For async operations
var apiClient = new SomiodApiClient("http://localhost:44316/api/somiod");
```

### Application Operations

```csharp
// Create an application
var response = apiClient.CreateApplication("MyHouse");
if (response.IsSuccess)
{
    Console.WriteLine($"Application created: {response.Data.ResourceName}");
}

// Get an application
var appResponse = apiClient.GetApplication("MyHouse");

// Get containers in an application
var containersResponse = apiClient.GetApplicationContainers("MyHouse");
if (containersResponse.IsSuccess)
{
    foreach (var containerPath in containersResponse.Data)
    {
        Console.WriteLine(containerPath);
    }
}

// Delete an application
var deleteResponse = apiClient.DeleteApplication("MyHouse");
```

### Container Operations

```csharp
// Create a container (e.g., air conditioner)
var response = apiClient.CreateContainer("MyHouse", "AirConditioner");
if (response.IsSuccess)
{
    Console.WriteLine($"Container created: {response.Data.ResourceName}");
}

// Get a specific container
var containerResponse = apiClient.GetContainer("MyHouse", "AirConditioner");

// Delete a container
var deleteResponse = apiClient.DeleteContainer("MyHouse", "AirConditioner");
```

### Subscription Operations

```csharp
// Create a subscription
var response = apiClient.CreateSubscription(
    applicationName: "MyHouse",
    containerName: "AirConditioner",
    subscriptionName: "MySub1",
    evt: 1,
    endpoint: "mqtt://broker.hivemq.com/somiod/myhouse/ac"
);

if (response.IsSuccess)
{
    Console.WriteLine($"Subscription created: {response.Data.Endpoint}");
}

// Get a subscription
var subResponse = apiClient.GetSubscription("MyHouse", "AirConditioner", "MySub1");

// Delete a subscription
var deleteResponse = apiClient.DeleteSubscription("MyHouse", "AirConditioner", "MySub1");
```

### Content Instance Operations

```csharp
// Create a content instance (post data)
var xmlContent = "<data><temperature>25</temperature><cmd>HIGH</cmd></data>";
var response = apiClient.CreateContentInstance(
    applicationName: "MyHouse",
    containerName: "AirConditioner",
    content: xmlContent,
    contentType: "text/xml"
);

// Get a content instance
var dataResponse = apiClient.GetContentInstance("MyHouse", "AirConditioner", "data1");

// Delete a content instance
var deleteResponse = apiClient.DeleteContentInstance("MyHouse", "AirConditioner", "data1");
```

### Async/Await Usage (Modern Applications)

```csharp
private async void btnCreateApp_Click(object sender, EventArgs e)
{
    var apiClient = new SomiodApiClient();
    var response = await apiClient.CreateApplicationAsync("MyHouse");
    
    if (response.IsSuccess)
    {
        MessageBox.Show($"Application created: {response.Data.ResourceName}");
    }
    else
    {
        MessageBox.Show($"Error: {response.Message}");
    }
}
```

### Error Handling

```csharp
var response = apiClient.CreateApplication("MyApp");

if (response.IsSuccess)
{
    // Success
    Console.WriteLine(response.Message);
}
else
{
    // Handle errors
    if (response.StatusCode == 409)
    {
        Console.WriteLine("Application already exists");
    }
    else if (response.StatusCode == 404)
    {
        Console.WriteLine("Resource not found");
    }
    else
    {
        Console.WriteLine($"Error: {response.Message}");
    }
}
```

## WinForms Integration Example

```csharp
public partial class MyForm : Form
{
    private SomiodApiClientSync apiClient;

    public MyForm()
    {
        InitializeComponent();
        apiClient = new SomiodApiClientSync();
    }

    private void Form_Load(object sender, EventArgs e)
    {
        // Create application
        var appResponse = apiClient.CreateApplication("MyHouse");
        
        // Get containers
        var containersResponse = apiClient.GetApplicationContainers("MyHouse");
        if (containersResponse.IsSuccess)
        {
            foreach (var path in containersResponse.Data)
            {
                listBox1.Items.Add(path);
            }
        }
    }

    private void btnCreateContainer_Click(object sender, EventArgs e)
    {
        var response = apiClient.CreateContainer("MyHouse", txtContainerName.Text);
        
        if (response.IsSuccess)
        {
            MessageBox.Show("Container created successfully!");
        }
        else
        {
            MessageBox.Show($"Failed: {response.Message}");
        }
    }
}
```

## Dependencies

- Newtonsoft.Json (13.0.3 or later)
- .NET Framework 4.8

## Installation

1. Add reference to ModelsLibrary.dll in your project
2. Or add the ModelsLibrary project to your solution and reference it

## Notes

- Use `SomiodApiClientSync` for WinForms applications to avoid blocking UI issues
- Use `SomiodApiClient` (async) for ASP.NET or modern applications
- All methods return `ApiResponse<T>` with consistent error handling
- HTTP status codes are preserved in the `StatusCode` property
- 409 (Conflict) typically means the resource already exists
- 404 (Not Found) means the resource doesn't exist
