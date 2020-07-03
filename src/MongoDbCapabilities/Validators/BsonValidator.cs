using FluentValidation.Resources;
using FluentValidation.Validators;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MongoDbCapabilities.Validators
{
    public abstract class BsonValidator : PropertyValidator
    {
        public BsonValidator(string message) : base(message) { }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is not BsonDocument)
            {
                return false;
            }

            var value = context.PropertyValue as BsonDocument;
            return IsValid(value);
        }

        protected abstract bool IsValid(BsonDocument value);
    }

    public class BsonSubObjectValidator : BsonValidator
    {
        public BsonSubObjectValidator()
           : base("'{PropertyName}' must not contain sub-objects.")
        {
        }

        protected override bool IsValid(BsonDocument value)
        {
            foreach (var element in value.Elements)
            {
                if (element.Value.BsonType == BsonType.Document)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class BsonDotNotationValidator : BsonValidator
    {
        public BsonDotNotationValidator()
          : base("'{PropertyName}' must not contain 'dot notation' properties.")
        {
        }

        protected override bool IsValid(BsonDocument value)
        {
            foreach (var element in value.Elements)
            {
                if (element.Name.Contains('.'))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class BsonGenericValidator : BsonValidator
    {
        private readonly Func<BsonDocument, bool> _isValidFunction;
        public BsonGenericValidator(string message, Func<BsonDocument, bool> isValidFunction)
            : base(message)
        {
            _isValidFunction = isValidFunction ?? throw new ArgumentNullException(nameof(isValidFunction));
        }

        protected override bool IsValid(BsonDocument value)
        {
            return _isValidFunction(value);
        }
    }
}
