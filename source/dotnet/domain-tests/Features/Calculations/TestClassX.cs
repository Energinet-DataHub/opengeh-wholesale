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

using Test.Core;
using Xunit;

namespace Energinet.DataHub.Wholesale.DomainTests.Features.Calculations
{
    public class TestClassX
    {
        public TestClassX()
        {
        }

        [Fact]
        public async Task Test()
        {
            using var stream = EmbeddedResources.GetStream<Root>("Features.Calculations.TestData.amount_for_es_for_hourly_tarif_40000_for_e17_e02.csv");
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

            }
        }
    }
}
