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
    public class CreateDocument
    {
        public class Command : IRequest<Result>
        {
            public string Name { get; set; }
            public CommandSettings Settings { get; set; }

            public class CommandSettings
            {
                public bool Enabled { get; set; }
                public string Secret { get; set; }
                public int SomeOtherProperty { get; set; }
            }
        }

        public class Result
        {
            public string Id { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.Settings).SetValidator(new SettingsValidator());
            }
        }

        public class SettingsValidator : AbstractValidator<Command.CommandSettings>
        {
            public SettingsValidator()
            {
                RuleFor(x => x).NotNull();
                RuleFor(x => x.Secret).NotEmpty();
                RuleFor(x => x.SomeOtherProperty).GreaterThan(0);
            }
        }

        public class MapperProfile : Profile
        {
            public MapperProfile()
            {
                CreateMap<Command, Document>()
                    .AfterMap((c, d) => d.Id = ObjectId.GenerateNewId().ToString());
                CreateMap<Command.CommandSettings, Document.DocumentSettings>().ReverseMap();
                CreateMap<Document, Result>();
            }
        }

        public class Handler : IRequestHandler<Command, Result>
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

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var database = _mongo.GetDatabase(_options.DatabaseName);
                var documents = database.GetCollection<Document>(_options.CollectionName);

                var document = _mapper.Map<Document>(request);
                await documents.InsertOneAsync(document);

                var result = _mapper.Map<Result>(document);
                return result;
            }
        }
    }
}
