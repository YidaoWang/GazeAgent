using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.UDP
{
    public enum CommandType
    {
        Setting = 1,
        Next = 2,
        Responce = 0
    }

    interface ICommandData
    {
        CommandType CommandType { get; }
        byte[] ToBytes();
    }
}
