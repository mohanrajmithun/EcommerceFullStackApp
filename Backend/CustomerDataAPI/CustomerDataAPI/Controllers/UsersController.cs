using CustomerDataAPI.Entities;
using CustomerDataAPI.Infrastructure;
using CustomerDataAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CustomerDataAPI.Enums;
using SerilogTimings;
using SalesAPILibrary.Interfaces;
using SalesAPILibrary.Shared_Entities;
using ApplicationUser = CustomerDataAPI.Entities.ApplicationUser;

namespace CustomerDataAPI.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly AppDbContext _context;
        private readonly ILogger<UsersController> _logger;
        private readonly ICustomerDataService _customerDataService;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            TokenService tokenService,
            AppDbContext context,
            ILogger<UsersController> logger,
            ICustomerDataService customerDataService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
            _logger = logger;
            _customerDataService = customerDataService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult> Register(RegistrationRequest request)
        {
            _logger.LogInformation("Registration process started for email: {Email}", request.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state during registration for email: {Email}", request.Email);
                return BadRequest(ModelState);
            }

            ApplicationUser newUser = new ApplicationUser()
            {
                UserName = request.Email,
                Email = request.Email,
                Role = Role.User
            };

            using (Operation.Time("Creating the user in Database"))
            {
                try
                {
                    var result = await _userManager.CreateAsync(newUser, request.password!);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created successfully with email: {Email}", request.Email);

                        CustomerDetails customerDetails = new CustomerDetails
                        {
                            Email = request.Email,
                            Address = request.Address,
                            CustomerName = request.FullName,
                            PhoneNo = request.Phone_Number,
                            IsActive = true
                        };

                        var customer = await _customerDataService.AddCustomer(customerDetails);

                        newUser.CustomerId = customer.CustomerId;
                        newUser.PhoneNumber = request.Phone_Number.ToString();

                        var updateUserResult = await _userManager.UpdateAsync(newUser);

                        if (!updateUserResult.Succeeded)
                        {
                            _logger.LogWarning("Failed to update user with CustomerId for email: {Email}", request.Email);
                            return BadRequest("Failed to update user with CustomerId.");
                        }

                        _logger.LogInformation("User updated with CustomerId: {CustomerId} for email: {Email}", customer.CustomerId, request.Email);
                        request.password = ""; // Clear password for safety
                        return CreatedAtAction(nameof(Register), new { email = request.Email, role = request.Role }, request);
                    }

                    foreach (var error in result.Errors)
                    {
                        _logger.LogWarning("Error during user creation: {Error}", error.Description);
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while registering user with email: {Email}", request.Email);
                    throw;
                }
            }

            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Authenticate(AuthRequest request)
        {
            _logger.LogInformation("Login process started for email: {Email}", request.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state during login for email: {Email}", request.Email);
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email!);
                if (user == null)
                {
                    _logger.LogWarning("Bad credentials for email: {Email}", request.Email);
                    return BadRequest(new { message = "Bad Credentials" });
                }

                var validPassword = await _userManager.CheckPasswordAsync(user, request.Password!);
                if (!validPassword)
                {
                    _logger.LogWarning("Invalid password for email: {Email}", request.Email);
                    return BadRequest(new { message = "Bad Credentials" });
                }

                var userInDb = _context.Users.FirstOrDefault(u => u.Email == request.Email);
                if (userInDb is null)
                {
                    _logger.LogWarning("Unauthorized access attempt for email: {Email}", request.Email);
                    return Unauthorized();
                }

                var accessToken = _tokenService.CreateToken(userInDb);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User authenticated successfully for email: {Email}", request.Email);

                if (userInDb.CustomerId == null)
                {
                    return Ok(new AuthResponse
                    {
                        Email = userInDb.Email,
                        Token = accessToken,
                        Username = userInDb.UserName,
                    });
                }

                return Ok(new AuthResponse
                {
                    Email = userInDb.Email,
                    Token = accessToken,
                    Username = userInDb.UserName,
                    CustomerId = (int)userInDb.CustomerId,
                    Role = userInDb.Role.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for email: {Email}", request.Email);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}
