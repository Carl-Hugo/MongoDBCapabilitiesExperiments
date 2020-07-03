using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDbCapabilities.Features.Models
{
    [BsonIgnoreExtraElements]
    public class Document
    {
        [BsonId]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("settings")]
        public DocumentSettings Settings { get; set; }

        [BsonIgnoreExtraElements]
        public class DocumentSettings
        {
            [BsonElement("enabled")]
            public bool Enabled { get; set; }

            [BsonElement("secret")]
            public string Secret { get; set; }

            [BsonElement("someOtherProperty")]
            public int SomeOtherProperty { get; set; }

            [BsonIgnoreIfDefault]
            [BsonElement("patchedOn")]
            public DateTime? PatchedOn { get; set; }
        }
    }

}
