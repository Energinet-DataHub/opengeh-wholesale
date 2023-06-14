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

using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures
{
    /// <summary>
    /// Responsible for building the configuration root used for extracting integration test values.
    ///
    /// On developer machines we use the 'integrationtest.local.settings.json' to set values.
    /// On hosted agents we must set these using environment variables.
    /// </summary>
    public class CalculationResultsTestConfiguration
    {
        public CalculationResultsTestConfiguration()
        {
            Root = BuildConfigurationRoot();
        }

        public IConfigurationRoot Root { get; }

        /// <summary>
        /// Load settings from file if available, but also allow
        /// those settings to be overriden using environment variables.
        /// </summary>
        private static IConfigurationRoot BuildConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("integrationtest.local.settings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
