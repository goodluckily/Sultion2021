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

            //1.����̬������ӳ��
            //configuration.GetSection("Logging").Bind(MySettings.Setting);

            JwtHelper.GetConfiguration(_configuration);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();

            //������ӳ�䵽������ȥ
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
                    Name = "��̫��",
                    Email = string.Empty,
                    Url = new Uri("https://www.cnblogs.com/goodluckily/")
                },
                License = new OpenApiLicense
                {
                    Name = "���֤����",
                    Url = new Uri("https://www.cnblogs.com/goodluckily/")
                }
            };

            services.AddSwaggerGen(c =>
            {
                //c.AddServer(new OpenApiServer() { Url = "http://localhost:5000", Description = "��ַ1" });
                //c.AddServer(new OpenApiServer() { Url = "http://127.0.0.1:5001", Description = "��ַ2" });
                //c.AddServer(new OpenApiServer() { Url = "http://192.168.28.213:5002", Description = "��ַ3" });

                #region ���鷽����

                //����ApiGroupNames����ö��ֵ���ɽӿ��ĵ���Skip(1)����ΪEnum��һ��FieldInfo�����õ�һ��Intֵ
                typeof(ApiGroupNames).GetFields().Skip(1).ToList().ForEach(f =>
                {
                    //��ȡö��ֵ�ϵ�����
                    var info = f.GetCustomAttributes(typeof(GroupInfoAttribute), false).OfType<GroupInfoAttribute>().FirstOrDefault();
                    openApiInfo.Title = info?.Title;
                    openApiInfo.Version = info?.Version;
                    openApiInfo.Description = info?.Description;
                    c.SwaggerDoc(f.Name, openApiInfo);
                });

                //�жϽӿڹ����ĸ�����
                c.DocInclusionPredicate((docName, apiDescription) =>
                {
                    if (!apiDescription.TryGetMethodInfo(out MethodInfo method)) return false;
                    //1.ȫ���ӿ�
                    if (docName == "All") return true;
                    //�����õ����������������µ�ֵ
                    var actionlist = apiDescription.ActionDescriptor.EndpointMetadata.FirstOrDefault(x => x is ApiGroupAttribute);
                    //2.�õ���δ����Ľӿ�***************
                    if (docName == "NoGroup") return actionlist == null ? true : false;
                    //3.���ض�Ӧ�Ѿ��ֺ���Ľӿ�
                    if (actionlist != null)
                    {
                        //�ж��Ƿ�����������
                        var actionfilter = actionlist as ApiGroupAttribute;
                        return actionfilter.GroupName.Any(x => x.ToString().Trim() == docName);
                    }
                    return false;
                });

                #endregion

              
                ////Filter ��Ҫ��װ Swashbuckle.AspNetCore.Filters
                //c.OperationFilter<AddResponseHeadersFilter>();
                //c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                //c.OperationFilter<SecurityRequirementsOperationFilter>();

                //JWT ���÷���
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "���Ƿ�ʽ��(JWT��Ȩ(���ݽ�������ͷ�н��д���) ֱ�����¿�������Bearer {token}��ע������֮����һ���ո�)",
                    Name = "Authorization",//jwtĬ�ϵĲ�������
                    In = ParameterLocation.Header,//jwtĬ�ϴ��Authorization��Ϣ��λ��(����ͷ��)
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
                    //��ȡ������Ҫʹ�õ�Microsoft.IdentityModel.Tokens.SecurityKey����ǩ����֤��
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                    //��ȡ������һ��System.String������ʾ��ʹ�õ���Ч�����߼����ҵķ����ߡ�
                    ValidIssuer = issuer,
                    //��ȡ������һ���ַ��������ַ�����ʾ�����ڼ�����Ч���ڷ������ƵĹ��ڡ�
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

                #region ���鷽����
                //����ApiGroupNames����ö��ֵ���ɽӿ��ĵ���Skip(1)����ΪEnum��һ��FieldInfo�����õ�һ��Intֵ
                typeof(ApiGroupNames).GetFields().Skip(1).ToList().ForEach(f =>
                {
                    //��ȡö��ֵ�ϵ�����
                    var info = f.GetCustomAttributes(typeof(GroupInfoAttribute), false).OfType<GroupInfoAttribute>().FirstOrDefault();
                    c.SwaggerEndpoint($"/swagger/{f.Name}/swagger.json", info != null ? info.Title : f.Name);
                });
                #endregion

                //swagger Ĭ���۵�
                //c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);

                //MiniProfiler�õ�
                c.IndexStream = () => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("WebApi.index.html");
            });

            #endregion




            //�����֤�м��
            app.UseAuthentication();

            app.UseRouting();

            //�����Ȩ�м��
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
