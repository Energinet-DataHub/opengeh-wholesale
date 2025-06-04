using System.Text.RegularExpressions;
using Energinet.DataHub.Wholesale.Calculations.Interfaces;

namespace Energinet.DataHub.Wholesale.Calculations.Application.Model;

public sealed record GridAreaCode
{
    public GridAreaCode(string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        if (!Regex.IsMatch(code, @"^((00\d)|(0[1-9]\d)|([1-9]\d\d))$", RegexOptions.ECMAScript))
            throw new BusinessValidationException("Code must be 3 characters number with left padded zeros");

        Code = code;
    }

    /// <summary>
    /// A max 3 digit number with left padded zeros to ensure an exact total of 3 characters.
    /// Examples: 001, 010, 987
    /// </summary>
    public string Code { get; }
}
