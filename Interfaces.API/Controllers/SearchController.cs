using Application.Documents.Commands.PublishText;
using Application.Documents.Queries.SearchText;
using Domain.Entities.Documents;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Interfaces.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SearchController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery(Name = "message")] string message,
            [FromQuery(Name = "tags")] string[] tags, [FromQuery(Name = "userRole")] string userRole)
        {
            var documents = await _mediator.Send(new SearchTextQuery()
            {
                Message = message, Tags = tags, UserRole =
                    userRole
            });
            return Ok(documents);
        }
    }
}
