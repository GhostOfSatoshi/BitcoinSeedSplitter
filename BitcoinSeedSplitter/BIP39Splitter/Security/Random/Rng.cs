using System.Security.Cryptography;

namespace Security.Random
{
    public class Rng : IRng
    {
        private readonly RandomNumberGenerator _rng;

        public Rng()
        {
            _rng = new RNGCryptoServiceProvider();
        }

        public void GetBytes(byte[] bytes)
        {
            _rng.GetBytes(bytes);
        }
    }
}