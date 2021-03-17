using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebApi.ApiGroup;
using WebApi.Helpers;
using WebApi.Model.ModelApi;

namespace WebApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;

            //1.往静态类里面映射
            //configuration.GetSection("Logging").Bind(MySettings.Setting);

            JwtHelper.GetConfiguration(_configuration);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();

            //将配置映射到类里面去
            //private readonly Logging _config;
            //public HomeController(IOptions<Logging> config)
            //{
            //    _config = config.Value;
            //}
            services.Configure<AppSettingsConfig>(_configuration);


            #region MiniProfiler
            //   /profiler/results
            services.AddMiniProfiler(options =>
                    options.RouteBasePath = "/profiler"
                );
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            #endregion

            #region swagger

            var openApiInfo = new OpenApiInfo
            {
                Version = "v1",
                Title = "WebApi",
                Description = "A simple example ASP.NET Core Web API",
                TermsOfService = new Uri("https://www.cnblogs.com/goodluckily/"),
                Contact = new OpenApiContact
                {
                    Name = "雨太阳",
                    Email = string.Empty,
                    Url = new Uri("https://www.cnblogs.com/goodluckily/")
                },
                License = new OpenApiLicense
                {
                    Name = "许可证名字",
                    Url = new Uri("https://www.cnblogs.com/goodluckily/")
                }
            };

            services.AddSwaggerGen(c =>
            {
                //c.AddServer(new OpenApiServer() { Url = "http://localhost:5000", Description = "地址1" });
                //c.AddServer(new OpenApiServer() { Url = "http://127.0.0.1:5001", Description = "地址2" });
                //c.AddServer(new OpenApiServer() { Url = "http://192.168.28.213:5002", Description = "地址3" });

                #region 分组方案二

                //遍历ApiGroupNames所有枚举值生成接口文档，Skip(1)是因为Enum第一个FieldInfo是内置的一个Int值
                typeof(ApiGroupNames).GetFields().Skip(1).ToList().ForEach(f =>
                {
                    //获取枚举值上的特性
                    var info = f.GetCustomAttributes(typeof(GroupInfoAttribute), false).OfType<GroupInfoAttribute>().FirstOrDefault();
                    openApiInfo.Title = info?.Title;
                    openApiInfo.Version = info?.Version;
                    openApiInfo.Description = info?.Description;
                    c.SwaggerDoc(f.Name, openApiInfo);
                });

                //判断接口归于哪个分组
                c.DocInclusionPredicate((docName, apiDescription) =>
                {
                    if (!apiDescription.TryGetMethodInfo(out MethodInfo method)) return false;
                    //1.全部接口
                    if (docName == "All") return true;
                    //反射拿到控制器分组特性下的值
                    var actionlist = apiDescription.ActionDescriptor.EndpointMetadata.FirstOrDefault(x => x is ApiGroupAttribute);
                    //2.得到尚未分组的接口***************
                    if (docName == "NoGroup") return actionlist == null ? true : false;
                    //3.加载对应已经分好组的接口
                    if (actionlist != null)
                    {
                        //判断是否包含这个分组
                        var actionfilter = actionlist as ApiGroupAttribute;
                        return actionfilter.GroupName.Any(x => x.ToString().Trim() == docName);
                    }
                    return false;
                });

                #endregion

              
                ////Filter 需要安装 Swashbuckle.AspNetCore.Filters
                //c.OperationFilter<AddResponseHeadersFilter>();
                //c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                //c.OperationFilter<SecurityRequirementsOperationFilter>();

                //JWT 配置方法
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "这是方式二(JWT授权(数据将在请求头中进行传输) 直接在下框中输入Bearer {token}（注意两者之间是一个空格）)",
                    Name = "Authorization",//jwt默认的参数名称
                    In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, true);

            });
            #endregion


            #region JWT

            var secret = _configuration["JwtTokenManagement:secret"];
            var issuer = _configuration["JwtTokenManagement:issuer"];
            var audience = _configuration["JwtTokenManagement:audience"];

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                //Token Validation Parameters
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    //获取或设置要使用的Microsoft.IdentityModel.Tokens.SecurityKey用于签名验证。
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                    //获取或设置一个System.String，它表示将使用的有效发行者检查代币的发行者。
                    ValidIssuer = issuer,
                    //获取或设置一个字符串，该字符串表示将用于检查的有效受众反对令牌的观众。
                    ValidAudience = audience,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
            });
            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiniProfiler();

            #region Swagger

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger";

                #region 分组方案二
                //遍历ApiGroupNames所有枚举值生成接口文档，Skip(1)是因为Enum第一个FieldInfo是内置的一个Int值
                typeof(ApiGroupNames).GetFields().Skip(1).ToList().ForEach(f =>
                {
                    //获取枚举值上的特性
                    var info = f.GetCustomAttributes(typeof(GroupInfoAttribute), false).OfType<GroupInfoAttribute>().FirstOrDefault();
                    c.SwaggerEndpoint($"/swagger/{f.Name}/swagger.json", info != null ? info.Title : f.Name);
                });
                #endregion

                //swagger 默认折叠
                //c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);

                //MiniProfiler用的
                c.IndexStream = () => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("WebApi.index.html");
            });

            #endregion




            //添加认证中间件
            app.UseAuthentication();

            app.UseRouting();

            //添加授权中间件
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
