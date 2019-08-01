using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.UDP
{
    public class NextCommand : ICommandData
    {
        public CommandType CommandType => CommandType.Next;
        

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }


    }
}
