
using Application.Documents.Commands.PublishText;
using Application.Documents.Commands.UploadDocument;
using Microsoft.AspNetCore.Mvc;
using Application.Documents.Queries.DownloadDocument;
using Domain.Entities.Documents;
using MediatR;

namespace Interfaces.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DocumentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult> UploadDocument([FromForm(Name = "file")] IFormFile file,
            [FromForm(Name = "tags")] string[] tags, [FromForm(Name = "accessRoles")] string[] accessRoles)
        {
            var result = await _mediator.Send(new UploadDocumentCommand() { File = file, Tags = tags, AccessRoles = accessRoles});
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult> DownloadDocument([FromQuery] string fileId)
        {
            var fileStream = new DownloadDocumentQuery() { FileId = fileId };
            return Ok(fileStream);
        }
        
        [HttpPost("PublishText")]
        public async Task<Document> ReadText([FromForm(Name = "fileId")] string fileId)
        {
            var document = await _mediator.Send(new PublishTextCommand()
            {
                FileId = fileId
            });
            return document;
        }
    }
}