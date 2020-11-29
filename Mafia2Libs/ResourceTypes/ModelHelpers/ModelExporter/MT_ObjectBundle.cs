using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceTypes.ModelHelpers.ModelExporter
{
    public class MT_ObjectBundle
    {
        private const string FileHeader = "MTB";
        private const int FileVersion = 1;

        public MT_Object[] Objects { get; set; }
    }
}
