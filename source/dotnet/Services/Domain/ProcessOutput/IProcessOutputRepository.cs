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

using Energinet.DataHub.Wholesale.Domain.BatchAggregate;
using Energinet.DataHub.Wholesale.Domain.GridAreaAggregate;

namespace Energinet.DataHub.Wholesale.Domain.ProcessOutput;

public interface IProcessOutputRepository
{
    /// <summary>
    /// Create zip archives for each process in the batch.
    /// The archive contains the basis data files and the result file.
    /// </summary>
    Task CreateBasisDataZipAsync(Batch completedBatch);

    Task<ProcessActorResult> GetAsync(Guid batchId, GridAreaCode gridAreaCode);

    Task<Stream> GetZippedBasisDataStreamAsync(Batch batch);
}
