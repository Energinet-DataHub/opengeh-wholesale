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

using Energinet.DataHub.Wholesale.CalculationResults.Interfaces.CalculationResults.Model.WholesaleResults;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.CalculationResults.Statements;

public sealed class WholesaleServicesQuerySnippetProviderFactory(
    IEnumerable<IWholesaleServicesDatabricksContract> databricksContracts)
{
    private readonly IDictionary<AmountType, IWholesaleServicesDatabricksContract> _databricksContracts =
        databricksContracts
            .DistinctBy(hc => hc.GetAmountType())
            .ToDictionary(hc => hc.GetAmountType());

    public WholesaleServicesQuerySnippetProvider Create(WholesaleServicesQueryParameters parameters)
    {
        return new WholesaleServicesQuerySnippetProvider(_databricksContracts[parameters.AmountType], parameters);
    }
}
