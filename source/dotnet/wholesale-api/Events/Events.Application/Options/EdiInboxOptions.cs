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

namespace Energinet.DataHub.Wholesale.Events.Application.Options;

public class EdiInboxOptions
{
    /// <summary>
    /// Connection string for EDI inbox.
    /// </summary>
    public string EDI_INBOX_CONNECTION_STRING { get; set; } = string.Empty;

    /// <summary>
    /// Queue name for the EDI inbox.
    /// </summary>
    public string EDI_INBOX_QUEUE_NAME { get; set; } = string.Empty;
}
