using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RatesService.Data.Models;

public class ExchangeRateTable
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string TableNo { get; set; }

    public DateTime EffectiveDate { get; set; }

    public List<CurrencyRate> Rates { get; set; } = [];
}
