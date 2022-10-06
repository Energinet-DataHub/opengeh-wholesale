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

namespace Energinet.DataHub.Wholesale.Sender.Infrastructure.Services;

public static class QualityMapper
{
    public static string MapToCim(Quality quality) => quality switch
    {
        Quality.Estimated => "A03",
        Quality.Measured => "A04",
        Quality.Incomplete => "A05",
        _ => throw new ArgumentException($"Invalid quality {quality}", nameof(quality)),
    };
}
