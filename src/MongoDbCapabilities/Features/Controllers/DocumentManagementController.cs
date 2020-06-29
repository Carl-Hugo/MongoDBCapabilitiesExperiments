using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace MongoDbCapabilities.Features.Controllers
{
    [ApiController]
    [Route("documents")]
    public class DocumentManagementController : ControllerBase
    {
        private readonly IMediator _mediator;
        public DocumentManagementController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        public async Task<IActionResult> ReadOneAsync([FromQuery]ReadAllDocuments.Query query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ReadOneAsync([FromRoute]ReadDocument.Query query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody]CreateDocument.Command command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(
                nameof(ReadOneAsync).Replace("Async", ""),
                new { id = result.Id },
                result
            );
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOneAsync([FromRoute]DeleteDocument.Command command)
        {
            await _mediator.Send(command);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllAsync([FromQuery]DeleteAllDocuments.Command command)
        {
            await _mediator.Send(command);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> ReplaceAsync([FromBody]ReplaceDocument.Command command)
        {
            await _mediator.Send(command);
            return Ok();
        }
    }
}
