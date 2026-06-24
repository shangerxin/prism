using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace prism.infra.WebAPI
{
    public class PrismWebAPIConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("DashboardSettings", IsRequired = true)]
        public DashboardSettings DashboardSettings
        {
            get { return (DashboardSettings)this["DashboardSettings"]; }
            set { this["DashboardSettings"] = value; }
        }
    }

    public class DashboardSettings : ConfigurationElement
    {
        [ConfigurationProperty("DefaultTakeCount", IsRequired = true, DefaultValue = 50)]
        public int DefaultTakeCount
        {
            get { return (int)this["DefaultTakeCount"]; }
            set { this["DefaultTakeCount"] = value; }
        }
    }
}
