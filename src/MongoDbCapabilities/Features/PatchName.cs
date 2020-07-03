using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbCapabilities.Features.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbCapabilities.Features
{
    public class PatchName
    {
        public class Command : IRequest
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id).ObjectId();
                RuleFor(x => x.Name).NotEmpty();
            }
        }

        public class Handler : AsyncRequestHandler<Command>
        {
            private readonly IMongoClient _mongo;
            private readonly MongoDocumentOptions _options;

            public Handler(IMongoClient mongo, MongoDocumentOptions options)
            {
                _mongo = mongo ?? throw new ArgumentNullException(nameof(mongo));
                _options = options ?? throw new ArgumentNullException(nameof(options));
            }

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var database = _mongo.GetDatabase(_options.DatabaseName);
                var documents = database.GetCollection<Document>(_options.CollectionName);

                var update = new BsonDocument(
                    "$set",
                    new BsonDocument(new[] {
                        new BsonElement("name", request.Name),
                        new BsonElement("settings.patchedOn", DateTime.UtcNow),
                        new BsonElement("settings.secret", "auto-patched secret"),
                    }.AsEnumerable())
                );
                var result = await documents.UpdateOneAsync(
                    doc => doc.Id == request.Id,
                    update,
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
                        throw new DocumentNotFoundException(request.Id);
                    }
                    if (result.ModifiedCount == 0)
                    {
                        throw new NotImplementedException("For some reason ModifiedCount equals 0.");
                    }
                }
            }
        }
    }
}
