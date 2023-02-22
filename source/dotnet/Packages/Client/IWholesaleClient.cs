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

using Energinet.DataHub.Wholesale.Contracts;

namespace Energinet.DataHub.Wholesale.Client;

public interface IWholesaleClient
{
    /// <summary>
    /// Start processes by creating a batch request.
    /// Returns the batch ID
    /// In case of errors an exception is thrown.
    /// </summary>
    Task<Guid> CreateBatchAsync(BatchRequestDto wholesaleBatchRequestDto);

    /// <summary>
    /// Returns batches matching the search criteria.
    /// In case of errors an exception is thrown.
    /// </summary>
    Task<IEnumerable<BatchDtoV2>> GetBatchesAsync(BatchSearchDto batchSearchDto);

    /// <summary>
    /// Returns batches matching the search criteria.
    /// In case of errors an exception is thrown.
    /// </summary>
    Task<IEnumerable<BatchDtoV2>> GetBatchesAsync(BatchSearchDtoV2 batchSearchDto);

    Task<Stream> GetZippedBasisDataStreamAsync(Guid batchId);

    Task<BatchDtoV2?> GetBatchAsync(Guid batchId);

    Task<ProcessStepResultDto?> GetProcessStepResultAsync(ProcessStepResultRequestDto processStepResultRequestDto);

    Task<ProcessStepResultDto?> GetProcessStepResultAsync(ProcessStepResultRequestDtoV2 processStepResultRequestDtoV2);

    Task<WholesaleActorDto[]?> GetProcessStepActorsAsync(ProcessStepActorsRequest processStepActorsRequest);

    /// <summary>
    /// Process step results provided by the following method:
    /// When only 'EnergySupplierGln' is provided, a result is returned for a energy supplier for the requested grid area, for the specified time series type.
    /// if only a 'BalanceResponsiblePartyGln' is provided, a result is returned for a balance responsible party for the requested grid area, for the specified time series type.
    /// if both 'BalanceResponsiblePartyGln' and 'EnergySupplierGln' is provided, a result is returned for the balance responsible party's energy supplier for requested grid area, for the specified time series type.
    /// if no 'BalanceResponsiblePartyGln' and 'EnergySupplierGln' is provided, a result is returned for the requested grid area, for the specified time series type.
    /// </summary>
    Task<ProcessStepResultDto?> GetProcessStepResultAsync(ProcessStepResultRequestDtoV3 processStepResultRequestDtoV3);
}
