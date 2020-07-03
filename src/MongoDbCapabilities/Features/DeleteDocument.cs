using AutoMapper;
using FluentValidation;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbCapabilities.Features.Models;
using Namotion.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbCapabilities.Features
{
    public class DeleteDocument
    {
        public class Command : IRequest
        {
            public string Id { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id).ObjectId();
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

                var result = await documents.DeleteOneAsync(
                    doc => doc.Id == request.Id,
                    cancellationToken
                );

                if (!result.IsAcknowledged)
                {
                    throw new NotSupportedException("An unacknowledged delete result is not supported.");
                }

                if (result.DeletedCount == 0)
                {
                    throw new DocumentNotFoundException(request.Id);
                }
                else if (result.DeletedCount != 1)
                {
                    throw new NotSupportedException($"Something went wrong and '{result.DeletedCount}' documents were deleted.");
                }
            }
        }
    }
}
