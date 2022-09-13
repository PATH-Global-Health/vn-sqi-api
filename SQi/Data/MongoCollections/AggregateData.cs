using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.MongoCollections
{
    public class AggregateData
    {
        [BsonId()]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string province_code { get; set; }
        public string month { get; set; }
        public string year { get; set; }
        public DateTime date { get; set; }
        public List<IndicatorData> datas { get; set; }
        public bool IsSync { get; set; }
    }
    public class IndicatorData
    {
        public string indicator_code { get; set; }
        public string district_code { get; set; }
        public string site_code { get; set; }
        public Detail data { get; set; }
        public OptionalData optional_data { get; set; }
    }

    public class Detail
    {
        public string sex { get; set; }
        public string age_group { get; set; }
        public string key_population { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public int _value { get; set; }
        public int denominatorValue { get; set; }
    }

    public class OptionalData
    {
        public string value { get; set; }
        public int _value { get; set; }
        public string drug_name { get; set; }
        public string unit_name { get; set; }
        public string data_source { get; set; }
    }
}
