using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Linq;
using System.Collections;

public class clsSeed
{
    private static readonly byte[] baMnemonic = clsGBitcoin.NoBOMUTF8.GetBytes("mnemonic");

    public string SeedString { get; init; } = "";
    public string PassPhrase { get; init; } = "";
    public Int16[] iaSeedWordIDs { get; private set; }
    public string[] saSeedWords { get; private set; }
    public int iWordCount { get; set; }
    private BitArray bitaSeed;
    public byte[] SeedBytes { get; private set; }
    public string SeedBitsHex { get; private set; } = "";
    public string BIP39SeedHex { get; private set; } = "";

    private SHA256 mySHA256 = SHA256.Create();

    public clsSeed(string SeedWords,string PassPhrase = "")
    {

        this.SeedString = SeedWords;
        this.PassPhrase = PassPhrase;

        this.saSeedWords = SeedWords.Split(' ');
        if ((this.saSeedWords.Length!=12) && (this.saSeedWords.Length != 24))
            throw new Exception($"Incorrect seed word count {this.saSeedWords.Length}");

        this.iaSeedWordIDs = clsBIP39Words.GetWordIDsFromWords(saSeedWords);
        this.iWordCount = iaSeedWordIDs.Length;

        CalcAndCheckValuesFromWordIDs();
    }
    public clsSeed(byte[] baSeed, string PassPhrase = "")
    {
        SeedBytes = baSeed;
        BitArray bita = CopyByteArrayToBitArrayReverse(SeedBytes);

        this.saSeedWords = clsBIP39Words.GetWordsFromBitArray(bita);
        this.SeedString = String.Join(' ', this.saSeedWords);
        this.PassPhrase = PassPhrase;

        iaSeedWordIDs = clsBIP39Words.GetWordIDsFromWords(saSeedWords);
        iWordCount = iaSeedWordIDs.Length;

        CalcAndCheckValuesFromWordIDs();
    }
    private static BitArray CopyByteArrayToBitArrayReverse(in byte[] source)
    {
        BitArray bita = new BitArray(Convert.ToInt32(source.Length * 8));

        for (int i1 = 0; i1 < source.Length; i1++)
        {
            for (int iBit = 0; iBit < 8; iBit++)
            {
                bita[(i1 * 8) + iBit] = GetBit(source[i1], 7 - iBit);
            }
        }
        return bita;
    }
    ~clsSeed()
    {
        mySHA256.Dispose();
    }

    private void CalcAndCheckValuesFromWordIDs()
    {
        this.bitaSeed = new BitArray(11 * this.iaSeedWordIDs.Length);

        for (int iWordPos = 0; iWordPos < iaSeedWordIDs.Length; iWordPos++)
        {
            CopyBitArray(clsBIP39Words.liBitArrays[iaSeedWordIDs[iWordPos]], 0, ref this.bitaSeed,iWordPos * 11, 11);
        }

        this.SeedBytes = CopyBitArrayToByteArray(bitaSeed);

        CheckMnemonic();
        this.SeedBitsHex = ByteToHexString(SeedBytes);

        byte[] baSeedUFT8, baSalt;
        baSeedUFT8 = clsGBitcoin.NoBOMUTF8.GetBytes(SeedString);
        baSalt = clsGBitcoin.Concat(baMnemonic, clsGBitcoin.NoBOMUTF8.GetBytes(PassPhrase));
        Rfc2898DeriveBytes der = new Rfc2898DeriveBytes(baSeedUFT8, baSalt, 2048, HashAlgorithmName.SHA512);
        byte[] baDer = der.GetBytes(64);
        BIP39SeedHex = ByteToHexString(baDer);

        byte[] baCheck = StringToByteArray(BIP39SeedHex);

        if (baDer.SequenceEqual(baCheck) == false)
            throw new Exception("Byte arrays don't match");
    }
    public void CheckMnemonic()
    {
        int iNumOfBytes = Convert.ToInt32(Math.Floor(Convert.ToDouble((11 * this.iaSeedWordIDs.Length) / 8)));
        int iCRCLen = bitaSeed.Length - (iNumOfBytes * 8);

        if (iCRCLen == 0) //if 8 and 11 length matches at 24 words the last word is the CRC
            iCRCLen = 8;

        BitArray bitaOrigCheckSum = new BitArray(iCRCLen);
        BitArray bitaCalcCheckSum = new BitArray(iCRCLen);

        CopyBitArray(bitaSeed, bitaSeed.Length - iCRCLen, ref bitaOrigCheckSum, 0, iCRCLen);

        BitArray bitaToHash = new BitArray(bitaSeed.Length - iCRCLen);
        CopyBitArray(bitaSeed, 0, ref bitaToHash, 0, bitaSeed.Length-iCRCLen);

        byte[] toHashBytes = CopyBitArrayToByteArray(bitaToHash);

        byte[] baRet = mySHA256.ComputeHash(toHashBytes);

        for(int i1=0;i1<iCRCLen;i1++)
        {
            bitaCalcCheckSum[i1] = GetBit(baRet[0], 7-i1);
        }


        if (clsHelpers.CompareBitArrays(bitaOrigCheckSum,bitaCalcCheckSum)==false)
        {
            throw new Exception("Incorrect CRC");
        }
    }
    private static bool GetBit(byte bValue, int bitNumber)
    {
        return ((bValue & (1 << bitNumber)) != 0);
    }

