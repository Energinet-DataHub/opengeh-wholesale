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

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures
{
    /// <summary>
    /// Responsible for retrieving settings necessary for performing domain tests of 'Wholesale'.
    ///
    /// On developer machines we use the 'domaintest.local.settings.json' to set values.
    /// On hosted agents we must set these using environment variables.
    /// </summary>
    public class WholesaleDomainConfiguration : DomainTestConfiguration
    {
        public WholesaleDomainConfiguration()
        {
            UserTokenConfiguration = B2CUserTokenConfiguration.CreateFromConfiguration(Root);
            WebApiBaseAddress = new Uri(Root.GetValue<string>("WEBAPI_BASEADDRESS"));
            ExistingBatchId = Root.GetValue<Guid>("EXISTING_BATCHID");
            ExistingGridAreaCode = Root.GetValue<string>("EXISTING_GRIDAREACODE");
        }

        /// <summary>
        /// Settings necessary to retrieve a user token for authentication with Wholesale Web API in live environment.
        /// </summary>
        public B2CUserTokenConfiguration UserTokenConfiguration { get; }

        /// <summary>
        /// Base address setting for Wholesale Web API in live environment.
        /// </summary>
        public Uri WebApiBaseAddress { get; }

        /// <summary>
        /// An existing batch id, which can be used to call "batch" Web API in live environment.
        /// </summary>
        public Guid ExistingBatchId { get; }

        /// <summary>
        /// An existing grid area code, which can be used to call "batch" Web API in live environment.
        /// </summary>
        public string ExistingGridAreaCode { get; }
    }
}
