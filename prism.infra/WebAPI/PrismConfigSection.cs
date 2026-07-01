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
        [ConfigurationProperty("BackupRoot", IsRequired = true)]
        public BackupRoot BackupRoot
        {
            get { return (BackupRoot)this["BackupRoot"]; }
            set { this["BackupRoot"] = value; }
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

    public class BackupRoot : ConfigurationElement
    {
        [ConfigurationProperty("Path", IsRequired = true)]
        public string Path
        {
            get { return (string)this["Path"]; }
            set { this["Path"] = value; }
        }


        //Add configuration properties base on the enum BackupItemTypes.
        [ConfigurationProperty("SqlServerDB", IsRequired = true)]
        public string SqlServerDB
        {
            get { return (string)this["SqlServerDB"]; }
            set { this["SqlServerDB"] = value; }
        }

        [ConfigurationProperty("MongoDB", IsRequired = true)]
        public string MongoDB
        {
            get { return (string)this["MongoDB"]; }
            set { this["MongoDB"] = value; }
        }
    }
}
