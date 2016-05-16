using System.Runtime.Serialization;

namespace Khs.Employee.Contract.Model
{
    [DataContract]
    public class Employee
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Email { get; set; }

    }
}
