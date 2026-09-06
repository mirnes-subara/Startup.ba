using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class StartupImageController : BaseCRUDController<StartupImageResponse, StartupImageSearchObject, StartupImageUpsertRequest, StartupImageUpsertRequest>
    {
        public StartupImageController(IStartupImageService service) : base(service)
        {
        }

        /// <summary>
        /// Raw JPEG/PNG bytes for list thumbnails and Image.network. Public so Flutter
        /// does not need to attach a bearer token to image URLs.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{id}/file")]
        public async Task<IActionResult> GetFile(int id)
        {
            var file = await ((IStartupImageService)_crudService).GetFileAsync(id);
            if (file == null)
                return NotFound();

            return File(file.Value.Data, file.Value.ContentType);
        }
    }
}
