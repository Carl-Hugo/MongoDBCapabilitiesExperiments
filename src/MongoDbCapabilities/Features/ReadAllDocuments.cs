using AutoMapper;
using FluentValidation;
using MediatR;
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
    public class ReadAllDocuments
    {
        public class Query : IRequest<IEnumerable<Result>> { }

        public class Result
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class MapperProfile : Profile
        {
            public MapperProfile()
            {
                CreateMap<Document, Result>();
            }
        }

        public class Handler : IRequestHandler<Query, IEnumerable<Result>>
        {
            private readonly IMapper _mapper;
            private readonly IMongoClient _mongo;
            private readonly MongoDocumentOptions _options;

            public Handler(IMapper mapper, IMongoClient mongo, MongoDocumentOptions options)
            {
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
                _mongo = mongo ?? throw new ArgumentNullException(nameof(mongo));
                _options = options ?? throw new ArgumentNullException(nameof(options));
            }

            public Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken)
            {
                var database = _mongo.GetDatabase(_options.DatabaseName);
                var documents = database.GetCollection<Document>(_options.CollectionName);

                var data = documents.AsQueryable();

                var result = _mapper
                    .ProjectTo<Result>(data)
                    .AsEnumerable();
                return Task.FromResult(result);
            }
        }
    }
}
