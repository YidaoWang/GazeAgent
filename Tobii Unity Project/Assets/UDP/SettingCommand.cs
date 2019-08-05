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
                var reader = new BinaryReader(stream, Encoding.UTF8);
                if (reader.ReadByte() != (byte)CommandType)
                    return;
                var number = reader.ReadInt32();
                for (var i = 0; i < number; i++)
                {
                    var type = (ExperimentType)reader.ReadByte();
                    var num = reader.ReadInt32();
                    var imgPath = new string(reader.ReadChars(reader.ReadInt32()));
                    var ca = reader.ReadBoolean();
                    ExperimentList.Add(new Experiment(type, num, imgPath, ca));
                }
            }
        }

        public byte[] ToBytes()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream, Encoding.UTF8);
                writer.Write((byte)CommandType);
                writer.Write(ExperimentList.Count);
                foreach (var e in ExperimentList)
                {
                    writer.Write((byte)e.ExperimentType);
                    writer.Write(e.Number);
                    writer.Write(e.ImageFile.Length);
                    writer.Write(e.ImageFile);
                    writer.Write(e.CorrectAnswer);
                }
                writer.Close();
                return stream.ToArray();
            }
        }
    }
}
