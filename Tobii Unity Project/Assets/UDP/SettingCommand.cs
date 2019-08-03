using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UDP
{
    public class SettingCommand : ICommandData
    {
        public CommandType CommandType => CommandType.Setting;

        public List<Experiment> ExperimentList { get; set; }

        public SettingCommand(List<Experiment> experimentList)
        {
            ExperimentList = experimentList;
        }

        public SettingCommand(byte[] data)
        {
            ExperimentList = new List<Experiment>(); 
            using (var stream = new MemoryStream(data))
            {
                var reader = new BinaryReader(stream);
                if (reader.ReadByte() != (byte)CommandType)
                    return;
                var number = reader.Read();
                for(var i = 0; i < number; i++)
                {
                    var type = (ExperimentType)reader.ReadByte();
                    var num = reader.Read();
                    var bytelength = reader.Read();
                    var imgPath = Encoding.UTF8.GetString(reader.ReadBytes(bytelength));
                    var ca = reader.ReadBoolean();
                    ExperimentList.Add(new Experiment(type, num, imgPath, ca));
                }
            }
        }

        public byte[] ToBytes()
        {
            var byteList = new List<byte>();
            byteList.Add((byte)CommandType);
            var number = BitConverter.GetBytes(ExperimentList.Count);
            byteList.AddRange(number);
            foreach(var e in ExperimentList)
            {
                var utf8 = Encoding.UTF8.GetBytes(e.ImageFile);
                byteList.Add((byte)e.ExperimentType);
                byteList.AddRange(BitConverter.GetBytes(e.Number));
                byteList.AddRange(BitConverter.GetBytes(utf8.Length));
                byteList.AddRange(utf8);
                byteList.AddRange(BitConverter.GetBytes(e.CorrectAnswer));
            }
            return byteList.ToArray();
        }
    }
}
