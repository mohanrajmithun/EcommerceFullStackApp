using SalesAPILibrary.Shared_Entities;
using SalesAPILibrary.Interfaces;
using System;

using Microsoft.EntityFrameworkCore;
using CustomerDataAPI.Infrastructure;
using System.Xml.Linq;

namespace CustomerDataAPI.Services
{
    public class CustomerDataService : ICustomerDataService
    {
        private readonly ILogger<CustomerDataService> logger;
        private readonly AppDbContext appDbContext;


        public CustomerDataService(AppDbContext appDbContext, ILogger<CustomerDataService> logger)
        {
            this.logger = logger;
            this.appDbContext = appDbContext;

        }


        public async Task<IList<Customer>> GetAllCustomers()
        {
            logger.LogInformation("Fetching all customers ...");

            IList<Customer> customers = await appDbContext.Customers.ToListAsync();
            return customers;

        }


        public async Task<Customer> GetCustomerById(int id)
        {
            logger.LogInformation($"Fetching customer with id {id}...");

            Customer customer = await appDbContext.Customers.SingleOrDefaultAsync(customer => customer.CustomerId == id);

            if (customer == null)
            {
                return null;
            }

            return customer;
        }

        public async Task<CustomerDetails> GetCustomerDetails(int id)
        {
            logger.LogInformation($"Fetching customer details with id {id}...");

            Customer customer = await appDbContext.Customers.SingleOrDefaultAsync(customer => customer.CustomerId == id);


            if (customer != null)
            {
                CustomerDetails customerDetails = new CustomerDetails
                {
                    CustomerId = customer.CustomerId,
                    CustomerName = customer.CustomerName,
                    Address = customer.Address,
                    Email = customer.Email,
                    PhoneNo = customer.PhoneNo,
                    IsActive = customer.IsActive
                };

                return customerDetails;

            }

            return null;

        }

        public async Task<string> GetAddress(int id)
        {
            logger.LogInformation($"Fetching customer address with id {id}...");

            Customer customer = await appDbContext.Customers.SingleOrDefaultAsync(customer => customer.CustomerId == id);
            if (customer != null)
            {
                return customer.Address;


            }

            return null;

        }

        public async Task<bool> IsActiveCustomer(int id)
        {
            logger.LogInformation($"Fetching customer with id {id}...");

            Customer customer = await appDbContext.Customers.SingleOrDefaultAsync(customer => customer.CustomerId == id);
            if (customer != null)
            {
                return customer.IsActive;


            }


            return false;
        }

        public async Task<Customer> UpdateCustomerDetails(CustomerDetails customerDetails)
        {
            logger.LogInformation($"updating customer details with id {customerDetails.CustomerId}...");


            Customer customer = await appDbContext.Customers.SingleOrDefaultAsync(customer => customer.CustomerId == customerDetails.CustomerId);

            if (customer != null)
            {
                customer.CustomerName = customerDetails.CustomerName;
                customer.Address = customerDetails.Address;
                customer.Email = customerDetails.Email;
                customer.PhoneNo = customerDetails.PhoneNo;
                customer.IsActive = customerDetails.IsActive;
                customer.UpdateDate = DateTime.UtcNow;

                await appDbContext.SaveChangesAsync();

                return customer;


            }

            return null;



        }

        public async Task<CustomerDetails> UpdateCustomerAddress(int id, string address)
        {
            logger.LogInformation($"updating customer address with id {id}...");


            Customer customer = await appDbContext.Customers.SingleOrDefaultAsync(customer => customer.CustomerId == id);

            if (customer != null)
            {
                customer.Address = address;

                await appDbContext.SaveChangesAsync();
            }

            return null;

        }

        public async Task<CustomerDetails> AddCustomer(CustomerDetails customerDetails)
        {
            logger.LogInformation("Creating a new customer");

            Customer customer = new Customer()
            {
                CustomerName = customerDetails.CustomerName,
                Address = customerDetails.Address,
                Email = customerDetails.Email,
                PhoneNo = customerDetails.PhoneNo,
                IsActive = customerDetails.IsActive,
                UpdateDate = DateTime.UtcNow,
                CreateDate = DateTime.UtcNow


            };

            var result = await appDbContext.Customers.AddAsync(customer);

            await appDbContext.SaveChangesAsync();

            CustomerDetails customerDetails1 = new CustomerDetails()
            {
                CustomerName = result.Entity.CustomerName,
                Address = result.Entity.Address,
                Email = result.Entity.Email,
                PhoneNo = result.Entity.PhoneNo,
                IsActive = result.Entity.IsActive,
                CustomerId = result.Entity.CustomerId,
            };

            return customerDetails1;

        }

        public async Task<CustomerDetails> DeleteCustomer(int id)
        {
            logger.LogInformation("Deleting a  customer");

            Customer customer = await appDbContext.Customers.SingleOrDefaultAsync(customer => customer.CustomerId == id);

            CustomerDetails Deletedcustomer = new CustomerDetails()
            {
                CustomerName = customer.CustomerName,
                Address = customer.Address,
                Email = customer.Email,
                PhoneNo = customer.PhoneNo
            };

            appDbContext.Customers.Remove(customer);
            var result = await appDbContext.SaveChangesAsync();

            if (result > 0)
            {
                return Deletedcustomer;
            }

            return null;
        }











    }
}
