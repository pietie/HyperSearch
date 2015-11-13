using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HscLib.INI
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class IniHeaderAttribute : Attribute
    {
        public string Name { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class IniFieldAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
