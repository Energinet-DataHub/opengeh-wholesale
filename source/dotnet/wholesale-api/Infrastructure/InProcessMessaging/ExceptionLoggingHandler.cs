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

using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Wholesale.Infrastructure.InProcessMessaging;

public class ExceptionLoggingHandler<TRequest, TResponse, TException> : IRequestExceptionHandler<TRequest, TResponse, TException>
    where TRequest : IRequest<TResponse>
    where TException : Exception
{
    private readonly ILogger<ExceptionLoggingHandler<TRequest, TResponse, TException>> _logger;

    public ExceptionLoggingHandler(ILogger<ExceptionLoggingHandler<TRequest, TResponse, TException>> logger)
    {
        _logger = logger;
    }

    public Task Handle(TRequest request, TException exception, RequestExceptionHandlerState<TResponse> state, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Something went wrong while handling request of type {@requestType}", typeof(TRequest));
        state.SetHandled(default!);
        return Task.CompletedTask;
    }
}
