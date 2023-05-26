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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.WebApi.V3.Batch;
using FluentAssertions;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.WebApi.V3;

public static class ProcessTypeMapperTests
{
    [Theory]
    [InlineAutoMoqData(Common.Models.ProcessType.BalanceFixing, ProcessType.BalanceFixing)]
    [InlineAutoMoqData(Common.Models.ProcessType.Aggregation, ProcessType.Aggregation)]
    public static void Map_ReturnsExpectedType(Common.Models.ProcessType source, ProcessType expected)
    {
        var actual = ProcessTypeMapper.Map(source);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineAutoMoqData(ProcessType.BalanceFixing, Common.Models.ProcessType.BalanceFixing)]
    [InlineAutoMoqData(ProcessType.Aggregation, Common.Models.ProcessType.Aggregation)]
    public static void MapProcessType_ReturnsExpectedType(ProcessType source, Common.Models.ProcessType expected)
    {
        var actual = ProcessTypeMapper.Map(source);
        actual.Should().Be(expected);
    }
}
