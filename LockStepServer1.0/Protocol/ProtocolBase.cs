using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockStepServer1._0.Protocol
{
    public class ProtocolBase
    {
        public virtual ProtocolBase Decode(byte[] readbuff, int start, int lenght)
        {
            return new ProtocolBase();
        }
        public virtual byte[] Encode()
        {
            return new byte[] { };
        }
        public virtual string GetName()
        {
            return "";
        }
        public virtual string GetDesc()
        {
            return "";
        }
    }
}
