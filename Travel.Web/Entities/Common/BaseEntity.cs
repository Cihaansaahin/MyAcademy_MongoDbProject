using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Travel.Web.Entities.Common
{
    public abstract class BaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        // store MongoDB ObjectId as string in the POCO
        public string Id { get; set; }
    }
}
