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

using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.Wholesale.Application.Processes;
using Energinet.DataHub.Wholesale.Sender.Domain;

namespace Energinet.DataHub.Wholesale.Sender.Application;

public class DataAvailableNotifier : IDataAvailableNotifier
{
    private readonly IDataAvailableNotificationSender _notificationSender;
    private readonly ICorrelationContext _correlationContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProcessRepository _processRepository;
    private readonly IDataAvailableNotificationFactory _dataAvailableNotificationFactory;

    public DataAvailableNotifier(
        IDataAvailableNotificationSender notificationSender,
        ICorrelationContext correlationContext,
        IUnitOfWork unitOfWork,
        IProcessRepository processRepository,
        IDataAvailableNotificationFactory dataAvailableNotificationFactory)
    {
        _notificationSender = notificationSender;
        _correlationContext = correlationContext;
        _unitOfWork = unitOfWork;
        _processRepository = processRepository;
        _dataAvailableNotificationFactory = dataAvailableNotificationFactory;
    }

    public async Task NotifyAsync(ProcessCompletedEventDto completedProcessEvent)
    {
        var notification = _dataAvailableNotificationFactory.Create(completedProcessEvent);
        await CreateAndAddProcessAsync(completedProcessEvent, notification.Uuid).ConfigureAwait(false);
        await _notificationSender.SendAsync(_correlationContext.Id, notification).ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    private async Task CreateAndAddProcessAsync(
        ProcessCompletedEventDto completedProcessEvent,
        Guid notificationUuid)
    {
        var messageHubReference = new MessageHubReference(notificationUuid);
        var process = new Process(messageHubReference, completedProcessEvent.GridAreaCode);
        await _processRepository.AddAsync(process).ConfigureAwait(false);
    }
}
