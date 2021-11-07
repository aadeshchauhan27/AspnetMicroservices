using Dapper;
using Discount.API.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discount.API.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly IConfiguration _configuration;
        public DiscountRepository(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public async Task<Coupon> GetDiscount(string productName)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var coupon = await connection.
                QueryFirstOrDefaultAsync<Coupon>("Select * From coupon Where ProductName= @ProductName", new { ProductName = productName });
            if (coupon == null)
                return new Coupon() { ProductName = "no discount", Amount = 0, Description = "No discount Desc" };
            return coupon;
        }

        public async Task<bool> CreateDiscount(Coupon coupon)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var afftected = await connection.
                ExecuteAsync("Insert into coupon (Productname, description, amount) values (@ProductName, @Description, @Amount)",
                new { ProductName = coupon.ProductName, Description = coupon.Description, Amount = coupon.Amount });
            if (afftected == 0)
                return false;
            return true;
        }

        public async Task<bool> UpdateDiscount(Coupon coupon)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var afftected = await connection.
                ExecuteAsync("Update coupon SET ProductName=@ProductName, Description=@Description, Amount=@Amount WHERE Id=@Id",
                new { ProductName = coupon.ProductName, Description = coupon.Description, Amount = coupon.Amount , Id = coupon.Id});
            if (afftected == 0)
                return false;
            return true;
        }


        public async Task<bool> DeleteDiscount(string productName)
        {
            using var connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var afftected = await connection.
                ExecuteAsync("Delete from coupon WHERE ProductName=@ProductName",
                new { ProductName = productName });
            if (afftected == 0)
                return false;
            return true;
        }
    }
}
