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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Edi.Responses;
using Energinet.DataHub.Wholesale.Edi.Validation;
using Google.Protobuf;

namespace Energinet.DataHub.Wholesale.Edi.Factories.WholesaleServices;

public class WholesaleServicesRequestRejectedMessageFactory
{
    public static ServiceBusMessage Create(IReadOnlyCollection<ValidationError> errors, string referenceId)
    {
        var body = CreateRejectedMessage(errors);

        var message = new ServiceBusMessage()
        {
            Body = new BinaryData(body.ToByteArray()),
            Subject = body.GetType().Name,
        };

        message.ApplicationProperties.Add("ReferenceId", referenceId);
        return message;
    }

    private static IMessage CreateRejectedMessage(IReadOnlyCollection<ValidationError> errors)
    {
        var response = new WholesaleServicesRequestRejected();
        response.RejectReasons.AddRange(errors.Select(CreateRejectReason));
        return response;
    }

    private static RejectReason CreateRejectReason(ValidationError error)
    {
        return new RejectReason() { ErrorCode = error.ErrorCode, ErrorMessage = error.Message, };
    }
}
