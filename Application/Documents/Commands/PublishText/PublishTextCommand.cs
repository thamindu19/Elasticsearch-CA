using Application.Documents.Queries.DownloadDocument;
using Azure.Storage.Blobs;
using MediatR;
using Ghostscript.NET.Rasterizer;
using Newtonsoft.Json;
using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.EventHubs;
using Domain.Entities.Documents;
using Domain.Events;
using Microsoft.AspNetCore.Mvc;
using Tesseract;

namespace Application.Documents.Commands.PublishText
{
    public class PublishTextCommand : IRequest<Document>
    {
        public string FileId { get; set; }
    }

    public class PublishTextCommandHandler : IRequestHandler<PublishTextCommand, Document>
    {
        private const string BlobStorageConnectionString =
            "DefaultEndpointsProtocol=https;AccountName=es001;AccountKey=ouz2feuhCKr/XSPMzgA6ADIwoxwmr5+sGWyJ6fYiS0E34U/R0zs1eM4sPtMj0CzJL0Q8aOEdTUBe+ASttcyCUA==;EndpointSuffix=core.windows.net";

        private const string BlobContainerName = "pdfdocuments";
        
        private readonly BlobContainerClient _storageClient;

        private readonly IMediator _mediator;

        public PublishTextCommandHandler(IMediator mediator)
        {
            _storageClient = new BlobContainerClient(BlobStorageConnectionString, BlobContainerName);
            _mediator = mediator;
        }

        public async Task<Document> Handle(PublishTextCommand request, CancellationToken cancellationToken)
        {
            BlobClient blobClient = _storageClient.GetBlobClient($"{request.FileId}.pdf");
            
            using (var rasterizer = new GhostscriptRasterizer())
            {
                using (var stream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(stream);
                    var blobProperties = await blobClient.GetPropertiesAsync();
                    blobProperties.Value.Metadata.TryGetValue("file_name", out string? fileName);
                    blobProperties.Value.Metadata.TryGetValue("tags", out string? tags);
                    blobProperties.Value.Metadata.TryGetValue("access_roles", out string? accessRoles);

                    using (var engine = new TesseractEngine("C:/Program Files/Tesseract-OCR/tessdata", "eng",
                               EngineMode.Default))
                    {
                        rasterizer.Open(stream);

                        string text = "";

                        for (int i = 1; i <= rasterizer.PageCount; i++)
                        {
                            using (var pageImage = rasterizer.GetPage(300, i))
                            {
                                using (var memoryStream = new MemoryStream())
                                {
                                    pageImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                                    memoryStream.Position = 0;

                                    using (var page = engine.Process(Pix.LoadFromMemory(memoryStream.ToArray())))
                                    {
                                        text += page.GetText();
                                    }
                                }
                            }
                        }
                        
                        var document = new Document
                        {
                            FileId = Guid.Parse(request.FileId),
                            FileName = fileName,
                            Tags = tags.Split(','),
                            Message = text,
                            AccessRoles = accessRoles.Split(',')
                        };

                        await _mediator.Publish(new TextPublishedEvent(document));
                        return document;
                    }
                }
            }
        }
    }
}
