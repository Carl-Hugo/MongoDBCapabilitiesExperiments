using FluentValidation.Validators;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDbCapabilities.Validators
{
    public class ObjectIdValidator : PropertyValidator
    {
        private readonly bool _mustNotBeEmpty;

        public ObjectIdValidator(bool mustNotBeEmpty)
            : base("'{PropertyName}' must be a valid 'ObjectId'.")
        {
            _mustNotBeEmpty = mustNotBeEmpty;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue is not string)
            {
                return false;
            }
            var value = context.PropertyValue as string;
            if (_mustNotBeEmpty && string.IsNullOrEmpty(value))
            {
                return false;
            }
            return ObjectId.TryParse(value, out _);
        }
    }
}
