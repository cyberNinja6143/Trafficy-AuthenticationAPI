using AuthenticationAPI.Models.Responses;
using AuthenticationAPI.Models;
using AuthenticationAPI.Services.TokenGenerators;
using AuthenticationAPI.Services.UserRepositories;
using Microsoft.AspNetCore.Mvc;
using AuthenticationAPI.Services.PasswordHashers;
using AuthenticationAPI.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AuthenticationAPI.Services.EmailService;
using AuthenticationAPI.Models.ConfigModels;
using System.Text.RegularExpressions;
using AuthenticationAPI.Services.TokenGenerators.JWTClaimsBinder;
using System.Net;
using AuthenticationAPI.Services.HouseKeeping;

namespace AuthenticationAPI.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly AccessTokenGenerator _accessTokenGenerator;
        public readonly IEmailService _emailService;
        private readonly EmailCred _emailCred;
        private readonly IWebHostEnvironment _env;
        private readonly TokenValidator _tokenValidator;
        private readonly JWTClaimUserBinder _JWTClaimUserBinder;
        private readonly Cleaner _cleaner;

        public AuthenticationController(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            AccessTokenGenerator accessTokenGenerator,
            IEmailService emailService,
            EmailCred emailCred,
            IWebHostEnvironment env,
            TokenValidator tokenValidator,
            JWTClaimUserBinder jWTClaimUserBinder,
            Cleaner cleaner)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _accessTokenGenerator = accessTokenGenerator;
            _emailService = emailService;
            _emailCred = emailCred;
            _env = env;
            _tokenValidator = tokenValidator;
            _JWTClaimUserBinder = jWTClaimUserBinder;
            _cleaner = cleaner;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            /*
             * Checks if the body is formatted correctly
             * Checks if the username is taken
             * If all checks pass the information from the body
             * Is used to create a user object which is signed into a jwt
             * The jwt is used to generate a link which is sent to the users email
             * The client can click on the link to verify their email.
             */
            if (!ModelState.IsValid)
            {
                return BadRequest(new String("883"));
            }

            User existingUserByEmail = await _userRepository.GetByEmail(registerRequest.Email);
            if (existingUserByEmail != null)
            {
                return Conflict(new String("885"));
            }

            //If all tests are passed the email and user creation logic begins
            User registrationUser = new User();
            registrationUser.Username = registerRequest.Username;
            /*
             * The token is created using a blank user with just a guid. The guid is required
             * to confirm the user
             * this guid is changed after the email is verified to protect user privacy
             */
            Guid userGuid = Guid.NewGuid();
            while (await _userRepository.GetByUUID(userGuid) != null)
            {
                userGuid = Guid.NewGuid();
            }
            registrationUser.UUID = userGuid;
            registrationUser.Username = "null";
            registrationUser.Email = "null";
            registrationUser.PasswordHash = "null";
            registrationUser.EmailConfirmed = false;
            string regestrationToken = _accessTokenGenerator.GenerateToken(registrationUser);

            //After the token is generated the user gets properly binded to the post body
            registrationUser.Username = registerRequest.Username;
            registrationUser.Email = registerRequest.Email;
            registrationUser.PasswordHash = _passwordHasher.HashPassword(registerRequest.Password);
            await _userRepository.Create(registrationUser);
            //after this the user is saved to the repository

            //Create email message
            string body = "<html><body><p>EmailConfirmation</p><p>Please Click the link to verify your email <a href='word'>link</a>.</p></body></html>";
            body = Regex.Replace(body, @"\bword\b", $"https://authapi.trafficy.net/ConfirmEmail?token={WebUtility.UrlEncode(regestrationToken)}&tempAuth={WebUtility.UrlEncode(userGuid.ToString())}");
            EmailMessage messageToSend = new EmailMessage();
            messageToSend.From = _emailCred.Username;
            messageToSend.To = registrationUser.Email;
            messageToSend.Body = body;
            messageToSend.Subject = "Confirm Email";

            //Send the email
            Boolean result = await _emailService.SendEmail(messageToSend);
            if (!result)
            {
                return Conflict(new String("500"));
            }
            
            _cleaner.CleanIfEmailDoesNotVerify(registrationUser.UUID);
            return Ok();

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            /*
             * Takes the username from post body
             * Checks if a user by that username exists in the database
             * Takes the password, hashes it using the Bcrypt alogrithm and checks if it
             * matches the database
             * If all checks pass the requester is given an Authentication token that can be used to
             * access services.
             */
            if (!ModelState.IsValid)
            {
                return BadRequest(new String("883"));
            }

            User existingUserByUsername = await _userRepository.GetByEmail(loginRequest.Email);
            if (existingUserByUsername == null)
            {
                return Conflict(new String("588"));
            }

            bool paswordGood = _passwordHasher.VerifyPassword(loginRequest.Password, existingUserByUsername.PasswordHash);
            if (!paswordGood)
            {
                return Unauthorized(new String("800"));
            }

            string accessToken = _accessTokenGenerator.GenerateToken(existingUserByUsername);

            return Ok(new AuthenticatedUserResponse()
            {
                AccessToken = accessToken,
            });
        }

        [Authorize]
        [HttpPost("refreash")]
        public async Task<IActionResult> Refreash()
        {
            /*
             * Takes the UUID from the post header.
             * Parses it into a Guid (Globally unique identifier).
             * Validates that a user with that UUID exists.
             * Generates and sends a new authentication token.
             */
            string id = HttpContext.User.FindFirstValue("UUID");
            try
            {
                Guid UUID = Guid.Parse(id);
                User user = await _userRepository.GetByUUID(UUID);
                if (user == null)
                {
                    return Conflict(new String("588"));
                }
                string accessToken = _accessTokenGenerator.GenerateToken(user);

                return Ok(new AuthenticatedUserResponse()
                {
                    AccessToken = accessToken,
                });
            }
            catch (Exception)
            {
                return BadRequest(new String("883"));
            }
            
        }

        [HttpPost("deleteUser")]
        public async Task<IActionResult> DeleteUser([FromBody] GenericRequest request)
        {
            Guid usertoRemove = Guid.Parse(request.Request);
            bool done = await _userRepository.DeleteById(usertoRemove);
            if (done)
            {
                return Ok();
            }
            return Conflict(new String($"Could not remove {usertoRemove}"));
        }

        [HttpGet("listUsers")]
        public async Task<IActionResult> ListUsers()
        {
            string userLIst = await _userRepository.GetAllUsersAsString();
            return Ok(new String(userLIst));
        }

        //This method is used to confirm the user's email
        [HttpGet("ConfirmEmail")]
         public async Task<IActionResult> ConfirmEmail()
         {
            //Grab elements from query
            var token = HttpContext.Request.Query["token"].ToString();
            var guid = HttpContext.Request.Query["tempAuth"].ToString();
            //Make sure query was made by a trafficy auth server
            Boolean tokenValid = _tokenValidator.isTokenValid(token);
            if (!tokenValid)
             {
                 return BadRequest(new String("883"));
             }
            
            //Get the user and confrim their email
            Guid guidFromQuery = Guid.Parse(guid);
            var userToConfrim = await _userRepository.GetByUUID(guidFromQuery);
            if (userToConfrim == null)
            {
                return BadRequest(new String("883"));
            }
            userToConfrim.EmailConfirmed = true;

            //change the guid for good measure
            await _userRepository.DeleteById(guidFromQuery);
            Guid userGuid = Guid.NewGuid();
            while (await _userRepository.GetByUUID(userGuid) != null)
            {
                userGuid = Guid.NewGuid();
            }
            userToConfrim.UUID = userGuid;
            await _userRepository.Create(userToConfrim);
            return View("Confirmation");
         }

        //This is used to serve web pages
        [HttpGet("Serve/{id}")]
        public ActionResult Serve(string id)
        {
            switch(id)
            {
                case "delottiePlayer":
                    return PhysicalFile(Path.Combine(_env.ContentRootPath, "/app/wwwroot/js/animations/dependencies/dotlottie-player.mjs"), "text/javascript");
                case "ODPU3M3Z":
                    return PhysicalFile(Path.Combine(_env.ContentRootPath, "/app/wwwroot/js/animations/dependencies/chunk-ODPU3M3Z.mjs"), "text/javascript");
                case "TRZ6EGBZ":
                    return PhysicalFile(Path.Combine(_env.ContentRootPath, "/app/wwwroot/js/animations/dependencies/chunk-TRZ6EGBZ.mjs"), "text/javascript");
                case "HDDX7F4A":
                    return PhysicalFile(Path.Combine(_env.ContentRootPath, "/app/wwwroot/js/animations/dependencies/chunk-HDDX7F4A.mjs"), "text/javascript");
                case "ZWH2ESXT":
                    return PhysicalFile(Path.Combine(_env.ContentRootPath, "/app/wwwroot/js/animations/dependencies/chunk-ZWH2ESXT.mjs"), "text/javascript");
                case "MJGYILXD-NRTSROOT":
                    return PhysicalFile(Path.Combine(_env.ContentRootPath, "/app/wwwroot/js/animations/dependencies/lottie_svg-MJGYILXD-NRTSROOT.mjs"), "text/javascript");
                case "veryifyAnimation":
                    return PhysicalFile(Path.Combine(_env.ContentRootPath, "/app/wwwroot/js/animations/verified.json"), "application/json");
                default:
                    return null;
            }
        }
    }
}
