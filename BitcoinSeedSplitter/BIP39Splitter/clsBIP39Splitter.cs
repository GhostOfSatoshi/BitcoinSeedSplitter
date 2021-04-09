using System;
using System.Collections.Generic;
using System.Text;
using Security.Random;
using Security.Secrets;
using System.Linq;
using System.Security.Cryptography;
using System.Collections;

namespace BIP39Splitter
{
    public class clsBIP39Splitter
    {
        private SHA256 mySHA256 = SHA256.Create();
        private static int iPWHasCycleCount = 100000;

        public List<string> SeedToSplitSecrets(in byte[] baSecretIn, byte TotalShares, byte Threshold,string Password)
        {
            byte[] baSecret = new byte[baSecretIn.Length];
            Password = Password.Trim();
            if (Password.Length>0)
            {
                byte[] baPassword = Encoding.ASCII.GetBytes(Password);

                byte[] baPWHash = mySHA256.ComputeHash(baPassword);
                for (int i1 = 0; i1 < iPWHasCycleCount; i1++)
                {
                    baPWHash = mySHA256.ComputeHash(baPWHash);
                }                
                BitArray bitaPWHash = clsHelpers.CopyByteArrayToBitArray(baPWHash);
                BitArray bitaSecret = clsHelpers.CopyByteArrayToBitArray(baSecretIn);

                BitArray bitaXORed = clsHelpers.XORBitArray(bitaSecret, bitaPWHash);

                BitArray bitaDoubleXORed = clsHelpers.XORBitArray(bitaXORed, bitaPWHash);

                if (clsHelpers.CompareBitArrays(bitaSecret, bitaDoubleXORed) == false)
                    throw new Exception("BitArrays XOR validation failed");

                baSecret = clsHelpers.CopyBitArrayToByteArray(bitaXORed);
            }
            else
            {
                Array.Copy(baSecretIn, baSecret, baSecretIn.Length);
            }

            ShamirSecretShare sham = new ShamirSecretShare(new Gf256(new Rng()), new Rng());

            List<byte[]> liSplitParts = sham.Split(baSecret, TotalShares, Threshold).ToList();

            List<string> liShares = new List<string>();

            byte[] baIDHash=mySHA256.ComputeHash(baSecret);
            BitArray bitaHash = clsHelpers.CopyByteArrayToBitArray(baIDHash);
            BitArray bitaSplitID = new BitArray(11);
            clsHelpers.CopyBitArray(bitaHash, 0, ref bitaSplitID, 0, 11);

            UInt16 iSplitID = clsBIP39Words.GetWordValueFromBits(bitaSplitID);

            byte bShareID = 1;
            foreach(byte[] ba in liSplitParts)
            {
                string sShare = GetShareWords(ba, iSplitID, bShareID, Threshold);
                liShares.Add(sShare);
                bShareID++;
            }

            return liShares;
        }

        public bool CheckShare (string sShare)
        {
            byte[] baPayload;
            string sRet = GetBytesFromShare(sShare, 99, out baPayload);  //Force to not check if enough shares provided. Force to decode the whole share to check if it's valid
            if (sRet == "")
                return true;
            else
                return false;   
        }
        private string GetShareWords(byte[] baNewSecret, UInt16 iSplitID, byte ShareID,byte Threshold)
        {
            clsShareHeader header = new clsShareHeader();
            string sRet=header.BuildHeader(baNewSecret, iSplitID, ShareID, Threshold);
            if (sRet != "")
                return sRet;
            
            BitArray bitaSecret = clsHelpers.CopyByteArrayToBitArray(baNewSecret);

            int iCRCLen = 11 - ((header.bits.Length + bitaSecret.Length) % 11);
            if (iCRCLen < 4)
                iCRCLen += 11;
            int iTotalLen = header.bits.Length + bitaSecret.Length + iCRCLen;

            BitArray bitaShare = new BitArray(iTotalLen);

            clsHelpers.CopyBitArray(header.bits, 0, ref bitaShare, 0, header.bits.Length);
            clsHelpers.CopyBitArray(bitaSecret, 0, ref bitaShare, header.bits.Length, bitaSecret.Length);
            byte[] baToHash = clsHelpers.CopyBitArrayToByteArray(bitaShare);
            byte[] baHash = mySHA256.ComputeHash(baToHash);
            BitArray bitaHash = clsHelpers.CopyByteArrayToBitArray(baHash);
            clsHelpers.CopyBitArray(bitaHash, 0, ref bitaShare, bitaShare.Length - iCRCLen, iCRCLen);

            string ShareString = "";
            for (int i1 = 0; i1 < bitaShare.Length; i1 += 11)
            {
                BitArray bitaWord = new BitArray(11);
                clsHelpers.CopyBitArray(bitaShare, i1, ref bitaWord, 0, 11);

                string sWord = clsBIP39Words.GetWordFromBits(bitaWord);

                ShareString += sWord + " ";
            }

            ShareString = ShareString.Trim();

            //Seed = clsHelpers.CopyBitArrayToByteArray(bitaShare);
            //ShareHexString = clsHelpers.ByteToHexString(Seed);

            return ShareString;
        }

