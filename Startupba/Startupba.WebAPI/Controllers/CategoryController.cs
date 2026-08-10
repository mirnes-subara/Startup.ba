using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class CategoryController : BaseCRUDController<CategoryResponse, CategorySearchObject, CategoryUpsertRequest, CategoryUpsertRequest>
    {
        public CategoryController(ICategoryService service) : base(service)
        {
        }

        /// <summary>
        /// Non-admins only see active categories; admins see all for management.
        /// </summary>
        public override async Task<PagedResult<CategoryResponse>> Get([FromQuery] CategorySearchObject? search = null)
        {
            search ??= new CategorySearchObject();
            if (!User.IsInRole("Administrator"))
            {
                search.IsActive = true;
            }

            return await base.Get(search);
        }

        [Authorize(Roles = "Administrator")]
        public override async Task<CategoryResponse> Create([FromBody] CategoryUpsertRequest request)
        {
            return await base.Create(request);
        }

        [Authorize(Roles = "Administrator")]
        public override async Task<CategoryResponse?> Update(int id, [FromBody] CategoryUpsertRequest request)
        {
            return await base.Update(id, request);
        }

        [Authorize(Roles = "Administrator")]
        public override async Task<bool> Delete(int id)
        {
            return await base.Delete(id);
        }
    }
}
