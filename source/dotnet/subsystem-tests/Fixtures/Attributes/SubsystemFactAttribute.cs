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

using Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Fixtures.Attributes
{
    /// <summary>
    /// Use this to mark subsystem tests (facts).
    ///
    /// On developer machines we use the 'subsystemtest.local.settings.json' to set the 'SUBSYSTEMFACT_SKIP' value.
    /// On hosted agents we must set it using an environment variable.
    /// </summary>
    public sealed class SubsystemFactAttribute : FactAttribute
    {
        private static readonly Lazy<bool> _shouldSkip = new Lazy<bool>(ShouldSkip);

        public SubsystemFactAttribute()
        {
            if (_shouldSkip.Value)
                Skip = "Subsystem fact was configured to be skipped.";
        }

        private static bool ShouldSkip()
        {
            var configuration = new SubsystemTestConfiguration();
            return configuration.Root.GetValue("SUBSYSTEMFACT_SKIP", defaultValue: true);
        }
    }
}
