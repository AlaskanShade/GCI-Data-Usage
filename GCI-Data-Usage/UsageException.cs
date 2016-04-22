using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataUsage
{
    public class UsageException : Exception
    {
        public UsageException(string message) : base(message)
        {

        }
    }
}
