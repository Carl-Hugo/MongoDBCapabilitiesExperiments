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
    public class ReadOneRawDocument
    {
        public class Query : IRequest<Result>
        {
            public string Id { get; set; }
        }

        public class Result : Dictionary<string, object>
        {

        }

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.Id).ObjectId();
            }
        }

        public class Handler : IRequestHandler<Query, Result>
        {
            private readonly IMongoClient _mongo;
            private readonly MongoDocumentOptions _options;

            public Handler(IMongoClient mongo, MongoDocumentOptions options)
            {
                _mongo = mongo ?? throw new ArgumentNullException(nameof(mongo));
                _options = options ?? throw new ArgumentNullException(nameof(options));
            }

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                var database = _mongo.GetDatabase(_options.DatabaseName);
                var documents = database.GetCollection<Result>(_options.CollectionName);

                var filter = new BsonDocument("_id", request.Id);
                var result = await documents
                    .Find(filter)
                    .FirstOrDefaultAsync(cancellationToken);

                if (result == null)
                {
                    throw new DocumentNotFoundException(request.Id);
                }
                return result;
            }
        }
    }
}
