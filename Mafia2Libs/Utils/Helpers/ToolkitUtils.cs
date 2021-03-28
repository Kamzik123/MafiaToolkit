using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Helpers
{
    public class ToolkitUtils
    {
        public static bool IsOfType(object ObjectInQuestion, Type TypeToCheck)
        {
            bool bIsOfType = (ObjectInQuestion.GetType() == TypeToCheck);
            return bIsOfType;
        }
    }
}
