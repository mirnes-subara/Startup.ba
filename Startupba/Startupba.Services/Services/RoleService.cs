using Startupba.Model;
using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Database;
using Startupba.Services.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Startupba.Services.Services
{
    public class RoleService : BaseCRUDService<RoleResponse, RoleSearchObject, Role, RoleUpsertRequest, RoleUpsertRequest>, IRoleService
    {
        /// <summary>Fixed seed roles used by authorization. Must not be renamed or deleted.</summary>
        private static readonly HashSet<string> SystemRoleNames =
            new(StringComparer.OrdinalIgnoreCase) { "Administrator", "User" };

        private static readonly HashSet<int> SystemRoleIds = new() { 1, 2 };

        public RoleService(StartupbaDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        protected override IQueryable<Role> ApplyFilter(IQueryable<Role> query, RoleSearchObject search)
        {
            if (!string.IsNullOrEmpty(search.Name))
            {
                query = query.Where(r => r.Name.Contains(search.Name));
            }

            if (!string.IsNullOrEmpty(search.FTS))
            {
                query = query.Where(r => r.Name.Contains(search.FTS) || r.Description.Contains(search.FTS));
            }

            if (search.IsActive.HasValue)
            {
                query = query.Where(r => r.IsActive == search.IsActive.Value);
            }

            return query;
        }

        protected override async Task BeforeInsert(Role entity, RoleUpsertRequest request)
        {
            if (SystemRoleNames.Contains(request.Name))
            {
                throw new UserException("Cannot create a role that conflicts with a system role name.");
            }

            if (await _context.Roles.AnyAsync(r => r.Name == request.Name))
            {
                throw new UserException("A role with this name already exists.");
            }
        }

        protected override async Task BeforeUpdate(Role entity, RoleUpsertRequest request)
        {
            if (IsSystemRole(entity))
            {
                if (!string.Equals(entity.Name, request.Name, StringComparison.OrdinalIgnoreCase))
                {
                    throw new UserException("System roles (Administrator, User) cannot be renamed.");
                }
            }
            else if (SystemRoleNames.Contains(request.Name))
            {
                throw new UserException("Cannot rename a role to a reserved system role name.");
            }

            if (await _context.Roles.AnyAsync(r => r.Name == request.Name && r.Id != entity.Id))
            {
                throw new UserException("A role with this name already exists.");
            }
        }

        protected override Task BeforeDelete(Role entity)
        {
            if (IsSystemRole(entity))
            {
                throw new UserException("System roles (Administrator, User) cannot be deleted.");
            }

            return Task.CompletedTask;
        }

        private static bool IsSystemRole(Role entity)
        {
            return SystemRoleIds.Contains(entity.Id) || SystemRoleNames.Contains(entity.Name);
        }
    }
}
