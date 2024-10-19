using CustomerDataAPI.Entities;
using CustomerDataAPI.Infrastructure;
using CustomerDataAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CustomerDataAPI.Enums;
using SerilogTimings;
using Serilog.AspNetCore;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using ApplicationUser = CustomerDataAPI.Entities.ApplicationUser;


namespace CustomerDataAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly TokenService tokenService;
        private readonly AppDbContext context;
        private readonly ILogger<UsersController> logger;
        private readonly ICustomerDataService CustomerDataService;


        public UsersController(UserManager<ApplicationUser> userManager, TokenService tokenService, AppDbContext context, ILogger<UsersController> logger, ICustomerDataService CustomerDataService)
        {
            this.userManager = userManager;
            this.tokenService = tokenService;
            this.context = context;
            this.logger = logger;
            this.CustomerDataService = CustomerDataService;



        }
        [HttpPost("Register")]
        public async Task<ActionResult> Register(RegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

        

                ApplicationUser new_user = new ApplicationUser()
                {
                    UserName = request.Email,
                    Email = request.Email,
                    Role = Role.User
                };
            


            using (Operation.Time("Creating the user in Database"))
            {
                var result = await userManager.CreateAsync(new_user, request.password!);

                if (result.Succeeded)
                { 

                    CustomerDetails customerDetails = new CustomerDetails();
                    customerDetails.Email = request.Email;
                    customerDetails.Address = request.Address;
                    customerDetails.CustomerName = request.FullName;
                    customerDetails.PhoneNo = request.Phone_Number;
                    customerDetails.IsActive = true;

                    CustomerDetails customer = await CustomerDataService.AddCustomer(customerDetails);

                    new_user.CustomerId = customer.CustomerId;
                    new_user.PhoneNumber = request.Phone_Number.ToString();

                    var updateUserResult = await userManager.UpdateAsync(new_user);
                    if (!updateUserResult.Succeeded)
                    {
                        return BadRequest("Failed to update user with CustomerId.");
                    }

                    request.password = "";
                    return CreatedAtAction(nameof(Register), new { email = request.Email, role = request.Role }, request);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

            }



            return BadRequest(ModelState);



        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Authenticate(AuthRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await userManager.FindByEmailAsync(request.Email!);
            if (user == null)
            {
                return BadRequest(new { message = "Bad Credentials" });
            }


            var valid_password = await userManager.CheckPasswordAsync(user, request.Password!);
            if (!valid_password)
            {
                return BadRequest(new { message = "Bad Credentials" });
            }

            var user_in_db = context.Users.FirstOrDefault(u => u.Email == request.Email);

            if (user_in_db is null)
            {
                return Unauthorized();
            }

            var access_token = tokenService.CreateToken(user_in_db);
            await context.SaveChangesAsync();

            if (user_in_db.CustomerId == null)
            {
                return Ok(new AuthResponse()
                {
                    Email = user_in_db.Email,
                    Token = access_token,
                    Username = user_in_db.UserName,

                });

            }

            return Ok(new AuthResponse()
            {
                Email = user_in_db.Email,
                Token = access_token,
                Username = user_in_db.UserName,
                CustomerId = (int)user_in_db.CustomerId,
                Role = user_in_db.Role.ToString()

            });
        }





    }
}

