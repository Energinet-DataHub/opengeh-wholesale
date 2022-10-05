# Wholesale Client Documentation

A package containing client functionality to integrate with the wholesale domain.

Usage example where `serviceCollection` is an `Microsoft.Extensions.DependencyInjection.IServiceCollection`.
```csharp
        serviceCollection.AddWholesaleClient(
            new Uri("http://some.base.uri"),
            () => "some-authorization-token");
```
