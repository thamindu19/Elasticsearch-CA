using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Domain.Entities.Documents;
using MediatR;
using Nest;
using Newtonsoft.Json;

namespace Application.Documents.Queries.SearchText;

public class SearchTextQuery : MediatR.IRequest<List<Document>>
{
    public string Message;
    public string[] Tags;
    public string UserRole;
}

public class SearchTextQueryHandler : IRequestHandler<SearchTextQuery, List<Document>>
{
    private readonly ElasticClient _elasticClient;

    public SearchTextQueryHandler()
    {
        var connectionSettings = new ConnectionSettings(new Uri("https://4.193.154.104:9200/"))
            .DefaultIndex(".ds-logs-azure.eventhub-event_hub-2023.03.29-000001")
            .DisableDirectStreaming()
            .ServerCertificateValidationCallback(OnCertificateValidation)
            .BasicAuthentication("elastic", "=OAxbvmXbY0oRydGrYWG");
        _elasticClient = new ElasticClient(connectionSettings);
    }

    public async Task<List<Document>> Handle(SearchTextQuery request, CancellationToken cancellationToken)
    {
        var response = await _elasticClient.SearchAsync<EsDocument>(s => s
            .From(0)
            .Size(100)
            .Query(q => q
                .Bool(b => b
                    .Must(mu => mu
                        .Match(m => m
                            .Field(f => f.azure.eventhub.message)
                            .Query(request.Message)
                        )
                    )
                    .Filter(fi => fi
                        .Terms(t => t
                            .Field(f => f.azure.eventhub.tags)
                            .Terms(request.Tags)
                        )
                    )
                    .Filter(fi => fi
                        .Terms(t => t
                            .Field(f => f.azure.eventhub.access_roles)
                            .Terms(request.UserRole)
                        )
                    )
                )
            )
            .Source(src => src
                .Includes(i => i
                    .Fields(
                        f => f.azure.eventhub.file_id,
                        f => f.azure.eventhub.file_name,
                        f => f.azure.eventhub.tags,
                        f => f.azure.eventhub.access_roles,
                        f => f.azure.eventhub.message
                    )
                )
            )
        );

        if (response.IsValid)
        {
            var documents = new List<Document>();
            foreach (var doc in response.Documents)
            {
                var document = new Document
                {
                    FileId = doc.azure.eventhub.file_id,
                    FileName = doc.azure.eventhub.file_name,
                    Tags = doc.azure.eventhub.tags,
                    AccessRoles = doc.azure.eventhub.access_roles,
                    Message = doc.azure.eventhub.message
                };
                documents.Add(document);
            }

            // var json = JsonConvert.SerializeObject(documents, Formatting.Indented);
            return documents;
        }
        else
        {
            return null;
        }
    }
    
private static bool OnCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }
    
    public class EsDocument
    {
        public Azure azure;
    }

    public class Azure
    {
        public EventHub eventhub;
    }
    
    public class EventHub
    {
        public Guid file_id { get; set; }
        public string file_name { get; set; }
        public string[] tags { get; set; }
        public string[] access_roles { get; set; }
        public string message { get; set; }
    }
}