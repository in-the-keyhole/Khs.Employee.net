using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Khs.Employee.Contract
{
    public interface IEmployeeRepository : IService
    {
        Task<Model.Employee> Get(long id);
        Task<IEnumerable<Model.Employee>> GetAll();
        Task<Model.Employee> Save(Model.Employee employee);
        Task Remove(long id);
    }
}
