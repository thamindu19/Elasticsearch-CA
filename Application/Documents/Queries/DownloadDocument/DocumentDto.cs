using Domain.Entities.Documents;
using Microsoft.AspNetCore.Mvc;

namespace Application.Documents.Queries.DownloadDocument;

public class DocumentDto : Document
{
    public string FileId { get; set; }
    public string? FileName { get; set; }
    public string[] Tags { get; set; }
    public FileStreamResult FileStream { get; set; }
}