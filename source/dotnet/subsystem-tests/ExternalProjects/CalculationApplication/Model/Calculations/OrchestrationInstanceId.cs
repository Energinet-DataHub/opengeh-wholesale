namespace Energinet.DataHub.Wholesale.Calculations.Application.Model.Calculations;

public sealed record OrchestrationInstanceId(string Id)
{
    public string Id { get; } = !string.IsNullOrWhiteSpace(Id)
        ? Id
        : throw new ArgumentException($"Id was null or whitespace (value: \"{Id}\")", nameof(Id));
}
