using AutoMapper;
using FluentValidation;
using ForEvolve.ExceptionMapper;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbCapabilities.Features.Models;
using Namotion.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbCapabilities.Features
{
    public class PatchSingleObjectLevel
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
                    .NoBsonDotNotation()
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
