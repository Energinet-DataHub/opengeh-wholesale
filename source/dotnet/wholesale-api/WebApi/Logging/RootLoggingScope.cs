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

using System.Reflection;

namespace Energinet.DataHub.Wholesale.WebApi.Logging;

/// <summary>
/// Root logging scope intended to be used as the first scope in the logging scope hierarchy.
/// Usage would likely be where call trees starts.
/// Examples: ASP.NET Core requests, background service invocations, function app endpoints etc.
/// </summary>
public class RootLoggingScope : LoggingScope
{
    public RootLoggingScope()
    {
        // Always log the application entry assembly in order to be able to identify the release
        // and/or source code origin of the deployed code writing the log entry.
        Add("EntryAssembly", Assembly.GetEntryAssembly()?.FullName ?? "Unknown");
    }
}
