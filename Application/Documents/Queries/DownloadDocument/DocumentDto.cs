using Domain.Entities.Documents;
using Microsoft.AspNetCore.Mvc;

namespace Application.Documents.Queries.DownloadDocument;

public class DocumentDto : Document
{
    public string fileId { get; set; }
    public string? fileName { get; set; }
    public string[] tags { get; set; }
    public FileStreamResult fileStream { get; set; }
}