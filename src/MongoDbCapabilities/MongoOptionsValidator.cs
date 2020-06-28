using Microsoft.Extensions.Options;
using System;

namespace MongoDbCapabilities
{
    public class MongoOptionsValidator : IValidateOptions<MongoOptions>
    {
        public ValidateOptionsResult Validate(string name, MongoOptions options)
        {
            try
            {
                var client = new MyMongoClient(options);
                using var session = client.StartSession();
                return ValidateOptionsResult.Success;
            }
            catch (Exception ex)
            {
                return ValidateOptionsResult.Fail(ex.Message);
            }
        }
    }
}
