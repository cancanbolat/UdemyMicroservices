using AutoMapper;
using FreeCourse.Services.Catalog.Dtos;
using FreeCourse.Services.Catalog.Models;
using FreeCourse.Services.Catalog.Settings;
using FreeCourse.Shared.Dtos;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreeCourse.Services.Catalog.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IMongoCollection<Category> categoryCollection;
        private readonly IMapper mapper;

        public CategoryService(IMapper mapper, IDatabaseSettings databaseSettings)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(databaseSettings.DatabaseName);
            categoryCollection = database.GetCollection<Category>(databaseSettings.CategoryCollectionName);

            this.mapper = mapper;
        }

        public async Task<Response<List<CategoryDto>>> GetAllAsync()
        {
            var categories = await categoryCollection.Find(x => true).ToListAsync();
            var result = mapper.Map<List<CategoryDto>>(categories);

            return Response<List<CategoryDto>>.Success(result, 200);
        }

        public async Task<Response<CategoryDto>> CreateAsync(CategoryDto categoryDto)
        {
            var category = mapper.Map<Category>(categoryDto);
            await categoryCollection.InsertOneAsync(category);

            return Response<CategoryDto>.Success(mapper.Map<CategoryDto>(category), 200);
        }

        public async Task<Response<CategoryDto>> GetByIdAsync(string id)
        {
            var category = await categoryCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            
            if (category == null)
            {
                return Response<CategoryDto>.Fail("Category not found", 404);
            }

            return Response<CategoryDto>.Success(mapper.Map<CategoryDto>(category), 200);
        }
    }
}
