using Startupba.Services.Database;
using System.Collections.Generic;
using System.Threading.Tasks;
using Startupba.Model.Responses;
using Startupba.Model.Requests;
using Startupba.Model.SearchObjects;

namespace Startupba.Services.Interfaces
{
    public interface IService<T, TSearch> where T : class where TSearch : BaseSearchObject
    {
        Task<PagedResult<T>> GetAsync(TSearch search);
        Task<T?> GetByIdAsync(int id);
    }
}