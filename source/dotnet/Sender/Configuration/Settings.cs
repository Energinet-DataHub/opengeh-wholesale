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

using Energinet.DataHub.Wholesale.Apps.Core.Configuration;

namespace Energinet.DataHub.Wholesale.Sender.Configuration;

public static class Settings
{
    public static Setting<string> AzureWebJobsStorage { get; }
        = new("AzureWebJobsStorage");

    public static Setting<string> AppInsightsInstrumentationKey { get; }
        = new("APPINSIGHTS_INSTRUMENTATIONKEY");
}
