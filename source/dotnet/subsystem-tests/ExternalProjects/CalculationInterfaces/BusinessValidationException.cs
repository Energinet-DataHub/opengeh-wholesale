namespace Energinet.DataHub.Wholesale.Calculations.Interfaces;

public class BusinessValidationException : Exception
{
    public BusinessValidationException(string message)
        : base(message)
    {
    }
}
