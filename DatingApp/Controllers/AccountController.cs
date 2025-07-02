using AutoMapper;
using DatingApp.Data;
using DatingApp.Dtos;
using DatingApp.Entities;
using DatingApp.Helpers;
using DatingApp.Interfaces;
using DatingApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DatingApp.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(UserManager<AppUser > userManger,
        ITokenService tokenService, IMapper mapper,IEmailService emailService ) : ControllerBase
    {
       

        [HttpPost("register")]
 
        public async Task<ActionResult<UserDto>> Register(RgisterDto rgisterDto)
        {
            if (await UserExists(rgisterDto.Username)) return BadRequest("Username is taken");
            using var hmac = new HMACSHA512();
            var user = mapper.Map<AppUser>(rgisterDto);
            user.UserName = rgisterDto.Username.ToLower();
            if (await userManger.FindByEmailAsync(rgisterDto.Email) != null)
                return BadRequest("Email is already taken");
            user.Email = rgisterDto.Email;
            //user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rgisterDto.Password));
            //user.PasswordSalt = hmac.Key;


            //context.Users.Add(user);
            //await context.SaveChangesAsync();
            var result = await userManger.CreateAsync(user, rgisterDto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return new UserDto
            {
                Username = user.UserName,
                Token = await tokenService.CreateToken(user),
                Gender=user.Gender,
                KnownAs=user.KnownAs
            };
          
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await userManger.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.NormalizedUserName == loginDto.Username.ToUpper());
            if (user == null || user.UserName==null) return Unauthorized("Invalid username");
            var result = await userManger.CheckPasswordAsync(user, loginDto.Password);
            if (!result) return Unauthorized("Invalid password");
            //using var hmac = new HMACSHA512(user.PasswordSalt);
            //var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            //for (int i = 0; i < computedHash.Length; i++)
            //{
            //    if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            //}
            return new UserDto 
            {
                Username = user.UserName,
                KnownAs=user.KnownAs,
                Token = await tokenService.CreateToken(user),
                Gender=user.Gender,
                PhotoUrl=user.Photos.FirstOrDefault(x=>x.IsMain)?.Url
            };
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var user = await userManger.FindByEmailAsync(dto.Email);
            if (user == null) return Ok("If this email exists, a reset link has been sent.");

            var token = await userManger.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"{Request.Scheme}://{Request.Host}/reset-password?email={dto.Email}&token={Uri.EscapeDataString(token)}";

            var htmlMessage = $@"
        <div style='font-family:Arial, sans-serif; padding:20px; background-color:#f9f9f9; color:#333;'>
            <h2 style='color:#007bff;'>🔐 Reset Your Password</h2>
            <p>Hi <strong>{user.UserName}</strong>,</p>
            <p>You recently requested to reset your password for your <strong>Dating App</strong> account. Click the button below to reset it:</p>
            <p style='text-align:center;'>
                <a href='{resetLink}' 
                   style='display:inline-block; padding:12px 20px; background-color:#007bff; color:#fff; text-decoration:none; border-radius:5px;'>
                   🔁 Reset Password
                </a>
            </p>
            <p>If you did not request a password reset, you can safely ignore this email.</p>
            <hr />
            <p style='font-size:12px; color:#888;'>This link will expire in a short time for security reasons.</p>
        </div>";

            await emailService.SendEmailAsync(dto.Email, "Reset Your Password", htmlMessage);

            return Ok("If this email exists, a reset link has been sent.");
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var user = await userManger.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest("User not found");

            var decodedToken = Uri.UnescapeDataString(dto.Token);
            var result = await userManger.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password has been reset successfully.");
        }
        private async Task<bool> UserExists(string username)
        {
            return await userManger.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper());
        }
    }
}
