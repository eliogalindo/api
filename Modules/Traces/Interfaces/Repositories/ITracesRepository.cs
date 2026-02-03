using api.Core.Interfaces.Repositories;
using api.Modules.Traces.Models;

namespace api.Modules.Traces.Interfaces.Repositories;

public interface ITracesRepository : IRepository<Trace>
{
}