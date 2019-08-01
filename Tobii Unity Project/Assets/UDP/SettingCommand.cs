using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.UDP
{
    public class SettingCommand : ICommandData
    {
        public CommandType CommandType => CommandType.Setting;

        public static int[] ExperimentOrder { get; set; }
        public static int RepeatNumber { get; set; }

        public SettingCommand(int[] experimentOrder, int repeatNumber)
        {
            ExperimentOrder = experimentOrder;
            RepeatNumber = repeatNumber;
        }

        public SettingCommand(byte[] data)
        {
            if (data[0] != (byte)CommandType) return;

            var intArray = new int[(data.Length - 1) / sizeof(int)];
            Buffer.BlockCopy(data, 1, intArray, 0, data.Length - 1);

            RepeatNumber = intArray[0];
            Array.Copy(intArray, 1, ExperimentOrder, 0, ExperimentOrder.Length);
        }

        public byte[] ToBytes()
        {
            var intArray = new float[ExperimentOrder.Length + 1];
            intArray[0] = RepeatNumber;
            Array.Copy(ExperimentOrder, 0, intArray, 1, ExperimentOrder.Length);

            var byteArray = new byte[intArray.Length * sizeof(int) + 1];
            byteArray[0] = (byte)CommandType;
            Buffer.BlockCopy(intArray, 0, byteArray, 1, byteArray.Length - 1);

            return byteArray;
        }
    }
}
