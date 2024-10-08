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

using Energinet.DataHub.Core.Outbox.Application;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.Outbox;

public class OutboxPublisher(IOutboxProcessor outboxProcessor)
{
    public const string FunctionName = nameof(OutboxPublisher);

    private readonly IOutboxProcessor _outboxProcessor = outboxProcessor;

    [Function(FunctionName)]
    public Task PublishOutboxAsync(
        [TimerTrigger("*/10 * * * * *")] TimerInfo timerInfo,
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        return _outboxProcessor.ProcessOutboxAsync();
    }
}
