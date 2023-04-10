using MediatR;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;

namespace Application.Documents.Queries.DownloadDocument;

public class DownloadDocumentQuery : IRequest<FileStreamResult>
{
    public string fileId { get; set; }
}

public class DownloadDocumentQueryHandler : IRequestHandler<DownloadDocumentQuery, FileStreamResult>
{
    private const string BlobStorageConnectionString =
        "DefaultEndpointsProtocol=https;AccountName=es001;AccountKey=ouz2feuhCKr/XSPMzgA6ADIwoxwmr5+sGWyJ6fYiS0E34U/R0zs1eM4sPtMj0CzJL0Q8aOEdTUBe+ASttcyCUA==;EndpointSuffix=core.windows.net";
    private const string BlobContainerName = "pdfdocuments";
    private readonly BlobContainerClient _storageClient;

    public DownloadDocumentQueryHandler()
    {
        _storageClient = new BlobContainerClient(BlobStorageConnectionString, BlobContainerName);
    }
    
    public async Task<FileStreamResult> Handle(DownloadDocumentQuery request, CancellationToken cancellationToken)
    {
        var blobClient = _storageClient.GetBlobClient($"{request.fileId}.pdf");
        var blobProperties = await blobClient.GetPropertiesAsync();
        blobProperties.Value.Metadata.TryGetValue("file_name", out string? fileName);
        blobProperties.Value.Metadata.TryGetValue("tags", out string? tags);
        var stream = await blobClient.OpenReadAsync();
        var fileStream = new FileStreamResult(stream, "application/pdf")
        {
            FileDownloadName = $"{fileName}.pdf"
        };
        // var document = new DocumentDto()
        // {
        //     fileId = new Guid(request.fileId),
        //     fileName = fileName,
        //     fileStream = fileStream,
        //     tags = tags.Split(',')
        // };
        return fileStream;
    }
}
