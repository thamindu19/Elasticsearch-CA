using Azure.Storage.Blobs;
using Domain.Entities.Documents;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Application.Documents.Commands.UploadDocument;

public class UploadDocumentCommand : IRequest<Document>
{
    public IFormFile file { get; set; }
    public string[] tags { get; set; }
    public string[] accessRoles { get; set; 
}

    public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Document>
    {
        private const string BlobStorageConnectionString =
            "DefaultEndpointsProtocol=https;AccountName=es001;AccountKey=ouz2feuhCKr/XSPMzgA6ADIwoxwmr5+sGWyJ6fYiS0E34U/R0zs1eM4sPtMj0CzJL0Q8aOEdTUBe+ASttcyCUA==;EndpointSuffix=core.windows.net";
        private const string BlobContainerName = "pdfdocuments";
        private readonly BlobContainerClient _storageClient;

        public UploadDocumentCommandHandler()
        {
            _storageClient = new BlobContainerClient(BlobStorageConnectionString, BlobContainerName);
        }

        public async Task<Document> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
        {
            var fileId = Guid.NewGuid();
            var blobName = $"{fileId}.pdf";
            BlobClient blobClient = _storageClient.GetBlobClient(blobName);

            using (Stream stream = request.file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            IDictionary<string, string> metadata = new Dictionary<string, string>();
            if (request.tags != null)
            {
                metadata.Add("fileName", request.file.FileName);
                metadata.Add("tags", request.tags[0]);
                metadata.Add("accessRoles", request.accessRoles[0]);
            }
            await blobClient.SetMetadataAsync(metadata);
            
            return new Document()
            {
                fileId = fileId,
                fileName = request.file.FileName,
                tags = request.tags[0].Split(','),
                accessRoles = request.accessRoles[0].Split(',')
            };
        }
    }
}
