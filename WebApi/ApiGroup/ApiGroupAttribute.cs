using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.ApiGroup
{
    /// <summary>
    /// 系统分组特性
    /// </summary>
    public class ApiGroupAttribute : Attribute
    {
        public ApiGroupAttribute(params ApiGroupNames[] name)
        {
            GroupName = name;
        }
        public ApiGroupNames[] GroupName { get; set; }
    }
}
