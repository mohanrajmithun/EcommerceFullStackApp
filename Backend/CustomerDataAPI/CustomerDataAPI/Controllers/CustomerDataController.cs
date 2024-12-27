using CustomerDataAPI.Entities;
using CustomerDataAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;

namespace CustomerDataAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerDataController : ControllerBase
    {
        private readonly ICustomerDataService _customerDataService;
        private readonly ILogger<CustomerDataController> _logger;

        public CustomerDataController(ICustomerDataService customerDataService, ILogger<CustomerDataController> logger)
        {
            _customerDataService = customerDataService;
            _logger = logger;
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("Customers")]
        public async Task<ActionResult<IList<Customer>>> GetAllCustomers()
        {
            _logger.LogInformation("Fetching all customers...");
            try
            {
                var customers = await _customerDataService.GetAllCustomers();
                if (customers != null)
                {
                    _logger.LogInformation("Successfully retrieved {CustomerCount} customers", customers.Count);
                    return Ok(customers);
                }

                _logger.LogWarning("No customers found.");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all customers.");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("customer")]
        public async Task<ActionResult<Customer>> GetcustomerById(int id)
        {
            _logger.LogInformation("Fetching customer with ID: {CustomerId}", id);
            try
            {
                if (id < 1)
                {
                    _logger.LogWarning("Invalid customer ID: {CustomerId}", id);
                    return BadRequest("Customer ID cannot be empty");
                }

                var customer = await _customerDataService.GetCustomerById(id);
                if (customer == null)
                {
                    _logger.LogWarning("Customer with ID: {CustomerId} not found", id);
                    return NotFound($"Customer with ID '{id}' not found");
                }

                _logger.LogInformation("Successfully retrieved customer with ID: {CustomerId}", id);
                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching customer with ID: {CustomerId}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("customer")]
        public async Task<ActionResult<CustomerDetails>> Createcustomer(CustomerDetails customerDetails)
        {
            _logger.LogInformation("Creating a new customer...");
            try
            {
                var createdCustomer = await _customerDataService.AddCustomer(customerDetails);
                _logger.LogInformation("Successfully created customer with ID: {CustomerId}", createdCustomer.CustomerId);
                return CreatedAtAction(nameof(_customerDataService.GetCustomerById), new { id = createdCustomer.CustomerId }, createdCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a customer.");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPut("updateCustomer")]
        public async Task<ActionResult<CustomerDetails>> UpdateCustomer(CustomerDetails customerDetails)
        {
            _logger.LogInformation("Updating customer with ID: {CustomerId}", customerDetails?.CustomerId);
            try
            {
                if (customerDetails == null)
                {
                    _logger.LogWarning("Received null customer object for update.");
                    return BadRequest("Customer object cannot be null");
                }

                var updatedCustomer = await _customerDataService.UpdateCustomerDetails(customerDetails);

                if (updatedCustomer == null)
                {
                    _logger.LogWarning("Customer with ID: {CustomerId} does not exist", customerDetails.CustomerId);
                    return NotFound($"The customer with ID: {customerDetails.CustomerId} does not exist");
                }

                _logger.LogInformation("Successfully updated customer with ID: {CustomerId}", updatedCustomer.CustomerId);
                return Ok(updatedCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating customer with ID: {CustomerId}", customerDetails?.CustomerId);
                return BadRequest(ex.Message);
            }
        }
    }
}
