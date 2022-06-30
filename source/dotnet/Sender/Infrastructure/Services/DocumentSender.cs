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

using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Extensions;
using Energinet.DataHub.MessageHub.Model.Model;

namespace Energinet.DataHub.Wholesale.Sender.Infrastructure.Services
{
    public class DocumentSender : IDocumentSender
    {
        private readonly IStorageHandler _storageHandler;
        private readonly IDataBundleResponseSender _dataBundleResponseSender;
        private readonly IDocumentFactory _documentFactory;

        public DocumentSender(
            IStorageHandler storageHandler,
            IDataBundleResponseSender dataBundleResponseSender,
            IDocumentFactory documentFactory)
        {
            _storageHandler = storageHandler;
            _dataBundleResponseSender = dataBundleResponseSender;
            _documentFactory = documentFactory;
        }

        public async Task SendAsync(DataBundleRequestDto request)
        {
            using var stream = new MemoryStream();
            await _documentFactory.CreateAsync(request, stream).ConfigureAwait(false);

            var path = await _storageHandler.AddStreamToStorageAsync(stream, request).ConfigureAwait(false);
            var response = request.CreateResponse(path);

            await _dataBundleResponseSender.SendAsync(response).ConfigureAwait(false);
        }
    }
}
