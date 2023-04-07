
using Application.Documents.Commands.UploadDocument;
using Microsoft.AspNetCore.Mvc;
using Application.Documents.Queries.DownloadDocument;
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

        [HttpGet("UploadDocument")]
        public async Task<ActionResult> UploadDocument([FromForm(Name = "file")] IFormFile file,
            [FromForm(Name = "tags")] string[] tags, [FromForm(Name = "accessRoles")] string[] accessRoles)
        {
            var result = await _mediator.Send(new UploadDocumentCommand() { file = file, tags = tags, accessRoles = accessRoles});
            return Ok(result);
        }

        [HttpGet("DownloadDocument")]
        [Route("{fileId:int}")]
        public async Task<ActionResult> DownloadDocument([FromRoute] Guid fileId)
        {
            var result = await _mediator.Send(new DownloadDocumentQuery() { fileId = fileId });
            return Ok(result);
        }
    }
}