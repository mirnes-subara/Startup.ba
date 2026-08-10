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
    public class StartupImageService : BaseCRUDService<StartupImageResponse, StartupImageSearchObject, StartupImage, StartupImageUpsertRequest, StartupImageUpsertRequest>, IStartupImageService
    {
        public StartupImageService(StartupbaDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public override async Task<PagedResult<StartupImageResponse>> GetAsync(StartupImageSearchObject search)
        {
            var query = _context.StartupImages
                .Include(si => si.Startup)
                .AsQueryable();

            query = ApplyFilter(query, search);

            int? totalCount = null;
            if (search.IncludeTotalCount)
            {
                totalCount = await query.CountAsync();
            }

            query = ApplyPaging(query, search);

            var list = await query.OrderBy(si => si.DisplayOrder).ThenBy(si => si.CreatedAt).ToListAsync();
            return new PagedResult<StartupImageResponse>
            {
                Items = list.Select(MapToResponse).ToList(),
                TotalCount = totalCount
            };
        }

        protected override IQueryable<StartupImage> ApplyFilter(IQueryable<StartupImage> query, StartupImageSearchObject search)
        {
            if (search.StartupId.HasValue)
            {
                query = query.Where(si => si.StartupId == search.StartupId.Value);
            }

            if (search.IsCover.HasValue)
            {
                query = query.Where(si => si.IsCover == search.IsCover.Value);
            }

            if (search.IsLogo.HasValue)
            {
                query = query.Where(si => si.IsLogo == search.IsLogo.Value);
            }

            if (search.IsActive.HasValue)
            {
                query = query.Where(si => si.IsActive == search.IsActive.Value);
            }

            return query;
        }

        protected StartupImageResponse MapToResponse(StartupImage entity)
        {
            var response = _mapper.Map<StartupImageResponse>(entity);

            if (entity.Startup != null)
            {
                response.StartupName = entity.Startup.Name;
            }

            return response;
        }

        public override async Task<StartupImageResponse?> GetByIdAsync(int id)
        {
            var entity = await _context.StartupImages
                .Include(si => si.Startup)
                .FirstOrDefaultAsync(si => si.Id == id);

            if (entity == null)
                return null;

            return MapToResponse(entity);
        }

        public override async Task<StartupImageResponse> CreateAsync(StartupImageUpsertRequest request)
        {
            var entity = new StartupImage();
            MapInsertToEntity(entity, request);
            entity.CreatedAt = DateTime.UtcNow;

            _context.StartupImages.Add(entity);

            await BeforeInsert(entity, request);

            await _context.SaveChangesAsync();

            // Reload with relationship
            await _context.Entry(entity).Reference(si => si.Startup).LoadAsync();

            return MapToResponse(entity);
        }

        public override async Task<StartupImageResponse?> UpdateAsync(int id, StartupImageUpsertRequest request)
        {
            var entity = await _context.StartupImages.FindAsync(id);
            if (entity == null)
                return null;

            await BeforeUpdate(entity, request);

            MapUpdateToEntity(entity, request);

            await _context.SaveChangesAsync();

            // Reload with relationship
            await _context.Entry(entity).Reference(si => si.Startup).LoadAsync();

            return MapToResponse(entity);
        }

        protected override async Task BeforeInsert(StartupImage entity, StartupImageUpsertRequest request)
        {
            if (!await _context.Startups.AnyAsync(s => s.Id == request.StartupId))
            {
                throw new InvalidOperationException("Startup does not exist.");
            }

            // If setting as cover, unset other covers for this startup
            if (request.IsCover)
            {
                var existingCovers = await _context.StartupImages
                    .Where(si => si.StartupId == request.StartupId && si.IsCover)
                    .ToListAsync();

                foreach (var cover in existingCovers)
                {
                    cover.IsCover = false;
                }
            }

            // If setting as logo, unset other logos for this startup
            if (request.IsLogo)
            {
                var existingLogos = await _context.StartupImages
                    .Where(si => si.StartupId == request.StartupId && si.IsLogo)
                    .ToListAsync();

                foreach (var logo in existingLogos)
                {
                    logo.IsLogo = false;
                }
            }
        }

        protected override async Task BeforeUpdate(StartupImage entity, StartupImageUpsertRequest request)
        {
            if (!await _context.Startups.AnyAsync(s => s.Id == request.StartupId))
            {
                throw new InvalidOperationException("Startup does not exist.");
            }

            // If setting as cover, unset other covers for this startup (excluding current entity)
            if (request.IsCover && !entity.IsCover)
            {
                var existingCovers = await _context.StartupImages
                    .Where(si => si.StartupId == request.StartupId && si.IsCover && si.Id != entity.Id)
                    .ToListAsync();

                foreach (var cover in existingCovers)
                {
                    cover.IsCover = false;
                }
            }

            // If setting as logo, unset other logos for this startup (excluding current entity)
            if (request.IsLogo && !entity.IsLogo)
            {
                var existingLogos = await _context.StartupImages
                    .Where(si => si.StartupId == request.StartupId && si.IsLogo && si.Id != entity.Id)
                    .ToListAsync();

                foreach (var logo in existingLogos)
                {
                    logo.IsLogo = false;
                }
            }
        }
    }
}
