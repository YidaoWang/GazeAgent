using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UDP
{
    public class SettingCommand : ICommandData
    {
        public CommandType CommandType => CommandType.Setting;

        public int[] ExperimentOrder { get; set; }
        public int RepeatNumber { get; set; }

        public SettingCommand(int[] experimentOrder, int repeatNumber)
        {
            ExperimentOrder = experimentOrder;
            RepeatNumber = repeatNumber;
        }

        public SettingCommand(byte[] data)
        {
            if (data[0] != (byte)CommandType) return;

            var intArray = new int[(data.Length - 1) / sizeof(int)];

            for(int i = 0; i < intArray.Length; i++)
            {
                intArray[i] = BitConverter.ToInt32(data, 1 + i * sizeof(int));
            }

            RepeatNumber = intArray[0];
            ExperimentOrder = new int[intArray.Length - 1];
            Array.Copy(intArray, 1, ExperimentOrder, 0, ExperimentOrder.Length);
        }

        public byte[] ToBytes()
        {
            var intArray = new int[ExperimentOrder.Length + 1];
            intArray[0] = RepeatNumber;
            Array.Copy(ExperimentOrder, 0, intArray, 1, ExperimentOrder.Length);

            var byteArray = new byte[intArray.Length * sizeof(int) + 1];
            byteArray[0] = (byte)CommandType;

            for (int i = 0; i < intArray.Length; i++)
            {
                Array.Copy(BitConverter.GetBytes(intArray[i]), 0, byteArray, 1 + i * sizeof(int), sizeof(int));
            }



            return byteArray;
        }
    }
}
