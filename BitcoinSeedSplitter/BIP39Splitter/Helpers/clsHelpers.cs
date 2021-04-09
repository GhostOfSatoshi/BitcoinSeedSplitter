using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class clsHelpers
{
    //public static byte[] StringToByteArray(string str)
    //{
    //    Dictionary<string, byte> hexindex = new Dictionary<string, byte>();
    //    for (int i = 0; i <= 255; i++)
    //        hexindex.Add(i.ToString("X2"), (byte)i);

    //    List<byte> hexres = new List<byte>();
    //    for (int i = 0; i < str.Length; i += 2)
    //        hexres.Add(hexindex[str.Substring(i, 2)]);

    //    return hexres.ToArray();
    //}
    public static byte[] StringToByteArray(String hex)
    {
        int NumberChars = hex.Length;
        byte[] bytes = new byte[NumberChars / 2];
        for (int i = 0; i < NumberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }
    public static byte BitArrayToByte(BitArray source)
    {
        if (source.Length != 8)
            throw new Exception($"Incorrent array length in BitArrayToByte {source.Length}");

        byte bRet = 0;
        if (source[7]) bRet += 1;
        if (source[6]) bRet += 2;
        if (source[5]) bRet += 4;
        if (source[4]) bRet += 8;
        if (source[3]) bRet += 16;
        if (source[2]) bRet += 32;
        if (source[1]) bRet += 64;
        if (source[0]) bRet += 128;

        return bRet;
    }
    public static BitArray XORBitArray(in BitArray dataToXor, in BitArray hashToXorWith)
    {
        BitArray bitaRet = new BitArray(dataToXor.Length);
        int iHashPointer = 0;
        for (int i1 = 0; i1 < dataToXor.Length; i1++)
        {
            bitaRet[i1] = dataToXor[i1] ^ hashToXorWith[iHashPointer];

            if (iHashPointer==hashToXorWith.Length-1)
            {
                iHashPointer=0;
            }
            else
            {
                iHashPointer++;
            }
        }
        return bitaRet;
    }
    public static void CopyBitArray(in BitArray source, int iLeftPos, ref BitArray dest, int iRightPos, int iLen)
    {
        for (int i1 = 0; i1 < iLen; i1++)
        {
            dest[iRightPos + i1] = source[iLeftPos + i1];
        }
    }
    public static bool CompareBitArrays(in BitArray Left,in BitArray Right)
    {
        if (Left.Length != Right.Length)
            return false;

        for(int i1=0;i1<Left.Length;i1++)
        {
            if (Left[i1] != Right[i1])
                return false;
        }
        return true;
    }
    public static byte[] CopyBitArrayToByteArray(in BitArray source)
    {
        int iNumOfBytes = Convert.ToInt32(Math.Floor(Convert.ToDouble((source.Length) / 8)));

        byte[] baRet = new byte[iNumOfBytes];

        BitArray bita8 = new BitArray(8);
        for (int i1 = 0; i1 < iNumOfBytes; i1++)
        {
            clsHelpers.CopyBitArray(source, i1 * 8, ref bita8, 0, 8);

            baRet[i1] = CopyBitArrayToByte(bita8);
        }

        return baRet;
    }
    public static BitArray CopyByteArrayToBitArray(in byte[] source)
    {
        BitArray bita = new BitArray(Convert.ToInt32(source.Length * 8));

        for(int i1=0;i1<source.Length;i1++)
        {
            BitArray bita8 = CopyByteToBitArray8(source[i1]);
            clsHelpers.CopyBitArray(bita8, 0, ref bita, i1 * 8, 8);
        }

        return bita;
    }
    public static BitArray CopyByteToBitArray8(in byte bIn)
    {
        BitArray bita = new BitArray(8);

        for (int iBit = 0; iBit < 8; iBit++)
        {
            bita[iBit] = clsHelpers.GetBit(bIn, iBit);
        }
        return bita;
    }
    public static BitArray CopyByteToBitArray4(in byte bIn)
    {
        if (bIn > 15)
            throw new Exception("Max four bit value overflow.");

        BitArray bita = new BitArray(4);

        for (int iBit = 0; iBit < 4 ; iBit++)
        {
            bita[iBit] = clsHelpers.GetBit(bIn, iBit);
        }
        return bita;
    }
    public static byte CopyBitArrayToByte(in BitArray bitaIn)
    {
        if (bitaIn.Length > 8)
            throw new Exception("Max 8 bits value overflow.");

        byte bRet = 0;

        for (int i1 = 0; i1<bitaIn.Length ; i1++)
        {
            byte bCurrValue = Convert.ToByte(Math.Pow(2, i1));
            if (bitaIn[i1] == true)
            {
                bRet += bCurrValue;
            }
        }

        return bRet;
    }
    public static bool GetBit(byte bValue, int bitNumber)
    {
        return ((bValue & (1 << bitNumber)) != 0);
    }

}

