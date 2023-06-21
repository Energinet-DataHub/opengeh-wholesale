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

using Google.Protobuf;
using NodaTime;

namespace Energinet.DataHub.Wholesale.Events.Infrastructure.Communication;

// ADR-008
// https://energinet.atlassian.net/wiki/spaces/D3/pages/328957986/ADR+008+-+Integration+events+with+protocol+buffers
public record OutboxEvent(Guid EventIdentification, string MessageName, Instant OperationTimeStamp, string MessageVersion, IMessage Message);
