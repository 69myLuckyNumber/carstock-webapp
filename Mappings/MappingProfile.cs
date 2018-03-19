using System.Linq;
using AutoMapper;
using car_heap.Controllers.Resources;
using car_heap.Core.Models;

namespace car_heap.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Domain to API Resources
            CreateMap<Make, MakeResource>()
                .ForMember(ms => ms.Id, opts => opts.MapFrom(m => m.MakeId))
                .ForMember(ms => ms.Name, opts => opts.MapFrom(m => m.Name))
                .ForMember(ms => ms.Models, opts => opts.MapFrom(m => m.Models));
            CreateMap<Model, ModelResource>()
                .ForMember(mds => mds.Id, opts => opts.MapFrom(m => m.ModelId))
                .ForMember(mds => mds.Name, opts => opts.MapFrom(m => m.Name));
            CreateMap<Make, KeyValuePairResource>()
                .ForMember(ms => ms.Id, opts => opts.MapFrom(m => m.MakeId))
                .ForMember(ms => ms.Name, opts => opts.MapFrom(m => m.Name));
            CreateMap<Model, KeyValuePairResource>()
                .ForMember(mds => mds.Id, opts => opts.MapFrom(m => m.ModelId))
                .ForMember(mds => mds.Name, opts => opts.MapFrom(m => m.Name));
            CreateMap<Feature, FeatureResource>()
                .ForMember(fs => fs.Id, opts => opts.MapFrom(f => f.FeatureId))
                .ForMember(fs => fs.Name, opts => opts.MapFrom(f => f.Name))
                .ForMember(fs => fs.Description, opts => opts.MapFrom(f => f.Description));
            CreateMap<Status, KeyValuePairResource>()
                .ForMember(s => s.Id, opts => opts.MapFrom(s => s.StatusId))
                .ForMember(s => s.Name, opts => opts.MapFrom(s => s.Name));
            CreateMap<Order, OrderResource>()
                .ForMember(os => os.VehicleId, opts => opts.MapFrom(o => o.VehicleId))
                .ForMember(os => os.UserId, opts => opts.MapFrom(o => o.UserId))
                .ForMember(os => os.Status, opts =>
                    opts.MapFrom(o => new KeyValuePairResource { Id = o.StatusId, Name = o.Status.Name }));
            CreateMap<User, UserResource>()
                .ForMember(us => us.Id, opts => opts.MapFrom(u => u.UserId))
                .ForMember(us => us.Username, opts => opts.MapFrom(u => u.Username))
                .ForMember(us => us.DateRegistered, opts => opts.MapFrom(u => u.DateRegistered))
                .ForMember(us => us.Contact, opts =>
                    opts.MapFrom(u => new ContactResource
                    {
                        Id = u.Contact.ContactId, FirstName = u.Contact.FirstName,
                            LastName = u.Contact.LastName, Email = u.Contact.Email,
                            Phone = u.Contact.Phone
                    }));
            CreateMap<Vehicle, VehicleResource>()
                .ForMember(vr => vr.Id, opts => opts.MapFrom(v => v.VehicleId))
                .ForMember(vr => vr.IsRegistered, opts => opts.MapFrom(v => v.IsRegistered))
                .ForMember(vr => vr.LastUpdated, opts => opts.MapFrom(v => v.LastUpdated))
                .ForMember(vr => vr.Make, opts =>
                    opts.MapFrom(v => new KeyValuePairResource { Id = v.Model.MakeId, Name = v.Model.Make.Name }))
                .ForMember(vr => vr.Model, opts =>
                    opts.MapFrom(v => new KeyValuePairResource { Id = v.ModelId, Name = v.Model.Name }))
                .ForMember(vr => vr.Name, opts => opts.MapFrom(v => v.Name))
                .ForMember(vr => vr.Orders, opts => opts.MapFrom(v =>
                    v.Orders.Select(o => new OrderResource
                    {
                        VehicleId = o.VehicleId, UserId = o.UserId,
                        Comment = o.Comment, DateExpired = o.DateExpired, DateRequested = o.DateRequested,
                        Status = new KeyValuePairResource { Id = o.StatusId, Name = o.Status.Name }
                    })))
                .ForMember(vr => vr.User, opts => opts.MapFrom(v => new UserResource
                {
                    Id = v.UserId,
                    Username = v.User.Username,
                    DateRegistered = v.User.DateRegistered,
                    Contact = new ContactResource
                    {
                        Id = v.User.Contact.ContactId,
                        FirstName = v.User.Contact.FirstName,
                        LastName = v.User.Contact.LastName,
                        Email = v.User.Contact.Email,
                        Phone = v.User.Contact.Phone
                    }
                }))
                .ForMember(vr => vr.Features, opts =>
                    opts.MapFrom(v => v.Features.Select(f =>
                        new FeatureResource
                        {
                            Id = f.FeatureId,
                            Name = f.Feature.Name,
                            Description = f.Feature.Description
                        })));

            // API Resources to Domain
            CreateMap<SaveVehicleResource, Vehicle>()
                .ForMember(v => v.VehicleId, opts => opts.Ignore())
                .ForMember(v => v.UserId, opts => opts.MapFrom(vr => vr.UserId))
                .ForMember(v => v.ModelId, opts => opts.MapFrom(vr => vr.ModelId))
                .ForMember(v => v.Name, opts => opts.MapFrom(vr => vr.Name))
                .ForMember(v => v.IsRegistered, opts => opts.MapFrom(vr => vr.IsRegistered))
                .ForMember(v => v.Features, opts => opts.Ignore())
                .AfterMap((vr, v)=>
                {
                    // delete features
                    var removed = v.Features.Where(f => !vr.Features.Contains(f.FeatureId)).ToList();
                    removed.ForEach(f => v.Features.Remove(f));

                    // add features
                    var added = vr.Features.Where(id => !v.Features.Any(f => f.FeatureId == id))
                        .Select(id => new Integration { FeatureId = id })
                        .ToList();
                    added.ForEach(i => v.Features.Add(i));
                });

        }
    }
}