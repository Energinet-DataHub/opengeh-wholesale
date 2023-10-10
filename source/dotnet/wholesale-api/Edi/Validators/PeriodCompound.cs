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

namespace Energinet.DataHub.Wholesale.EDI.Validators;

/// <summary>
/// Is the wrapper for inspection of the period values
/// </summary>
public class PeriodCompound
{
    public PeriodCompound(string startValue, string endValue)
    {
        StarValue = startValue;
        EndValue = endValue;
    }

    public Instant? StartValueAsInstant => InstantPattern.General.Parse(StarValue).Success ? InstantPattern.General.Parse(StarValue).Value : null;

    public Instant? EndValueAsInstant => InstantPattern.General.Parse(EndValue).Success ? InstantPattern.General.Parse(EndValue).Value : null;

    private string StarValue { get; }

    private string EndValue { get; }
}
