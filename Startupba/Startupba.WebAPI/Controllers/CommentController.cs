using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class CommentController : BaseCRUDController<CommentResponse, CommentSearchObject, CommentUpsertRequest, CommentUpsertRequest>
    {
        public CommentController(ICommentService service) : base(service)
        {
        }

        [AllowAnonymous]
        public override async Task<PagedResult<CommentResponse>> Get([FromQuery] CommentSearchObject? search = null)
        {
            return await base.Get(search);
        }

        [AllowAnonymous]
        public override async Task<CommentResponse?> GetById(int id)
        {
            return await base.GetById(id);
        }
    }
}
