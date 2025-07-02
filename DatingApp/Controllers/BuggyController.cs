using DatingApp.Data;
using DatingApp.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuggyController : ControllerBase
    {
        private readonly DatingDbContext context;
        public BuggyController(DatingDbContext context)
        {
            this.context = context;
        }
        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return "secret text";     
        }
        [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound()
        {
            var thing = context.Users.Find(-1);
            if (thing == null) return NotFound();
            return Ok(thing);
        }
        [HttpGet("server-error")]
        public ActionResult<AppUser> GetServerError()
        {


            var thing = context.Users.Find(-1) ?? throw new Exception("Something went wrong");
            var thingToReturn = thing.ToString();
            return Ok(thingToReturn);
        }
        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest()
        {
            return BadRequest("This was not a good request");
        }
        [HttpGet("test")]
        public ActionResult<string> GetTest()
        {
            return "This is a test endpoint";
        }
    }
}
