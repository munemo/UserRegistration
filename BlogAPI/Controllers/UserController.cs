using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace BlogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private static List<User> users = new List<User>
            {


        };
        public readonly DataContext _context;
        public UserController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id); 
                

            if (user == null)
                return BadRequest("Resource not found");

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<List<User>>> AddUser(User user)
        {
           _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var NewUser  = await _context.Users.ToListAsync();
            return Ok(NewUser);
        }


        [HttpPost("register")]
        public async Task<ActionResult> Register(UserRegisterRequest request)
        {
            if(_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest("Please try another email or password");
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            var user = new User
            {   Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                VerificationToken = CreateRandomToken(),
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User successfully registered!");
        }

        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(
                    System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(
                    System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }


        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLoginRequest request)
        {
          var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return BadRequest("User not found.");

      
            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Password Incorrect.");
            }

            if (user.VerifiedAt == null)
                return BadRequest("User not verified.");

            return Ok($"Welcome back {user.Email} !");

        }


        [HttpPost("verify")]
        public async Task<ActionResult> Verify(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            if (user == null)
                return BadRequest("Invalid token.");
            user.VerifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return Ok("User verified!");

        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> Forgot(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
           
            if (user == null)
                return BadRequest("Resource not found.");
           
            user.PasswordResetToken = CreateRandomToken();
            user.ResetTokenExpires = DateTime.Now.AddDays(1);
            await _context.SaveChangesAsync();
         
        
            return Ok("You may now reset your password.");

        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);

            if (user == null || user.ResetTokenExpires < DateTime.Now)
                return BadRequest("Invalid Token.");

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordSalt = passwordSalt;
            user.PasswordHash = passwordHash;
            user.ResetTokenExpires = null;
            user.ResetTokenExpires = null;
            await _context.SaveChangesAsync();


            return Ok("Password succcessfully reset.");

        }


       

         [HttpPut]
          public async Task<ActionResult<List<User>>> UpdateUser(User request)
          {
              var user = await _context.Users.FindAsync(request.Id);

              if (user == null)
                  return BadRequest("User not found");
              user.Email = request.Email;
            //  user. = request.Password;

              await _context.SaveChangesAsync();

              var updatedUser = await _context.Users.ToListAsync();
              return Ok(updatedUser);
          }

        [HttpDelete("{id}")]
        public async Task<ActionResult<List<User>>> DeleteUser(int id)
        {

           User user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
           
            if (user == null)
                return BadRequest("User not found");
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok($"User {user.Email} deleted");
        }
    }
}
