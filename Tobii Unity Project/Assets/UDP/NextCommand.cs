using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.UDP
{
    public class NextCommand : ICommandData
    {
        public CommandType CommandType => CommandType.Next;
        public int LastExperimentNumber { get; set; }
        public bool Answer { get; set; }
        public string Respondent { get; set; }
        public DateTime NextStartTime { get; set; }

        NextCommand(int lastExperimentNumber, bool answer, string respondent)
        {
            LastExperimentNumber = lastExperimentNumber;
            Answer = answer;
            Respondent = respondent;
        }

        NextCommand(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var reader = new BinaryReader(stream, Encoding.UTF8);
                if (reader.ReadByte() != (byte)CommandType) return;
                LastExperimentNumber = reader.ReadInt32();
                Answer = reader.ReadBoolean();
                var length = reader.ReadInt32();
                Respondent = new string(reader.ReadChars(length));
                NextStartTime = new DateTime(reader.ReadInt64());
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
                writer.Write(Respondent.Length);
                writer.Write(Respondent);
                writer.Write(NextStartTime.ToBinary());
                return stream.ToArray();
            }
        }
    }
}
