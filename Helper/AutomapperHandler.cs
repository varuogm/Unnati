using AutoMapper;
using Unnati.Models;
using Unnati.Repos.Models;

namespace Unnati.Helper
{
    public class AutomapperHandler : Profile
    {
        public AutomapperHandler()
        {
            CreateMap<TblCustomer, Customermodel>()
                .ForMember(item => item.Statusname,
                            opt => opt.MapFrom(
                                    item => (item.IsActive != null && item.IsActive.Value) ? "Active" : "In active")).ReverseMap();
            //CreateMap<TblUser, UserModel>().ForMember(item => item.Statusname, opt => opt.MapFrom(
            //    item => (item.Isactive != null && item.Isactive.Value) ? "Active" : "In active")).ReverseMap();
        }
    }
}
