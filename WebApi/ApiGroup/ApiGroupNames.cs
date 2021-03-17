using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.ApiGroup
{
    /// <summary>
    /// 系统分组枚举值
    /// </summary>
    public enum ApiGroupNames
    {
        [GroupInfo(Title = "All", Description = "All接口", Version = "")]
        All = 0,
        [GroupInfo(Title = "尚未分组", Description = "尚未分组相关接口", Version = "")]
        NoGroup = 1,
        [GroupInfo(Title = "登录认证", Description = "登录认证相关接口", Version = "")]
        Login = 2,
        [GroupInfo(Title = "IT", Description = "登录认证相关接口", Version = "")]
        It = 3,
        [GroupInfo(Title = "人力资源", Description = "登录认证相关接口", Version = "")]
        Hr = 4,
        [GroupInfo(Title = "系统配置", Description = "系统配置相关接口", Version = "")]
        Config = 5
    }
}
