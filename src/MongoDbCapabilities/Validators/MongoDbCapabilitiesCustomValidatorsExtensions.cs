namespace FluentValidation
{
    using MongoDbCapabilities.Validators;

    public static class MongoDbCapabilitiesCustomValidatorsExtensions
    {
        public static IRuleBuilderOptions<T, string> ObjectId<T>(this IRuleBuilder<T, string> ruleBuilder, bool mustNotBeEmpty = true)
        {
            return ruleBuilder.SetValidator(new ObjectIdValidator(mustNotBeEmpty));
        }
    }
}
