using System.Text;

namespace Security.CustomEncoding
{
    public class Base16Encoder : IEncoder
    {
        private const string Alphabet = "ACDFGHJKMNRTUVWX";

        public string GetString(byte[] data)
        {
            var result = new StringBuilder();

            foreach (var b in data)
            {
                result.Append(Alphabet[(b & 0xF0) >> 4]);
                result.Append(Alphabet[b & 0x0F]);
            }

            return result.ToString();
        }

        public byte[] GetBytes(string data)
        {
            var result = new byte[data.Length / 2];

            var i = 0;

            foreach (var c in data)
            {
                var index = Alphabet.IndexOf(c);

                result[i / 2] |= i % 2 != 1
                                     ? (byte) (index << 4)
                                     : (byte) index;

                i++;
            }

            return result;
        }
    }
}