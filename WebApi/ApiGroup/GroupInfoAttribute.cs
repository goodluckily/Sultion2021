using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.ApiGroup
{
    public class GroupInfoAttribute : Attribute
    {
        public string Title { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
    }
}
