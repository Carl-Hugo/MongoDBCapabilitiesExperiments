using ForEvolve.ExceptionMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;
using System;
using System.IO;
using System.Text;
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
        public async Task<IActionResult> ReadOneAsync([FromQuery] ReadAllDocuments.Query query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ReadOneAsync([FromRoute] ReadDocument.Query query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateDocument.Command command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(
                nameof(ReadOneAsync).Replace("Async", ""),
                new { id = result.Id },
                result
            );
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOneAsync([FromRoute] DeleteDocument.Command command)
        {
            await _mediator.Send(command);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllAsync([FromQuery] DeleteAllDocuments.Command command)
        {
            await _mediator.Send(command);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> ReplaceAsync([FromBody] ReplaceDocument.Command command)
        {
            await _mediator.Send(command);
            return Ok();
        }

        [HttpPatch("patch-name")]
        public async Task<IActionResult> PatchNameAsync([FromBody] PatchName.Command command)
        {
            await _mediator.Send(command);
            return Ok();
        }

        [HttpPatch("single-object-level")]
        public async Task<IActionResult> PatchSingleObjectLevelAsync(
            [ModelBinder(typeof(Patch.ModelBinder))] PatchSingleObjectLevel.Command command)
        {
            await _mediator.Send(command);
            return Ok();
        }

        [HttpPatch("with-dotted-object-support")]
        public async Task<IActionResult> PatchWithDottedObjectSupportAsync(
            [ModelBinder(typeof(Patch.ModelBinder))] PatchWithDottedObjectSupport.Command command)
        {
            await _mediator.Send(command);
            return Ok();
        }

        [HttpPatch("with-object-support")]
        public async Task<IActionResult> PatchWithObjectSupportAsync(
            [ModelBinder(typeof(Patch.ModelBinder))] PatchWithObjectSupport.Command command)
        {
            await _mediator.Send(command);
            return Ok();
        }


        [HttpPatch("with-full-object-support")]
        public async Task<IActionResult> PatchWithFullObjectSupportAsync(
            [ModelBinder(typeof(Patch.ModelBinder))] PatchWithFullObjectSupport.Command command)
        {
            await _mediator.Send(command);
            return Ok();
        }
    }
}
