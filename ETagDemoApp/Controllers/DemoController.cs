using AutomatedETags.AutomatedETags;
using AutomatedETags;
using Microsoft.AspNetCore.Mvc;

namespace ETagDemoApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DemoController : ControllerBase
    {
        private static string MyContent = "Ana are mere.";
        private static int Version = 1;

        
        [HttpGet("strong")]
        [EnableETag]
        public IActionResult GetStrong()
        {
            return Ok(new { Message = MyContent });
        }

        [HttpGet("ignore-timestamp-with-weak-etags")]
        [WeakETag]
        public IActionResult GetSemantic()
        {
            HttpContext.Items["CustomETagValue"] = Version.ToString();

            return Ok(new
            {
                Content = MyContent,
                Version = Version,
                ServerTime = DateTime.Now // Normally this would break caching
            });
        }

        // skip etag
        [HttpGet("skip")]
        [SkipETag]
        public IActionResult GetSkip()
        {
            return Ok(new { Message = "This endpoint bypasses your middleware entirely." });
        }

        [HttpPost("update")]
        public IActionResult Update([FromBody] string newContent)
        {
            if (!ETagValidator.IsPreconditionValidWithHash(HttpContext, Version.ToString()))
            {
                // Someone else edited the data! Block the save.
                return StatusCode(412, new { Error = "Conflict! The data was modified by someone else." });
            }

            // Safe to save
            MyContent = newContent;
            Version++;

            return Ok(new { Message = "Saved successfully!", NewVersion = Version });
        }
    }
}
