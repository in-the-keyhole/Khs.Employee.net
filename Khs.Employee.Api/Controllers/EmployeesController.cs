using System;
using System.Threading.Tasks;
using System.Web.Http;
using Khs.Employee.Contract;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace Khs.Employee.Api.Controllers
{
    public class EmployeesController : ApiController
    {
        private readonly IEmployeeRepository _repo;

        public EmployeesController()
        {
            var uri = new Uri("fabric:/Khs.Employee/Service");
            _repo = ServiceProxy.Create<IEmployeeRepository>(uri, new ServicePartitionKey(0));
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> Get()
        {
            try
            {
                var results = await _repo.GetAll();
                return Ok(results);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IHttpActionResult> GetById(long id)
        {
            try
            {
                var results = await _repo.Get(id);
                return Ok(results);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody] Contract.Model.Employee employee)
        {
            try
            {
                var result = await _repo.Save(employee);
                return Ok(result);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IHttpActionResult> Put(long id, [FromBody] Contract.Model.Employee employee)
        {
            try
            {
                employee.Id = id;
                var result = await _repo.Save(employee);
                return Ok(result);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IHttpActionResult> Delete(long id)
        {
            try
            {
                await _repo.Remove(id);
                return Ok();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}
