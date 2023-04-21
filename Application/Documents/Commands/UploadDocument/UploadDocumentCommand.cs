using Azure.Storage.Blobs;
using Domain.Entities.Documents;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Application.Documents.Commands.UploadDocument;

public class UploadDocumentCommand : IRequest<Document>
{
    public IFormFile File { get; set; }
    public string[] Tags { get; set; }
    public string[] AccessRoles { get; set; }
    public string Group { get; set; }

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

            using (Stream stream = request.File.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            IDictionary<string, string> metadata = new Dictionary<string, string>();
            if (request != null)
            {
                metadata.Add("file_name", request.File.FileName);
                metadata.Add("tags", request.Tags[0]);
                metadata.Add("access_roles", request.AccessRoles[0]);
                metadata.Add("group", request.Group);
            }
            await blobClient.SetMetadataAsync(metadata);
            
            return new Document()
            {
                FileId = fileId,
                FileName = request.File.FileName,
                Tags = request.Tags[0].Split(','),
                AccessRoles = request.AccessRoles[0].Split(','),
                Group = request.Group
            };
        }
    }
}