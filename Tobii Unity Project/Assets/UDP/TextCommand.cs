using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.UDP
{
    public class TextCommand : ICommandData
    {
        public CommandType CommandType => CommandType.Text;

        public string Text { get; set; }

        public TextCommand(string text)
        {
            Text = text;
        }

        public TextCommand(byte[] data)
        {
            if (data[0] != (byte)CommandType)
                return;
            Text = Encoding.UTF8.GetString(data, 1, data.Length - 1);
        }

        public byte[] ToBytes()
        {
            var utf8 = Encoding.UTF8.GetBytes(Text);
            var bytes = new byte[1 + utf8.Length];
            bytes[0] = (byte)CommandType;
            utf8.CopyTo(bytes, 1);
            return bytes;
        }
    }
}
