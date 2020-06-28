using AutoMapper;
using FluentValidation;
using FluentValidation.Validators;
using MediatR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbCapabilities.Features
{
    public class ReadDocument
    {
        public class Query : IRequest<Result>
        {
            public string Id { get; set; }
        }

        public class Result : BaseResult
        {
            public string Id { get; set; }
        }


        public class BaseResult
        {
            public string Name { get; set; }
            public ResultSettings Settings { get; set; }

            public class ResultSettings
            {
                public bool Enabled { get; set; }
                public string Secret { get; set; }
                public int SomeOtherProperty { get; set; }
            }
        }


        public class MapperProfile : Profile
        {
            public MapperProfile()
            {
                CreateMap<Document, Result>();
            }
        }

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .Must(id => ObjectId.TryParse(id, out _))
                    .WithMessage("The 'Id' must be a valid 'ObjectId'.")
                ;
            }
        }
        private class Document : BaseResult
        {
            public ObjectId Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result>
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

            public Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                var database = _mongo.GetDatabase(_options.DatabaseName);
                var documents = database.GetCollection<Document>(_options.CollectionName);

                var data = documents
                    .AsQueryable()
                    .Where(x => x.Id.Equals(ObjectId.Parse(request.Id)))
                    .FirstOrDefault()
                ;

                var result = _mapper.Map<Result>(data);
                return Task.FromResult(result);
            }
        }
    }
}