    private static void CopyBitArray(in BitArray source, int iLeftPos, ref BitArray dest, int iRightPos, int iLen)
    {
        for (int i1 = 0; i1 < iLen; i1++)
        {
            dest[iRightPos + i1] = source[iLeftPos + i1];
        }
    }

    public static byte[] StringToByteArray(String hex)
    {
        int NumberChars = hex.Length;
        byte[] bytes = new byte[NumberChars / 2];
        for (int i = 0; i < NumberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }
    private static string ByteToHexString(byte[] bytes)
    {
        char[] c = new char[bytes.Length * 2];
        int b;
        for (int i = 0; i < bytes.Length; i++)
        {
            b = bytes[i] >> 4;
            c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
            b = bytes[i] & 0xF;
            c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
        }
        return new string(c);
    }
    private static byte[] CopyBitArrayToByteArray(in BitArray source)
    {
        int iNumOfBytes = Convert.ToInt32(Math.Floor(Convert.ToDouble((source.Length) / 8)));
        int iRema = source.Length % 8;
        if (iRema!=0)
        {   //Not full bytes
            iNumOfBytes += 1;
        }

        byte[] baRet = new byte[iNumOfBytes];
        //byte[] baTest = new byte[iNumOfBytes];

        for (int i1 = 0; i1 < iNumOfBytes; i1++)
        {
            BitArray bita8 = new BitArray(8);
            if (i1 * 8 + 8 < source.Length)
            {
                clsHelpers.CopyBitArray(source, i1 * 8, ref bita8, 0, 8);
            }
            else
            {
                clsHelpers.CopyBitArray(source, i1 * 8, ref bita8, 0, source.Length - i1 * 8);
            }
            baRet[i1] = clsHelpers.BitArrayToByte(bita8);

            //if (source[7 + (i1 * 8)]) baTest[i1] += 1;
            //if (source[6 + (i1 * 8)]) baTest[i1] += 2;
            //if (source[5 + (i1 * 8)]) baTest[i1] += 4;
            //if (source[4 + (i1 * 8)]) baTest[i1] += 8;
            //if (source[3 + (i1 * 8)]) baTest[i1] += 16;
            //if (source[2 + (i1 * 8)]) baTest[i1] += 32;
            //if (source[1 + (i1 * 8)]) baTest[i1] += 64;
            //if (source[0 + (i1 * 8)]) baTest[i1] += 128;
        }

        return baRet;
    }

}



//https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki#generating-the-mnemonic
//https://bitcoin.stackexchange.com/questions/68605/how-to-generate-a-valid-hash-for-a-bip39-seed-phrase