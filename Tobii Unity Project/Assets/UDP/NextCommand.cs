using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UDP
{
    public class NextCommand : ICommandData
    {
        public CommandType CommandType => CommandType.Next;
        public int LastExperimentNumber { get; set; }
        public bool Answer { get; set; }
        public string Respondent { get; set; }
        public DateTime NextStartTime { get; set; }

        public NextCommand(int lastExperimentNumber, bool answer, string respondent, DateTime nextStartTime)
        {
            LastExperimentNumber = lastExperimentNumber;
            Answer = answer;
            Respondent = respondent;
            NextStartTime = nextStartTime;
        }

        public NextCommand(byte[] data)
        {
            Debug.Log(LogDisplay.ArrayToString(data));
            using (var stream = new MemoryStream(data))
            {
                var reader = new BinaryReader(stream, Encoding.UTF8);
                if (reader.ReadByte() != (byte)CommandType) return;
                LastExperimentNumber = reader.ReadInt32();
                Answer = reader.ReadBoolean();
                Respondent = reader.ReadString();
                Debug.Log(Respondent);
                NextStartTime = DateTime.FromBinary(reader.ReadInt64());
            }
        }

        public byte[] ToBytes()
        {
            using (var stream = new MemoryStream())
            {             
                var writer = new BinaryWriter(stream, Encoding.UTF8);
                writer.Write((byte)CommandType);
                writer.Write(LastExperimentNumber);
                writer.Write(Answer);
                writer.Write(Respondent);
                writer.Write(NextStartTime.ToBinary());
                writer.Close();
                var d = stream.ToArray();
                //Debug.Log(LogDisplay.ArrayToString(d));
                return d;
            }
        }
    }
}
