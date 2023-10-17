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

using Energinet.DataHub.Wholesale.EDI.Models;
using AggregatedTimeSeriesRequest = Energinet.DataHub.Edi.Requests.AggregatedTimeSeriesRequest;

namespace Energinet.DataHub.Wholesale.EDI.Validation.AggregatedTimeSerie.Rules;

public class TimeSeriesTypeValidationRule : IValidationRule<AggregatedTimeSeriesRequest>
{
    public IList<ValidationError> Validate(AggregatedTimeSeriesRequest subject)
    {
        var actorRole = subject.RequestedByActorRole;
        if (actorRole == ActorRoleCode.MeteredDataResponsible)
        {
            return new List<ValidationError>();
        }

        if (subject.MeteringPointType == MeteringPointType.Exchange)
        {
            return new List<ValidationError>
            {
                ValidationError.InvalidTimeSeriesTypeForActor.WithPropertyName(actorRole),
            };
        }

        if (subject.MeteringPointType == MeteringPointType.Consumption && !subject.HasSettlementMethod)
        {
            return new List<ValidationError>
            {
                ValidationError.InvalidTimeSeriesTypeForActor.WithPropertyName(actorRole),
            };
        }

        return new List<ValidationError>();
    }
}
