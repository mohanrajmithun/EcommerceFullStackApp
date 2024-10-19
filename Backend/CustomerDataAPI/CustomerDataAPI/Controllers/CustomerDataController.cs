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
        private readonly ICustomerDataService CustomerDataService;
        public CustomerDataController(ICustomerDataService CustomerDataService)
        {

            this.CustomerDataService = CustomerDataService;


        }
        [Authorize(Roles ="User,Admin")]
        [HttpGet("Customers")]

        public async Task<ActionResult<IList<Customer>>> GetAllCustomers()
        {
            try
            {
                var customers = await CustomerDataService.GetAllCustomers();
                if (customers != null)
                {
                    return Ok(customers);

                }
                return NotFound();


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");

            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("customer")]
        public async Task<ActionResult<Customer>> GetcustomerById(int id)
        {
            try
            {
                // Validate the input parameter
                if (id < 1)
                {
                    return BadRequest("customer Id cannot be empty");
                }

                var customer = await CustomerDataService.GetCustomerById(id);
                if (customer == null)
                {
                    return NotFound($"customer with Id '{id}' not found");
                }

                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request");
            }

        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("customer")]
        public async Task<ActionResult<CustomerDetails>> Createcustomer(CustomerDetails customerDetails)
        {
            var created_customer = await CustomerDataService.AddCustomer(customerDetails);
            return CreatedAtAction(nameof(CustomerDataService.GetCustomerById), new { id = created_customer.CustomerId }, created_customer);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPut("updateCustomer")]
        public async Task<ActionResult<CustomerDetails>> UpdateCustomer(CustomerDetails customerDetails)
        {
            try
            {
                if (customerDetails == null)
                {
                    return BadRequest("Customer object cannot be null");
                }

                var customer = await CustomerDataService.UpdateCustomerDetails(customerDetails);

                if (customer == null)
                {
                    return NotFound($"the movie with id:{customer.CustomerId} does not exist");
                }
                return Ok(customer);

            }

            catch (Exception ex)
            {

                return BadRequest(ex.Message);

            }

        }




    }
}

