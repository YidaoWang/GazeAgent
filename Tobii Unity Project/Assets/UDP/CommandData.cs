using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.UDP
{
    public class CommandData : IMediaData
    {
        public MediaCondition MediaCondition => MediaCondition.Other;

        public byte[] ToBytes()
        {
            throw new NotImplementedException();
        }
    }
}
