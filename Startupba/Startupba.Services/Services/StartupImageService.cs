using Startupba.Model;
using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Database;
using Startupba.Services.Helpers;
using Startupba.Services.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Startupba.Services.Services
{
    public class StartupImageService : BaseCRUDService<StartupImageResponse, StartupImageSearchObject, StartupImage, StartupImageUpsertRequest, StartupImageUpsertRequest>, IStartupImageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StartupImageService(StartupbaDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
            : base(context, mapper)
        {
            _httpContextAccessor = httpContextAccessor;
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

            await _context.Entry(entity).Reference(si => si.Startup).LoadAsync();

            return MapToResponse(entity);
        }

        protected override async Task BeforeInsert(StartupImage entity, StartupImageUpsertRequest request)
        {
            ImageMagicBytes.EnsureJpegOrPng(request.ImageData);
            await EnsureCanManageStartupAsync(request.StartupId);

            await EnforceSingleCoverAndLogoAsync(request.StartupId, exceptImageId: null, request.IsCover, request.IsLogo);
        }

        protected override async Task BeforeUpdate(StartupImage entity, StartupImageUpsertRequest request)
        {
            ImageMagicBytes.EnsureJpegOrPng(request.ImageData);
            await EnsureCanManageStartupAsync(entity.StartupId);
            if (request.StartupId != entity.StartupId)
                await EnsureCanManageStartupAsync(request.StartupId);

            // Always enforce on the TARGET startup so moving a cover/logo cannot leave two of either.
            await EnforceSingleCoverAndLogoAsync(request.StartupId, entity.Id, request.IsCover, request.IsLogo);
        }

        protected override async Task BeforeDelete(StartupImage entity)
        {
            await EnsureCanManageStartupAsync(entity.StartupId);
        }

        private async Task EnsureCanManageStartupAsync(int startupId)
        {
            var startup = await _context.Startups.FirstOrDefaultAsync(s => s.Id == startupId);
            if (startup == null)
                throw new UserException("Startup does not exist.");

            _httpContextAccessor.EnsureOwnerOrAdmin(startup.FounderId, "You can only manage images for your own startups.");
        }

        /// <summary>
        /// At most one cover and one logo per startup. Applied to the target StartupId.
        /// </summary>
        private async Task EnforceSingleCoverAndLogoAsync(int startupId, int? exceptImageId, bool isCover, bool isLogo)
        {
            if (isCover)
            {
                var existingCovers = await _context.StartupImages
                    .Where(si => si.StartupId == startupId && si.IsCover && (!exceptImageId.HasValue || si.Id != exceptImageId.Value))
                    .ToListAsync();

                foreach (var cover in existingCovers)
                    cover.IsCover = false;
            }

            if (isLogo)
            {
                var existingLogos = await _context.StartupImages
                    .Where(si => si.StartupId == startupId && si.IsLogo && (!exceptImageId.HasValue || si.Id != exceptImageId.Value))
                    .ToListAsync();

                foreach (var logo in existingLogos)
                    logo.IsLogo = false;
            }
        }

        public async Task<(byte[] Data, string ContentType)?> GetFileAsync(int id)
        {
            var entity = await _context.StartupImages
                .AsNoTracking()
                .FirstOrDefaultAsync(si => si.Id == id && si.IsActive);

            if (entity == null || entity.ImageData == null || entity.ImageData.Length == 0)
                return null;

            return (entity.ImageData, ImageMagicBytes.ContentType(entity.ImageData));
        }
    }
}
