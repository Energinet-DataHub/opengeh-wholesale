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

using YamlDotNet.Serialization.NamingConventions;

namespace Energinet.DataHub.Wholesale.CalculationResults.Infrastructure.Tests.Infrastructure.SettlementReport;

/// <summary>
/// Represents the contract of calculation files between .NET and Databricks applications.
/// </summary>
public class CalculationFilePathsContract
{
    public class ContractFile
    {
        public string DirectoryExpression { get; set; } = null!;

        public string Extension { get; set; } = null!;
    }

    public ContractFile TimeSeriesHourBasisDataFileForTotalGridArea { get; set; } = null!;

    public ContractFile TimeSeriesQuarterBasisDataFileForTotalGridArea { get; set; } = null!;

    public ContractFile MasterBasisDataFileForTotalGridArea { get; set; } = null!;

    public ContractFile TimeSeriesHourBasisDataFileForEsPerGa { get; set; } = null!;

    public ContractFile TimeSeriesQuarterBasisDataFileForEsPerGa { get; set; } = null!;

    public ContractFile MasterBasisDataFileForEsPerGa { get; set; } = null!;

    public ContractFile ResultFile { get; set; } = null!;

    public ContractFile ResultFileForTotalGridArea { get; set; } = null!;

    public ContractFile ResultFileForGaBrpEs { get; set; } = null!;

    public ContractFile ActorsFile { get; set; } = null!;

    public static async Task<CalculationFilePathsContract> GetAsync()
    {
        await using var stream = EmbeddedResources.GetStream("BasisData.calculation-file-paths.yml");
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        using var reader = new StreamReader(stream);
        var resourceString = await reader.ReadToEndAsync();
        return deserializer.Deserialize<CalculationFilePathsContract>(resourceString);
    }
}
