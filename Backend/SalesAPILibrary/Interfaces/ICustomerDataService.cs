using SalesAPILibrary.Shared_Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAPILibrary.Interfaces
{
    public interface ICustomerDataService
    {
        Task<IList<Customer>> GetAllCustomers();

        Task<Customer> GetCustomerById(int id);

        Task<CustomerDetails> GetCustomerDetails(int id);

        Task<string> GetAddress(int id);

        Task<bool> IsActiveCustomer(int id);

        Task<Customer> UpdateCustomerDetails(CustomerDetails customerDetails);

        Task<CustomerDetails> UpdateCustomerAddress(int id, string address);

        Task<CustomerDetails> AddCustomer(CustomerDetails customerDetails);

        Task<CustomerDetails> DeleteCustomer(int id);
    }
}
