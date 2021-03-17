using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using WebApi.ApiGroup;
using WebApi.Model;
using WebApi.Model.ModelTest;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WeatherForecastController : Controller
    {

        private readonly ILogger<WeatherForecastController> _logger;

        private readonly IHttpContextAccessor _accessor;
        public WeatherForecastController(ILogger<WeatherForecastController> logger, IHttpContextAccessor accessor)
        {
            _logger = logger;
            _accessor = accessor;
        }
        /// <summary>
        /// 多分组共用请求
        /// </summary>
        /// <returns></returns>
        //[ProducesResponseType(201)]
        //[ProducesResponseType(400)]
        [HttpGet("getLoginAndIT")]
        [ApiGroup(ApiGroupNames.Login, ApiGroupNames.It)]
        public IActionResult GetLoginAndIT()
        {
            return Json("GetLoginAndIT ok");
        }

        [HttpGet("getConfig")]
        [ApiGroup(ApiGroupNames.Config)]

        public IActionResult GetConfig()
        {
            return Json("Config ok");
        }


        [HttpGet("getHr")]
        [ApiGroup(ApiGroupNames.Hr)]

        public IActionResult GetHr()
        {
            return Json("Hr ok");
        }

        [HttpGet("getIt")]
        [ApiGroup(ApiGroupNames.It)]
        public IActionResult GetIt()
        {
            return Json("GetIt ok");
        }
        /// <summary>
        /// 获取Miniprofiler Index的 Script (尚未分组的)
        /// </summary>
        /// <returns></returns>
        [HttpGet("getMiniprofilerScript")]
        public IActionResult getMiniprofilerScript()
        {
            var htmlstr = MiniProfiler.Current.RenderIncludes(_accessor.HttpContext);
            var script = htmlstr.Value;
            return Json(script);
        }

        /// <summary>
        /// Mapster测试
        /// </summary>
        /// <returns></returns>
        [HttpGet("getMapsterTest")]

        // 隐藏
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult getMapsterTest()
        {
            var user = new UserInfo() { Id = 1, Name = "cy", Address = "深证保安区" };

            //1.简单直接 Adapt 转换 
            var destObject1 = user.Adapt<UserInfoDto>();

            //2.自定义配置转换
            var config = new TypeAdapterConfig();
            //映射规则
            config.ForType<UserInfo, UserInfoDto>()
                .Map(dest => dest.NameDto, src => src.Name)
                .Map(dest => dest.AddressDto, src => src.Address)
                .NameMatchingStrategy(NameMatchingStrategy.ConvertDestinationMemberName(dest => dest.Replace("User", "")));

            var mapper = new Mapper(config);//务必将mapper设为单实例
            var dto = mapper.Map<UserInfoDto>(user);

            //3.
            //映射规则
            config.ForType<UserInfo, UserInfoDto>()
                .Map(dest => dest.NameDto, src => src.Name)
                .Map(dest => dest.AddressDto, src => src.Address)
                .IgnoreNullValues(true)//忽略空值映射
                .Ignore(dest => dest.NameDto)//忽略指定字段
                .IgnoreAttribute(typeof(DataMemberAttribute))//忽略指定特性的字段
                .NameMatchingStrategy(NameMatchingStrategy.IgnoreCase)//忽略字段名称的大小写
                .IgnoreNonMapped(true);//忽略除以上配置的所有字段

                config.ForType<UserInfo, UserInfoDto>()
                .IgnoreMember((member, side) => !member.Type.Namespace.StartsWith("System"));//实现更细致的忽略规则

            return Json("ok");
        }
    }
}
