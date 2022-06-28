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

using Energinet.DataHub.Wholesale.IntegrationEventListener.Common;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Wholesale.Tests.Application.MeteringPoints;

[UnitTest]
public class DebugTests
{
    [Fact]
    public void Value_IsDefinedCorrectly()
    {
        Debug.Abc.Should().Be("Abcaaa");
    }

    [Fact]
    public void Value2_IsDefinedCorrectly()
    {
        EnvironmentSettingNames.MeteringPointCreatedSubscriptionName.Should().Be("METERING_POINT_CREATED_SUBSCRIPTION_NAME");
    }
}
