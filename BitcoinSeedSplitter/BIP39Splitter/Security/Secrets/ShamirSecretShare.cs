using System;
using System.Collections.Generic;
using System.Linq;
using Security.Random;

namespace Security.Secrets
{
    public class ShamirSecretShare : IShamirSecretShare
    {
        private readonly IGf256 _gf256;
        private readonly IRng _rng;

        public ShamirSecretShare(IGf256 gf256, IRng rng)
        {
            _gf256 = gf256;
            _rng = rng;
        }

        public IEnumerable<byte[]> Split(byte[] secret, int parts, int minimum)
        {
            var values = new byte[parts][];

            var length = secret.Length;

            for (var i = 0; i < parts; i++)
            {
                values[i] = new byte[length];
            }

            for (var i = 0; i < length; i++)
            {
                var polynomial = _gf256.Generate(minimum - 1, secret[i]);

                for (var j = 0; j < parts; j++)
                {
                    values[j][i] = _gf256.Evaluate(polynomial, (byte) (j + 1));
                }
            }

            var result = new List<byte[]>();

            for (var i = 0; i < parts; i++)
            {
                var value = new byte[length + 1];

                _rng.GetBytes(value);

                // Note, this is to make the output look more random, but limits parts to 16
                value[0] = (byte) ((value[0] & 0xF0) + i + 1);
                Array.Copy(values[i], 0, value, 1, length);

                result.Add(value);
            }

            return result;
        }

        public byte[] Join(IEnumerable<byte[]> parts)
        {

            var partsDictionary = new Dictionary<int, byte[]>();

            foreach (var part in parts)
            {
                var value = new byte[part.Length - 1];
                Array.Copy(part, 1, value, 0, part.Length - 1);

                partsDictionary.Add(part[0] & 0x0F, value);
            }

            var partCount = partsDictionary.Count;
            var length = partsDictionary.First().Value.Length;

            var secret = new byte[length];

            for (var i = 0; i < secret.Length; i++)
            {
                var points = new byte[partCount][];

                for (var j = 0; j < partCount; j++)
                {
                    points[j] = new byte[length];
                }

                var k = 0;
                foreach (var part in partsDictionary)
                {
                    points[k][0] = (byte) part.Key;
                    points[k][1] = part.Value[i];
                    k++;
                }

                secret[i] = _gf256.Interpolate(points);
            }

            return secret;
        }
    }
}