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

using Energinet.DataHub.MessageHub.Model.Peek;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.Wholesale.Sender.Endpoints;

/// <summary>
/// Trigger on request from MessageHub to create a bundle
/// and create bundle and send response to MessageHub.
/// </summary>
public class PeekEndpoint
{
    private const string FunctionName = nameof(PeekEndpoint);
    private readonly IRequestBundleParser _requestBundleParser;
    private readonly IDocumentSender _documentSender;

    public PeekEndpoint(IRequestBundleParser requestBundleParser, IDocumentSender documentSender)
    {
        _requestBundleParser = requestBundleParser;
        _documentSender = documentSender;
    }

    [Function(FunctionName)]
    public Task RunAsync(
        [ServiceBusTrigger(
            "%" + EnvironmentSettingNames.MessageHubRequestQueueName + "%",
            Connection = EnvironmentSettingNames.MessageHubServiceBusListenConnectionString,
            IsSessionsEnabled = true)]
        byte[] data)
    {
        var request = _requestBundleParser.Parse(data);
        return _documentSender.SendAsync(request);
    }
}
