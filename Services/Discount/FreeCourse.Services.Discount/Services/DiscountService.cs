using Dapper;
using FreeCourse.Shared.Dtos;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.Services.Discount.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly IConfiguration configuration;
        private readonly IDbConnection dbConnection;

        public DiscountService(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.dbConnection = new NpgsqlConnection(configuration.GetConnectionString("PostgreSql"));
        }

        public async Task<Response<NoContent>> Delete(int id)
        {
            var deleteStatus = await dbConnection.
                ExecuteAsync("DELETE FROM discount WHERE id=@Id", new { Id = id });

            return deleteStatus > 0 ?
                Response<NoContent>.Success(204) :
                Response<NoContent>.Fail("Discount not found", 404);
        }

        public async Task<Response<List<Models.Discount>>> GetAll()
        {
            var discounts = await dbConnection.QueryAsync<Models.Discount>("SELECT * FROM discount");

            return Response<List<Models.Discount>>.Success(discounts.ToList(), 200);
        }

        public async Task<Response<Models.Discount>> GetByCodeAndUserId(string code, string userId)
        {
            var discounts = await dbConnection.
                QueryAsync<Models.Discount>("SELECT * FROM discount WHERE userid = @UserId AND code = @Code",
                new { UserId = userId, Code = code });

            var hasDiscount = discounts.FirstOrDefault();

            if (hasDiscount == null)
                return Response<Models.Discount>.Fail("Discount not found", 404);

            return Response<Models.Discount>.Success(hasDiscount, 200);
        }

        public async Task<Response<Models.Discount>> GetById(int id)
        {
            var discount = (await dbConnection.QueryAsync<Models.Discount>("SELECT * FROM discount WHERE id = @Id",
                new { Id = id })).SingleOrDefault();

            if (discount == null)
            {
                return Response<Models.Discount>.Fail("Discount not found", 404);
            }

            return Response<Models.Discount>.Success(discount, 200);
        }

        public async Task<Response<NoContent>> Save(Models.Discount entity)
        {
            var saveStatus = await dbConnection.
                ExecuteAsync("INSERT INTO discount(userid, rate, code) VALUES(@UserId, @Rate, @Code)", entity);

            if (saveStatus > 0)
            {
                return Response<NoContent>.Success(204);
            }

            return Response<NoContent>.Fail("An error occurred while adding", 500);
        }

        public async Task<Response<NoContent>> Update(Models.Discount entity)
        {
            var updateStatus = await dbConnection.
                ExecuteAsync("UPDATE discount SET userid=@UserId, code=@Code, rate=@Rate, WHERE id=@Id",
                new
                {
                    Id = entity.Id,
                    UserId = entity.UserId,
                    Code = entity.Code,
                    Rate = entity.Rate
                });

            if (updateStatus > 0)
            {
                return Response<NoContent>.Success(204);
            }

            return Response<NoContent>.Fail("Discount not found", 404);
        }
    }
}
