using prism.infra.Enum;
using prism.infra.WebAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prism.model.Model.WebAPI
{
    public class QueryBackupModel: QueryModelBase
    {
        public string Name { get; set; }
        public BackupItemTypes BackupItemType { get; set; }
        public string Path { get; set; }
        public string ConnectionString { get; set; }
    }
}
