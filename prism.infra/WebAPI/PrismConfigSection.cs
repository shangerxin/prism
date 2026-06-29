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
        [ConfigurationProperty("PrismBinaryRoot", IsRequired = true)]
        public PrismBinaryRoot PrismBinaryRoot
        {
            get { return (PrismBinaryRoot)this["PrismBinaryRoot"]; }
            set { this["PrismBinaryRoot"] = value; }
        }
        [ConfigurationProperty("CondaBinary", IsRequired = true)]
        public CondaBinary CondaBinary
        {
            get { return (CondaBinary)this["CondaBinary"]; }
            set { this["CondaBinary"] = value; }
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

    public class PrismBinaryRoot: ConfigurationElement
    {
        [ConfigurationProperty("Path", IsRequired = true)]
        public string Path
        {
            get { return (string)this["Path"]; }
            set { this["Path"] = value; }
        }
    }

    public class CondaBinary: ConfigurationElement
    {
        [ConfigurationProperty("Path", IsRequired = true)]
        public string Path
        {
            get { return (string)this["Path"]; }
            set { this["Path"] = value; }
        }
    }
}
