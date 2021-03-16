using AutoMapper;
using ProductShop.Data;
using ProductShop.Dtos.Import;
using ProductShop.Models;

using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            //Mapper.Initialize(x =>
            //{
            //    x.AddProfile<ProductShopProfile>();
            //});

            var userXml = File.ReadAllText("../../../Datasets/users.xml");
            var productXml = File.ReadAllText("../../../Datasets/products.xml");

            using (ProductShopContext context = new ProductShopContext())
            {
                ResetDatabase(context);

                var result = ImportProducts(context, productXml);

                Console.WriteLine(result);
            }
        }

        //Query 2. Import Products
        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            const string rootElement = "Products";

            var productDtos = XmlConverter.Deserializer<ImportProductDto>(inputXml, rootElement);

            var products = productDtos.Select(p => new Product
            {
                Name = p.Name,
                Price = p.Price,
                BuyerId = p.BuyerId,
                SellerId = p.SellerId
            }).ToArray();

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        //Query 1. Import Users
        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportUserDto[])
                , new XmlRootAttribute("Users"));

            var usersDto = (ImportUserDto[])xmlSerializer.Deserialize(new StringReader(inputXml));

            List<User> users = new List<User>();
            foreach (var userDto in usersDto)
            {
                var user = Mapper.Map<User>(userDto);
                users.Add(user);
            }

            context.Users.AddRange(users);
            context.SaveChanges();
            return $"Successfully imported {users.Count}"; ;
        }


        private static void ResetDatabase(ProductShopContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}