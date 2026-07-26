using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Database;
using Startupba.Services.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Startupba.Services.Services
{
    public class CategoryService : BaseCRUDService<CategoryResponse, CategorySearchObject, Category, CategoryUpsertRequest, CategoryUpsertRequest>, ICategoryService
    {
        public CategoryService(StartupbaDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public override async Task<PagedResult<CategoryResponse>> GetAsync(CategorySearchObject search)
        {
            var query = _context.Categories.Include(c => c.Startups).AsQueryable();
            query = ApplyFilter(query, search);

            int? totalCount = null;
            if (search.IncludeTotalCount)
            {
                totalCount = await query.CountAsync();
            }

            if (!search.RetrieveAll)
            {
                if (search.Page.HasValue)
                {
                    query = query.Skip(search.Page.Value * search.PageSize.Value);
                }
                if (search.PageSize.HasValue)
                {
                    query = query.Take(search.PageSize.Value);
                }
            }

            var list = await query.ToListAsync();
            return new PagedResult<CategoryResponse>
            {
                Items = list.Select(MapToResponse).ToList(),
                TotalCount = totalCount
            };
        }

        protected override IQueryable<Category> ApplyFilter(IQueryable<Category> query, CategorySearchObject search)
        {
            if (!string.IsNullOrEmpty(search.Name))
            {
                query = query.Where(c => c.Name.Contains(search.Name));
            }

            if (!string.IsNullOrEmpty(search.FTS))
            {
                query = query.Where(c =>
                    c.Name.Contains(search.FTS) ||
                    (c.Description != null && c.Description.Contains(search.FTS)));
            }

            if (search.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == search.IsActive.Value);
            }

            return query.OrderBy(c => c.Name);
        }

        public override async Task<CategoryResponse?> GetByIdAsync(int id)
        {
            var entity = await _context.Categories
                .Include(c => c.Startups)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (entity == null)
                return null;

            return MapToResponse(entity);
        }

        protected CategoryResponse MapToResponse(Category entity)
        {
            var response = _mapper.Map<CategoryResponse>(entity);
            response.StartupCount = entity.Startups?.Count ?? 0;
            return response;
        }

        protected override async Task BeforeInsert(Category entity, CategoryUpsertRequest request)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == request.Name))
            {
                throw new InvalidOperationException("A category with this name already exists.");
            }
        }

        protected override async Task BeforeUpdate(Category entity, CategoryUpsertRequest request)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == request.Name && c.Id != entity.Id))
            {
                throw new InvalidOperationException("A category with this name already exists.");
            }
        }

        protected override async Task BeforeDelete(Category entity)
        {
            if (await _context.Startups.AnyAsync(s => s.CategoryId == entity.Id))
            {
                throw new InvalidOperationException("Cannot delete a category that has startups. Deactivate it instead.");
            }
        }
    }
}
