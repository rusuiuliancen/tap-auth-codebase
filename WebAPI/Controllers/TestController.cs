using BusinessLayer.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ITestService _testService;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;

        public TestController(ITestService testService, 
            IMemoryCache memoryCache, IConfiguration configuration)
        {
            _testService = testService;
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Get()
        {
            var modelCount = _testService.CountRecords();
            if (modelCount == 0)
            {
                return StatusCode(356, "0 RECORDS FOUND.");
            }
            return Ok(modelCount);
        }

        [HttpGet("get-with-error")]
        public IActionResult GetWithError()
        {
            try
            {
                var modelCount = _testService.CountRecords();
                if (modelCount == 0)
                {
                    throw new ArgumentException("0 records.");
                }

                return Ok(modelCount);
            }
            catch (ArgumentNullException ex)
            {
                return StatusCode(400, "Bad null request.");
            }
            catch (ArgumentException ex)
            {
                return StatusCode(400, "Bad request.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }

        [HttpGet("get-from-cache")]
        public IActionResult GetFromCache()
        {
            var itemKey = "my_resource_key";

            if (_memoryCache.TryGetValue(itemKey, out var itemValue))
            {
                return Ok(itemValue);
            } else
            {
                var value = DateTime.Now.ToString();
                var cacheOptions = new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
                };

                _memoryCache.Set(itemKey, value, cacheOptions);
                return Ok(value);
            }
        }

        //[HttpGet("get-with-error")]
        //public IActionResult GetWithError()
        //{
        //    try
        //    {
        //        var modelCount = _testService.CountRecords();
        //        if (modelCount == 0)
        //        {
        //            throw new ArgumentException("0 records.");
        //        }

        //        return Ok(modelCount);
        //    }
        //    catch (ArgumentNullException ex)
        //    {
        //        return StatusCode(500, "Internal Server Error");
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return StatusCode(400, ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "Internal Server Error");
        //    }
        //}

        //[HttpGet("test-cache")]
        //public IActionResult TestCache()
        //{
        //    var cacheKey = "my_resource_key";

        //    if (_memoryCache.TryGetValue(cacheKey, out var cacheItemValue))
        //    {
        //        return Ok(cacheItemValue);
        //    }
        //    else
        //    {
        //        var datenowValue = DateTime.Now.ToString();
        //        var cacheEntryOptions = new MemoryCacheEntryOptions()
        //        {
        //            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
        //        };

        //        _memoryCache.Set(cacheKey, datenowValue, cacheEntryOptions);
        //        return Ok(datenowValue);
        //    }
        //}

        [HttpGet("token")]
        public IActionResult GetToken()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "JJ"),
                new Claim(ClaimTypes.Role, "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: creds);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}
