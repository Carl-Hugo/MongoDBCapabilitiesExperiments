using FluentValidation;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace MongoDbCapabilities
{
    public class MongoDocumentOptionsValidator : IValidateOptions<MongoDocumentOptions>
    {
        private readonly IValidator<MongoDocumentOptions> _validator;
        public MongoDocumentOptionsValidator(IValidator<MongoDocumentOptions> validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public ValidateOptionsResult Validate(string name, MongoDocumentOptions options)
        {
            var result = _validator.Validate(options);
            if (result.IsValid)
            {
                return ValidateOptionsResult.Success;
            }
            var messages = result.Errors.Select(x => x.ErrorMessage);
            return ValidateOptionsResult.Fail(messages);
        }

        public class Validator : AbstractValidator<MongoDocumentOptions>
        {
            public Validator()
            {
                RuleFor(x => x.DatabaseName).NotEmpty();
                RuleFor(x => x.CollectionName).NotEmpty();
            }
        }
    }
}
