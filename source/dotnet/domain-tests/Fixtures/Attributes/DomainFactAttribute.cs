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

using Energinet.DataHub.Wholesale.DomainTests.Fixtures.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Energinet.DataHub.Wholesale.DomainTests.Fixtures.Attributes
{
    /// <summary>
    /// Use this to mark domain tests (facts).
    ///
    /// On developer machines we use the 'domaintest.local.settings.json' to set the 'DOMAINFACT_SKIP' value.
    /// On hosted agents we must set it using an environment variable.
    /// </summary>
    public sealed class DomainFactAttribute : FactAttribute
    {
        private static readonly Lazy<bool> _shouldSkip = new Lazy<bool>(ShouldSkip);

        public DomainFactAttribute()
        {
            if (_shouldSkip.Value)
                Skip = "Domain fact was configured to be skipped.";
        }

        private static bool ShouldSkip()
        {
            var configuration = new DomainTestConfiguration();
            return configuration.Root.GetValue("DOMAINFACT_SKIP", defaultValue: true);
        }
    }
}
