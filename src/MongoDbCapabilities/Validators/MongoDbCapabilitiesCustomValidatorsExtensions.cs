namespace FluentValidation
{
    using MongoDB.Bson;
    using MongoDbCapabilities.Validators;
    using System;

    public static class MongoDbCapabilitiesCustomValidatorsExtensions
    {
        public static IRuleBuilderOptions<T, string> ObjectId<T>(this IRuleBuilder<T, string> ruleBuilder, bool mustNotBeEmpty = true)
        {
            return ruleBuilder.SetValidator(new ObjectIdValidator(mustNotBeEmpty));
        }

        public static IRuleBuilderOptions<T, BsonDocument> NoBsonSubObject<T>(this IRuleBuilder<T, BsonDocument> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new BsonSubObjectValidator());
        }
        public static IRuleBuilderOptions<T, BsonDocument> NoBsonDotNotation<T>(this IRuleBuilder<T, BsonDocument> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new BsonDotNotationValidator());
        }
        public static IRuleBuilderOptions<T, BsonDocument> WithBsonRule<T>(this IRuleBuilder<T, BsonDocument> ruleBuilder, string message, Func<BsonDocument, bool> isValidFunction)
        {
            return ruleBuilder.SetValidator(new BsonGenericValidator(message, isValidFunction));
        }
        public static IRuleBuilderOptions<T, BsonDocument> HasBsonId<T>(this IRuleBuilder<T, BsonDocument> ruleBuilder)
        {
            return ruleBuilder.WithBsonRule(
                "The BsonDocument must include an '_id' field.",
                document => document.TryGetElement("_id", out _)
            );
            
        }
    }
}
