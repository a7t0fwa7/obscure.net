using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using dnlib.DotNet;

namespace Obscure.Core.Utility
{
    public class Utils
    {
        static readonly RNGCryptoServiceProvider Csp = new RNGCryptoServiceProvider();
        internal static readonly char[] Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        public static int Next(int Min, int Max)
        {
            if (Min >= Max)
                throw new ArgumentOutOfRangeException("minValue must be lower than maxExclusiveValue");

            long Diff = (long)Max - Min;
            long UpperBound = uint.MaxValue / Diff * Diff;

            uint UInteger;
            do
            {
                UInteger = RandomUInt();
            } while (UInteger >= UpperBound);

            return (int)(Min + (UInteger % Diff));
        }

        public static uint RandomUInt()
        {
            byte[] Bytes = RandomBytes(sizeof(uint));
            return BitConverter.ToUInt32(Bytes, 0);
        }

        public static byte[] RandomBytes(int Length)
        {
            byte[] Buffer = new byte[Length];
            Csp.GetBytes(Buffer);
            return Buffer;
        }

        public static string RandomString(int Size)
        {
            byte[] Data = new byte[4 * Size];

            using (RNGCryptoServiceProvider Crypto = new RNGCryptoServiceProvider())
            {
                Crypto.GetBytes(Data);
            }

            StringBuilder Result = new StringBuilder(Size);
            for (int i = 0; i < Size; i++)
            {
                uint Random = BitConverter.ToUInt32(Data, i * 4);
                long Index = Random % Chars.Length;

                Result.Append(Chars[Index]);
            }

            return Result.ToString();
        }
    }
}