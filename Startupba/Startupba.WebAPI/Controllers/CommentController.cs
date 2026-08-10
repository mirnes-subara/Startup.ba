using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;

namespace Startupba.WebAPI.Controllers
{
    public class CommentController : BaseCRUDController<CommentResponse, CommentSearchObject, CommentUpsertRequest, CommentUpsertRequest>
    {
        public CommentController(ICommentService service) : base(service)
        {
        }
    }
}
