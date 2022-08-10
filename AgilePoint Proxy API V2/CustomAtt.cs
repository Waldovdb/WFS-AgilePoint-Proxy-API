using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgilePoint_Proxy_API_V2
{
    public class CustomAtt
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public CustomAtt()
        {
            
        }

        public CustomAtt(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
