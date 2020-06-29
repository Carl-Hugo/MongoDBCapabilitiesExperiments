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
    public class ReplaceDocument
    {
        public class Command : BaseDocument, IRequest
        {
            public string Id { get; set; }
        }

        private class Document : BaseDocument
        {
            public ObjectId Id { get; set; }
        }

        public class BaseDocument
        {
            public string Name { get; set; }
            public DocumentSettings Settings { get; set; }

            public class DocumentSettings
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
                CreateMap<Command, Document>()
                    .ForMember(x => x.Id, c => c.Ignore())
                    .AfterMap((command, document) =>
                    {
                        document.Id = ObjectId.Parse(command.Id);
                    });
            }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.Settings).SetValidator(new SettingsValidator());
            }
        }

        public class SettingsValidator : AbstractValidator<BaseDocument.DocumentSettings>
        {
            public SettingsValidator()
            {
                RuleFor(x => x).NotNull();
                RuleFor(x => x.Secret).NotEmpty();
                RuleFor(x => x.SomeOtherProperty).GreaterThan(0);
            }
        }

        public class Handler : AsyncRequestHandler<Command>
        {
            private readonly IMapper _mapper;
            private readonly IMongoClient _mongo;
            private readonly MongoDocumentOptions _options;

            public Handler(IMapper mapper, IMongoClient mongo, MongoDocumentOptions options, IMediator mediator)
            {
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
                _mongo = mongo ?? throw new ArgumentNullException(nameof(mongo));
                _options = options ?? throw new ArgumentNullException(nameof(options));
            }

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var database = _mongo.GetDatabase(_options.DatabaseName);
                var documents = database.GetCollection<Document>(_options.CollectionName);

                var filter = Builders<Document>
                    .Filter
                    .Eq(x => x.Id, ObjectId.Parse(request.Id));
                var replacement = _mapper.Map<Document>(request);
                var result = await documents.ReplaceOneAsync(
                    filter,
                    replacement,
                    cancellationToken: cancellationToken
                );
                if (!result.IsAcknowledged)
                {
                    throw new NotSupportedException("An unacknowledged replace result is not supported.");
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
