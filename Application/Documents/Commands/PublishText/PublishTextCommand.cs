using Application.Documents.Queries.DownloadDocument;
using Azure.Storage.Blobs;
using MediatR;
using Ghostscript.NET.Rasterizer;
using Newtonsoft.Json;
using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.EventHubs;
using Domain.Entities.Documents;
using Microsoft.AspNetCore.Mvc;
using Tesseract;

namespace Application.Documents.Commands.PublishText
{
    public class PublishTextCommand : IRequest<Document>
    {
        public string fileId { get; set; }
    }

    public class PublishTextCommandHandler : IRequestHandler<PublishTextCommand, Document>
    {
        private const string EventHubConnectionString =
            "Endpoint=sb://elasticsearch001.servicebus.windows.net/;SharedAccessKeyName=es;SharedAccessKey=tT1Jc2EumZuy9eeNQwYVLhFWHIb4rWawH+AEhPrS7ZY=;EntityPath=elasticsearch";

        private const string EventHubName = "elasticsearch";

        private const string BlobStorageConnectionString =
            "DefaultEndpointsProtocol=https;AccountName=es001;AccountKey=ouz2feuhCKr/XSPMzgA6ADIwoxwmr5+sGWyJ6fYiS0E34U/R0zs1eM4sPtMj0CzJL0Q8aOEdTUBe+ASttcyCUA==;EndpointSuffix=core.windows.net";

        private const string BlobContainerName = "pdfdocuments";
        private readonly BlobContainerClient _storageClient;
        private readonly EventHubProducerClient _producerClient;

        public PublishTextCommandHandler()
        {
            _storageClient = new BlobContainerClient(BlobStorageConnectionString, BlobContainerName);
            _producerClient = new EventHubProducerClient(EventHubConnectionString, EventHubName);
        }

        public async Task<Document> Handle(PublishTextCommand request, CancellationToken cancellationToken)
        {
            BlobClient blobClient = _storageClient.GetBlobClient($"{request.fileId}.pdf");
            
            using (var rasterizer = new GhostscriptRasterizer())
            {
                using (var stream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(stream);
                    var blobProperties = await blobClient.GetPropertiesAsync();
                    blobProperties.Value.Metadata.TryGetValue("fileName", out string? fileName);
                    blobProperties.Value.Metadata.TryGetValue("tags", out string? tags);
                    blobProperties.Value.Metadata.TryGetValue("accessRoles", out string? accessRoles);

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
                            fileId = Guid.Parse(request.fileId),
                            fileName = fileName,
                            tags = tags.Split(','),
                            message = text,
                            accessRoles = accessRoles.Split(',')
                        };

                        using EventDataBatch eventBatch = await _producerClient.CreateBatchAsync();
                        var eventData = new EventData(JsonConvert.SerializeObject(document));
                        eventBatch.TryAdd(eventData);

                        await _producerClient.SendAsync(eventBatch);
                        return document;
                    }
                }
            }
        }
    }
}
