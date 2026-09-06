using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Startupba.WebAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class CommentController : BaseCRUDController<CommentResponse, CommentSearchObject, CommentUpsertRequest, CommentUpsertRequest>
    {
        public CommentController(ICommentService service) : base(service)
        {
        }

        [HttpPost]
        public override async Task<CommentResponse> Create([FromBody] CommentUpsertRequest request)
        {
            request.UserId = this.RequireUserId();
            return await _crudService.CreateAsync(request);
        }
    }
}
