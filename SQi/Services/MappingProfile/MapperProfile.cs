using System;
using AutoMapper;
using Data.MongoCollections;
using Data.ViewModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;

namespace Services.MappingProfile
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<SQiCreateModel, SQi>();
            CreateMap<SQi, SQiViewModel>();
        }
    }
}
