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
    public class StartupStatusService : BaseCRUDService<StartupStatusResponse, StartupStatusSearchObject, StartupStatus, StartupStatusUpsertRequest, StartupStatusUpsertRequest>, IStartupStatusService
    {
        public StartupStatusService(StartupbaDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        protected override IQueryable<StartupStatus> ApplyFilter(IQueryable<StartupStatus> query, StartupStatusSearchObject search)
        {
            if (!string.IsNullOrEmpty(search.Name))
            {
                query = query.Where(s => s.Name.Contains(search.Name));
            }

            if (search.IsActive.HasValue)
            {
                query = query.Where(s => s.IsActive == search.IsActive.Value);
            }

            return query.OrderBy(s => s.Id);
        }

        protected override async Task BeforeInsert(StartupStatus entity, StartupStatusUpsertRequest request)
        {
            if (await _context.StartupStatuses.AnyAsync(s => s.Name == request.Name))
            {
                throw new InvalidOperationException("A startup status with this name already exists.");
            }
        }

        protected override async Task BeforeUpdate(StartupStatus entity, StartupStatusUpsertRequest request)
        {
            if (await _context.StartupStatuses.AnyAsync(s => s.Name == request.Name && s.Id != entity.Id))
            {
                throw new InvalidOperationException("A startup status with this name already exists.");
            }
        }

        protected override async Task BeforeDelete(StartupStatus entity)
        {
            if (await _context.Startups.AnyAsync(s => s.StatusId == entity.Id))
            {
                throw new InvalidOperationException("Cannot delete a status that is used by startups.");
            }
        }
    }
}
