using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Helpers;
using WebApi.Model.ModelApi;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly AppSettingsConfig  _appSettingsConfig;
        public LoginController(IOptions<AppSettingsConfig> appSettingsConfig)
        {
            _appSettingsConfig = appSettingsConfig.Value;
        }
        /// <summary>
        /// 登陆获取JWT字符串
        /// </summary>
        /// <returns></returns>
        [HttpGet("login")]
        public IActionResult Login() 
        {
            //1.判断账号密码是否正确

            //2.登录成功进行jwt加密
            TokenModelJwt tokenModel = new TokenModelJwt { UserId = Guid.NewGuid(), Level = "11" };
            var jwtStr = JwtHelper.JwtEncrypt(tokenModel);
            return Ok(jwtStr);
        }

        /// <summary>
        /// 解密JWT 获取用户信息 注意带上 截取前面的Bearer和空格
        /// </summary>
        /// <returns></returns>
        [HttpPost("jwtDecryptForUser")]
        public IActionResult JwtDecryptForUser(string jwtStr) 
        {
            var aa = _appSettingsConfig;
            var bb = aa as AppSettingsConfig;
           


            return Ok(JwtHelper.JwtDecrypt(jwtStr));
        }
    }
}
