﻿// Copyright 2020 Energinet DataHub A/S
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

using Microsoft.Extensions.Logging;
using Moq;

namespace Energinet.DataHub.Wholesale.Test.Core;

public static class MockILoggerExtensions
{
    public static void ShouldBeCalledWith<T>(this Mock<ILogger<T>> logger, LogLevel logLevel, string expectedMessage)
    {
        logger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(
                    (state, t) =>
                        CheckValue(state, expectedMessage, "{OriginalFormat}")),
                It.IsAny<Exception>(),
                ((Func<It.IsAnyType, Exception, string>)It.IsAny<object>())!));
    }

    private static bool CheckValue(object state, object expectedValue, string key)
    {
        var keyValuePairList = (IReadOnlyList<KeyValuePair<string, object>>)state;

        var actualValue = keyValuePairList.First(kvp => string.Compare(kvp.Key, key, StringComparison.Ordinal) == 0).Value;

        return expectedValue.Equals(actualValue);
    }
}
