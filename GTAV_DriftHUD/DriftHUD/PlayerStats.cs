using System;
using System.Text;
using System.IO;
using GTA.Native;
using System.Security.Cryptography;

namespace GTAV_DriftHUD
{
    public static class PlayerStats
    {
        static readonly string DataHash = "hxcdr1ft";
        static readonly string Salt = "Dolth0ay44";
        static readonly string VIKey = "@pQsQDF6vpfJA84A";

        public static void WritePlayerStat(VehicleHash hash, int data)
        {
            string str = encrypt(string.Format("{0}>{1}", (int)hash, data));

            using (var fstream = new FileStream("scripts\\driftstats.stat", FileMode.OpenOrCreate))
            {
                int seekPos = 0;
                byte[] buffer = new byte[24];

                while (seekPos < fstream.Length)
                {
                    fstream.Seek(seekPos, SeekOrigin.Begin);
                    fstream.Read(buffer, 0, 24);
                    var line = decrypt(Encoding.ASCII.GetString(buffer));
                    var keyVal = line.Substring(0, line.IndexOf('>'));
                    var value = line.Substring(line.IndexOf('>') + 1);
                    if (keyVal == ((int)hash).ToString())
                    {
                        using (StreamWriter writer = new StreamWriter(fstream))
                        {
                            writer.BaseStream.Seek(seekPos, SeekOrigin.Begin);
                            writer.BaseStream.Write(Encoding.ASCII.GetBytes(str), 0, 24);
                        }
                        return;
                    }
                    seekPos += 24;
                }
            }

            using (StreamWriter writer = File.AppendText("scripts\\driftstats.stat"))
            {
                if (writer.BaseStream.CanWrite)
                    writer.Write(str);
            }
        }

        public static int ReadPlayerStat(VehicleHash hash)
        {
            using (var fstream = new FileStream("scripts\\driftstats.stat", FileMode.OpenOrCreate))
            {
                int seekPos = 0;
                byte[] buffer = new byte[24];

                while (seekPos < fstream.Length)
                {
                    fstream.Seek(seekPos, SeekOrigin.Begin);
                    fstream.Read(buffer, 0, 24);
                    var line = decrypt(Encoding.ASCII.GetString(buffer));
                    var keyVal = line.Substring(0, line.IndexOf('>'));
                    var value = line.Substring(line.IndexOf('>') + 1);
                    if (keyVal == ((int)hash).ToString())
                        return Convert.ToInt32(value);
                    seekPos += 24;
                }
                return -1;
            }
        }

        private static string encrypt(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] keyBytes = new Rfc2898DeriveBytes(DataHash, Encoding.ASCII.GetBytes(Salt)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));

            byte[] cipherTextBytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherTextBytes = memoryStream.ToArray();
                    cryptoStream.Close();
                }
                memoryStream.Close();
            }
            return Convert.ToBase64String(cipherTextBytes);
        }

        private static string decrypt(string encryptedText)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = new Rfc2898DeriveBytes(DataHash, Encoding.ASCII.GetBytes(Salt)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };

            var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        }
    }
}
