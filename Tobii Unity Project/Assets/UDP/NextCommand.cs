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
        public int LastExperimentNumber { get; set; }
        public bool Answer
        {
            get
            {
                return _answer != 0;
            }
            set
            {
                if (value)
                {
                    _answer = 1;
                }
                else
                {
                    _answer = 0;
                }
            }
        }
        public string Respondent { get; set; }

        private byte _answer;

        NextCommand(int lastExperimentNumber, bool answer, string respondent)
        {
            LastExperimentNumber = lastExperimentNumber;
            Answer = answer;
            Respondent = respondent;
        }

        NextCommand(byte[] data)
        {
            if (data[0] != (byte)CommandType)
                return;
            _answer = data[1];
            LastExperimentNumber = BitConverter.ToInt32(data, 2);
            Respondent = Encoding.UTF8.GetString(data, 6, data.Length - 6);
        }

        public byte[] ToBytes()
        {
            var intBytes = BitConverter.GetBytes(LastExperimentNumber);
            var utf8 = Encoding.UTF8.GetBytes(Respondent);
            var bytes = new byte[2 + intBytes.Length + utf8.Length];
            bytes[0] = (byte)CommandType;
            bytes[1] = _answer;
            intBytes.CopyTo(bytes, 2);
            utf8.CopyTo(bytes, 6);
            return bytes;
        }
    }
}
