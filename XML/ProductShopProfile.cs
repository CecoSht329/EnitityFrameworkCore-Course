using AutoMapper;
using ProductShop.Dtos.Import;
using ProductShop.Models;

namespace ProductShop
{
    public class ProductShopProfile : Profile
    {
        public ProductShopProfile()
        {
            //TODO: potentiol error- comment out unnesesery elements if needed
            this.CreateMap<ImportUserDto, User>();
            //this.CreateMap<ImportProductDto, Product>();
        }
    }
}
