using Data.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.MongoCollections
{
    public class SQi
    {
        [BsonId()]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string District { get; set; }
        public string Province { get; set; }
        public string App { get; set; }
        public int Gender { get; set; }
        public int Age { get; set; }
        public string AgeGroup { get; set; }
        public DateTime SurveyDate { get; set; }
        public int Question1 { get; set; }
        public int Question2 { get; set; }
        public int Question3 { get; set; }
        public string Facility { get; set; }
        public string FacilityName { get; set; }
        public string OtherService { get; set; }
        public int Question4 { get; set; }
        public int Question5 { get; set; }
        public int Question6 { get; set; }
        public List<int> Question7 { get; set; }

        public string User { get; set; }
    }
}
