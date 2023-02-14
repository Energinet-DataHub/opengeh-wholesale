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

using Energinet.DataHub.Wholesale.Application.ProcessStep;
using Energinet.DataHub.Wholesale.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.Wholesale.WebApi.Controllers.V2;

[ApiController]
[Route("v2.2/ProcessStepResult")]
public class ProcessStepV22Controller : ControllerBase
{
    private readonly IProcessStepApplicationService _processStepApplicationService;

    public ProcessStepV22Controller(IProcessStepApplicationService processStepApplicationService)
    {
        _processStepApplicationService = processStepApplicationService;
    }

    [AllowAnonymous] // TODO: Temporary hack to enable EDI integration while awaiting architects decision
    [HttpPost]
    [ApiVersion("2.2")]
    public async Task<IActionResult> GetAsync([FromBody] ProcessStepResultRequestDtoV2 processStepResultRequestDtoV2)
    {
        var resultDto = await _processStepApplicationService.GetResultAsync(processStepResultRequestDtoV2).ConfigureAwait(false);
        return Ok(resultDto);
    }
}
