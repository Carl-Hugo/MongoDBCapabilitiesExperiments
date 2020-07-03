using ForEvolve.ExceptionMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbCapabilities.Features.Models;
using Namotion.Reflection;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbCapabilities.Features
{
    public class Patch
    {
        public class ModelBinder : IModelBinder
        {
            public async Task BindModelAsync(ModelBindingContext bindingContext)
            {
                var baseCommandType = typeof(Command);
                if (!baseCommandType.IsAssignableFrom(bindingContext.ModelType)){
                    bindingContext.Result = ModelBindingResult.Failed();
                    bindingContext.ModelState.AddModelError(
                        nameof(bindingContext.ModelType),
                        $"The model should inherit from '{baseCommandType.Name}'."
                    );
                    return;
                }

                using var reader = new StreamReader(bindingContext.HttpContext.Request.Body, Encoding.UTF8);
                var body = await reader.ReadToEndAsync();
                if (BsonDocument.TryParse(body, out var document))
                {
                    var model = Activator.CreateInstance(bindingContext.ModelType, document);
                    //var model = new Command(document);
                    bindingContext.Result = ModelBindingResult.Success(model);
                }
                else
                {
                    bindingContext.Result = ModelBindingResult.Failed();
                    bindingContext.ModelState.AddModelError(
                        nameof(body),
                        "The body of the request must be a valid JSON string."
                    );
                }
            }
        }

        public abstract class Command : IRequest
        {
            public Command(BsonDocument patch)
            {
                Patch = patch ?? throw new ArgumentNullException(nameof(patch));
            }
            public BsonDocument Patch { get; }
        }

        public abstract class Handler<TCommand> : AsyncRequestHandler<TCommand>
            where TCommand : Command
        {
            private readonly IMongoClient _mongo;
            private readonly MongoDocumentOptions _options;

            public Handler(IMongoClient mongo, MongoDocumentOptions options)
            {
                _mongo = mongo ?? throw new ArgumentNullException(nameof(mongo));
                _options = options ?? throw new ArgumentNullException(nameof(options));
            }

            protected override async Task Handle(TCommand request, CancellationToken cancellationToken)
            {
                var database = _mongo.GetDatabase(_options.DatabaseName);
                var documents = database.GetCollection<Document>(_options.CollectionName);

                var result = await documents.UpdateOneAsync(
                    new BsonDocument("_id", request.Patch.GetValue("_id")),
                    new BsonDocument("$set", request.Patch),
                    cancellationToken: cancellationToken
                );

                if (!result.IsAcknowledged)
                {
                    throw new NotSupportedException("An unacknowledged update result is not supported.");
                }
                if (result.IsModifiedCountAvailable)
                {
                    if (result.MatchedCount == 0)
                    {
                        var id = request.TryGetPropertyValue<string>("_id");
                        throw new DocumentNotFoundException(id);
                    }
                    if (result.ModifiedCount == 0)
                    {
                        throw new ConflictException("There was nothing to update.");
                    }
                }
            }
        }
    }
}
