// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using NodaTime;
using NodaTime.Text;

namespace Energinet.DataHub.Wholesale.Edi.Validation.WholesaleServicesRequest.Rules;

public sealed class PeriodValidationRule : IValidationRule<DataHub.Edi.Requests.WholesaleServicesRequest>
{
    private static readonly ValidationError _invalidDateFormat =
        new(
            "Forkert dato format for {PropertyName}, skal være YYYY-MM-DDT22:00:00Z eller YYYY-MM-DDT23:00:00Z / Wrong date format for {PropertyName}, must be YYYY-MM-DDT22:00:00Z or YYYY-MM-DDT23:00:00Z",
            "D66");

    public Task<IList<ValidationError>> ValidateAsync(DataHub.Edi.Requests.WholesaleServicesRequest subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        var periodStart = subject.PeriodStart;

        // This should not be possible, but we need to check for null due to the nullable type
        ArgumentNullException.ThrowIfNull(periodStart);

        var errors = new List<ValidationError>();

        var startInstant = ParseToInstant(periodStart, "Start date", errors);

        return Task.FromResult<IList<ValidationError>>(errors);
    }

    private static Instant? ParseToInstant(
        string dateTimeString,
        string propertyName,
        ICollection<ValidationError> errors)
    {
        var parseResult = InstantPattern.General.Parse(dateTimeString);

        if (parseResult.Success)
            return parseResult.Value;

        errors.Add(_invalidDateFormat.WithPropertyName(propertyName));
        return null;
    }
}
