using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Khs.Employee.Contract;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace Khs.Employee.Service
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class Service : StatefulService , IEmployeeRepository
    {
        public const string CacheName = "employeeRepo";
        public Service(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<Contract.Model.Employee> Get(long id)
        {
            using (var tx = StateManager.CreateTransaction())
            {
                var employees = await StateManager.GetOrAddAsync<IReliableDictionary<long, Contract.Model.Employee>>(CacheName);
                var employee = await employees.TryGetValueAsync(tx, id);
                return employee.Value;
            }
        }

        public async Task<IEnumerable<Contract.Model.Employee>> GetAll()
        {
            using (var tx = StateManager.CreateTransaction())
            {
                var projects = await StateManager.GetOrAddAsync<IReliableDictionary<long, Contract.Model.Employee>>(CacheName);

                var e = await projects.CreateEnumerableAsync(tx);
                var result = new List<Contract.Model.Employee>();
                using (var asyncEnumerator = e.GetAsyncEnumerator())
                {
                    while (await asyncEnumerator.MoveNextAsync(CancellationToken.None))
                    {
                        result.Add(asyncEnumerator.Current.Value);
                    }
                }
                return result;

            }
        }

        public async Task Remove(long id)
        {
            using (var tx = StateManager.CreateTransaction())
            {
                var projects = await StateManager.GetOrAddAsync<IReliableDictionary<long, Contract.Model.Employee>>(CacheName);
                await projects.TryRemoveAsync(tx, id);
                await tx.CommitAsync();
            }
        }

        public async Task<Contract.Model.Employee> Save(Contract.Model.Employee employee)
        {
            using (var tx = StateManager.CreateTransaction())
            {
                var employees = await StateManager.GetOrAddAsync<IReliableDictionary<long, Contract.Model.Employee>>(CacheName);
                if (employee.Id <= 0)
                {
                    employee.Id = await employees.GetCountAsync(tx) + 1;
                }
                await employees.AddOrUpdateAsync(tx, employee.Id, employee, (key, value) => employee);
                await tx.CommitAsync();
            }

            return employee;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see http://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] {
                new ServiceReplicaListener(context =>
                    this.CreateServiceRemotingListener(context))
            };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
