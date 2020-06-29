using AutoMapper;
using FluentValidation;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbCapabilities.Features
{
    public class DeleteAllDocuments
    {
        public class Command : IRequest { }

        private class Document
        {
            public ObjectId Id { get; set; }
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

                var result = await documents.DeleteManyAsync(x => true, cancellationToken);
                if (!result.IsAcknowledged)
                {
                    throw new NotSupportedException("An unacknowledged delete result is not supported.");
                }
            }
        }
    }
}
