using FluentValidation;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDbCapabilities.Features
{
    public class PatchWithDottedObjectSupport
    {
        public class Command : Patch.Command
        {
            public Command(BsonDocument patch) : base(patch) { }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Patch)
                    .NotNull()
                    .HasBsonId()
                    .NoBsonSubObject()
                ;
            }
        }

        public class Handler : Patch.Handler<Command>
        {
            public Handler(IMongoClient mongo, MongoDocumentOptions options)
                : base(mongo, options) { }
        }
    }
}
