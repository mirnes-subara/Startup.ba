using System;
using System.Collections.Generic;
using System.Text;

namespace Startupba.Model.SearchObjects
{
    public class RoleSearchObject : BaseSearchObject
    {
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
    }
} 