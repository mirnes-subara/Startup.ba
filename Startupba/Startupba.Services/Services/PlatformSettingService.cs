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
    public class PlatformSettingService : BaseCRUDService<PlatformSettingResponse, PlatformSettingSearchObject, PlatformSetting, PlatformSettingUpsertRequest, PlatformSettingUpsertRequest>, IPlatformSettingService
    {
        public PlatformSettingService(StartupbaDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        protected override IQueryable<PlatformSetting> ApplyFilter(IQueryable<PlatformSetting> query, PlatformSettingSearchObject search)
        {
            if (!string.IsNullOrEmpty(search.Key))
            {
                query = query.Where(ps => ps.Key.Contains(search.Key));
            }

            if (!string.IsNullOrEmpty(search.FTS))
            {
                query = query.Where(ps =>
                    ps.Key.Contains(search.FTS) ||
                    ps.Value.Contains(search.FTS) ||
                    (ps.Description != null && ps.Description.Contains(search.FTS)));
            }

            return query.OrderBy(ps => ps.Key);
        }

        public async Task<PlatformSettingResponse?> GetByKeyAsync(string key)
        {
            var entity = await _context.PlatformSettings.FirstOrDefaultAsync(ps => ps.Key == key);
            if (entity == null)
                return null;

            return _mapper.Map<PlatformSettingResponse>(entity);
        }

        protected override async Task BeforeInsert(PlatformSetting entity, PlatformSettingUpsertRequest request)
        {
            if (await _context.PlatformSettings.AnyAsync(ps => ps.Key == request.Key))
            {
                throw new InvalidOperationException("A setting with this key already exists.");
            }
        }

        protected override async Task BeforeUpdate(PlatformSetting entity, PlatformSettingUpsertRequest request)
        {
            if (await _context.PlatformSettings.AnyAsync(ps => ps.Key == request.Key && ps.Id != entity.Id))
            {
                throw new InvalidOperationException("A setting with this key already exists.");
            }
        }

        protected override void MapUpdateToEntity(PlatformSetting entity, PlatformSettingUpsertRequest request)
        {
            base.MapUpdateToEntity(entity, request);
            entity.UpdatedAt = DateTime.Now;
        }
    }
}
