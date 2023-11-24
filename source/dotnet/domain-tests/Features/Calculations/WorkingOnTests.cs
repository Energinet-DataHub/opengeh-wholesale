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

using System.Globalization;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;
using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.DomainTests.Features.Calculations
{
    public class WorkingOnTests
    {
        public WorkingOnTests()
        {
        }

        [Fact]
        public async Task Test()
        {
            const string ExpectedHeader = "grid_area;energy_supplier_id;quantity;time;price;amount;charge_code;";

            using var stream = EmbeddedResources.GetStream<Root>("Features.Calculations.TestData.amount_for_es_for_hourly_tarif_40000_for_e17_e02.csv");
            using var reader = new StreamReader(stream);

            var hasVerifiedHeader = false;
            var parsedTimeSeriesPoints = new List<WholesaleTimeSeriesPoint>();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (!hasVerifiedHeader)
                {
                    if (line != ExpectedHeader)
                    {
                        throw new Exception($"Cannot parse CSV file. Header is '{line}', expected '{ExpectedHeader}'.");
                    }

                    hasVerifiedHeader = true;
                    continue;
                }

                var columns = line!.Split(';');
                parsedTimeSeriesPoints.Add(new WholesaleTimeSeriesPoint(
                    Time: DateTimeOffset.Parse(columns[3], null, DateTimeStyles.AssumeUniversal),
                    Quantity: decimal.Parse(columns[2], CultureInfo.InvariantCulture),
                    Qualities: new List<QuantityQuality>(),
                    Price: decimal.Parse(columns[4], CultureInfo.InvariantCulture),
                    Amount: decimal.Parse(columns[5], CultureInfo.InvariantCulture)));
            }
        }
    }
}
