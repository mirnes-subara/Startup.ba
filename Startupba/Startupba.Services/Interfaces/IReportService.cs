using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using System.Threading.Tasks;

namespace Startupba.Services.Interfaces
{
    public interface IReportService : ICRUDService<ReportResponse, ReportSearchObject, ReportUpsertRequest, ReportUpsertRequest>
    {
        /// <summary>
        /// Admin resolves a report (Reviewed / Dismissed / ActionTaken) and
        /// notifies the reporter.
        /// </summary>
        Task<ReportResponse?> ResolveAsync(int id, ReportResolveRequest request);
    }
}