        public string JoinSecrets(List<string> MyShares, out byte[] baSecret,string Password)
        {
            baSecret = null;

            List<byte[]> liShares = new List<byte[]>();
            foreach (string sShare in MyShares)
            {
                byte[] baShare;
                string sRet=GetBytesFromShare(sShare,MyShares.Count,out baShare);
                if (sRet!="")
                {
                    return sRet;
                }
                liShares.Add(baShare);
            }

            ShamirSecretShare sham = new ShamirSecretShare(new Gf256(new Rng()), new Rng());

            baSecret = sham.Join(liShares);

            Password = Password.Trim();
            if (Password.Length > 0)
            {
                byte[] baPassword = Encoding.ASCII.GetBytes(Password);
                byte[] baPWHash = mySHA256.ComputeHash(baPassword);
                for (int i1 = 0; i1 < iPWHasCycleCount; i1++)
                {
                    baPWHash = mySHA256.ComputeHash(baPWHash);
                }
                BitArray bitaPWHash = clsHelpers.CopyByteArrayToBitArray(baPWHash);
                BitArray bitaSecret = clsHelpers.CopyByteArrayToBitArray(baSecret);

                BitArray bitaXORed = clsHelpers.XORBitArray(bitaSecret, bitaPWHash);

                BitArray bitaDoubleXORed = clsHelpers.XORBitArray(bitaXORed, bitaPWHash);

                if (clsHelpers.CompareBitArrays(bitaSecret, bitaDoubleXORed) == false)
                    throw new Exception("BitArrays XOR validation failed");
                baSecret = clsHelpers.CopyBitArrayToByteArray(bitaXORed);
            }

            return "";
        }
        private string GetBytesFromShare(string sShare, int iAvailableShares, out byte[] baShare)
        {
            baShare = null;

            try
            {
                string[] saSeed = sShare.Split(' ');
                BitArray bitaFullShare = new BitArray(saSeed.Length * 11);

                for (int iPos = 0; iPos < saSeed.Length; iPos++)
                {
                    BitArray bitaWord = clsBIP39Words.GetBitsFromWord(saSeed[iPos]);
                    clsHelpers.CopyBitArray(bitaWord, 0, ref bitaFullShare, iPos * 11, 11);
                }

                BitArray bitaHeader = new BitArray(27);
                clsHelpers.CopyBitArray(bitaFullShare, 0, ref bitaHeader, 0, bitaHeader.Length);

                clsShareHeader header = new clsShareHeader();
                string sRet = header.BuildHeader(bitaHeader);
                if (sRet != "")
                    return sRet;

                if (iAvailableShares < header.Threshold)
                    return $"Not enough shares Needed:{header.Threshold} Provided:{iAvailableShares}";

                int iCRCLen = bitaFullShare.Length - (header.DataLenInBytes * 8) - bitaHeader.Length;

                BitArray bitaCRCOrig = new BitArray(iCRCLen);
                clsHelpers.CopyBitArray(bitaFullShare, bitaFullShare.Length - iCRCLen, ref bitaCRCOrig, 0, iCRCLen);

                //Calc new check CRC
                int iTotalLen = header.bits.Length + (header.DataLenInBytes * 8) + iCRCLen;

                if (iTotalLen != bitaFullShare.Length)
                    return "Lenght calculation error";

                BitArray bitaPayload = new BitArray(header.DataLenInBytes * 8);
                clsHelpers.CopyBitArray(bitaFullShare, bitaHeader.Length, ref bitaPayload, 0, bitaPayload.Length);

                BitArray bitaForCRC = new BitArray(iTotalLen);
                clsHelpers.CopyBitArray(header.bits, 0, ref bitaForCRC, 0, header.bits.Length);
                clsHelpers.CopyBitArray(bitaPayload, 0, ref bitaForCRC, header.bits.Length, bitaPayload.Length);
                byte[] baToHash = clsHelpers.CopyBitArrayToByteArray(bitaForCRC);
                byte[] baHash = mySHA256.ComputeHash(baToHash);
                BitArray bitaNewHash = clsHelpers.CopyByteArrayToBitArray(baHash);

                BitArray bitaCRCNew = new BitArray(iCRCLen);
                clsHelpers.CopyBitArray(bitaNewHash, 0, ref bitaCRCNew, 0, iCRCLen);

                if (clsHelpers.CompareBitArrays(bitaCRCOrig, bitaCRCNew) == false)
                    return $"CRC error in {sShare}";

                baShare = clsHelpers.CopyBitArrayToByteArray(bitaPayload);

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        ~clsBIP39Splitter()
        {
            mySHA256.Dispose();
        }
    }
    public class clsShareHeader
    {
        //Header = 27 bits 
        //SplitID:11
        //ShareID:4
        //Threshold:4
        //DataLen:8 
        public BitArray bits;
        public UInt16 SplitID;
        public byte ShareID, Threshold,DataLenInBytes;
        public string BuildHeader(in byte[] baSecret, UInt16 SplitID, byte ShareID, byte Threshold)
        {
            this.SplitID = SplitID;
            this.ShareID = ShareID;
            this.Threshold = Threshold;
            this.DataLenInBytes = (byte)baSecret.Length;
            this.bits = new BitArray(27);

            BitArray bitaSplitID = clsBIP39Words.liBitArrays[SplitID];
            BitArray bitaShareID = clsHelpers.CopyByteToBitArray4(ShareID);
            BitArray bitaThreshold = clsHelpers.CopyByteToBitArray4(Threshold);
            BitArray bitaDataLenInBytes = clsHelpers.CopyByteToBitArray8(this.DataLenInBytes);

            clsHelpers.CopyBitArray(bitaSplitID, 0, ref this.bits, 0, 11);
            clsHelpers.CopyBitArray(bitaShareID, 0, ref this.bits, 11, 4);
            clsHelpers.CopyBitArray(bitaThreshold, 0, ref this.bits, 15, 4);
            clsHelpers.CopyBitArray(bitaDataLenInBytes, 0, ref this.bits, 19, 8);

            clsShareHeader test = new clsShareHeader();

            string sRet=test.BuildHeader(this.bits);
            if (sRet != "")
                return sRet;

            if (clsHelpers.CompareBitArrays(this.bits, test.bits) == false)
                return ("Header missmatch CompareBitArrays");
            if (this.SplitID != test.SplitID)
                return ("Header missmatch SplitID");
            if (this.ShareID!=test.ShareID)
                return ("Header missmatch ShareID");
            if (this.Threshold != test.Threshold)
                return ("Header missmatch Threshold");
            if (this.DataLenInBytes != test.DataLenInBytes)
                return ("Header missmatch DataLenInBytes");

            return "";
        }
        public string BuildHeader(BitArray bitaShare)
        {

            this.bits = bitaShare;

            BitArray bitaSplitID = new BitArray(11);
            clsHelpers.CopyBitArray(this.bits, 0, ref bitaSplitID, 0, 11);
            SplitID =  clsBIP39Words.GetWordValueFromBits(bitaSplitID);

            BitArray bitaShareID = new BitArray(4);
            clsHelpers.CopyBitArray(this.bits, 11, ref bitaShareID, 0, 4);
            this.ShareID = clsHelpers.CopyBitArrayToByte(bitaShareID);

            BitArray bitaThreshold = new BitArray(4);
            clsHelpers.CopyBitArray(this.bits, 15, ref bitaThreshold, 0, 4);
            this.Threshold = clsHelpers.CopyBitArrayToByte(bitaThreshold);

            BitArray bitaDataLenInBytes = new BitArray(8);
            clsHelpers.CopyBitArray(this.bits, 19, ref bitaDataLenInBytes, 0, 8);
            this.DataLenInBytes = clsHelpers.CopyBitArrayToByte(bitaDataLenInBytes);

            return "";
        }

    }
}

