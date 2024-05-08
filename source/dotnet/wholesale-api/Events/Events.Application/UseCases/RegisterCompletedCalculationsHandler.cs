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

using Energinet.DataHub.Wholesale.Calculations.Interfaces;
using Energinet.DataHub.Wholesale.Common.Application;
using Energinet.DataHub.Wholesale.Events.Application.CompletedCalculations;

namespace Energinet.DataHub.Wholesale.Events.Application.UseCases;

public class RegisterCompletedCalculationsHandler : IRegisterCompletedCalculationsHandler
{
    private readonly ICalculationsClient _calculationsClient;
    private readonly ICompletedCalculationRepository _completedCalculationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompletedCalculationFactory _completedCalculationFactory;

    public RegisterCompletedCalculationsHandler(
        ICalculationsClient calculationsClient,
        ICompletedCalculationRepository completedCalculationRepository,
        IUnitOfWork unitOfWork,
        ICompletedCalculationFactory completedCalculationFactory)
    {
        _calculationsClient = calculationsClient;
        _completedCalculationRepository = completedCalculationRepository;
        _unitOfWork = unitOfWork;
        _completedCalculationFactory = completedCalculationFactory;
    }

    public async Task RegisterCompletedCalculationsAsync()
    {
        var newCompletedCalculations = await GetNewCompletedCalculationsAsync().ConfigureAwait(false);
        await _completedCalculationRepository.AddAsync(newCompletedCalculations).ConfigureAwait(false);
        await _unitOfWork.CommitAsync().ConfigureAwait(false);
    }

    private async Task<IEnumerable<CompletedCalculation>> GetNewCompletedCalculationsAsync()
    {
        var lastKnownCompletedCalculation = await _completedCalculationRepository.GetLastCompletedOrNullAsync().ConfigureAwait(false);
        var completedCalculationDtos = await _calculationsClient.GetCompletedAfterAsync(lastKnownCompletedCalculation?.CompletedTime).ConfigureAwait(false);
        return _completedCalculationFactory.CreateFromCalculations(completedCalculationDtos);
    }
}
