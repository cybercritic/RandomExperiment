using System;
using System.Text;
using System.Security.Cryptography;

namespace Random_Experiment_WPF
{
    public class SHA
    {
        public static byte[] GetHash(string input)
        {
            byte[] data = Encoding.UTF8.GetBytes(input);

            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] hashValue = mySHA256.ComputeHash(data);

            return hashValue;
        }

        public static string GetSHAString(byte[] data)
        {
            string result = "";
            for (int i = 0; i < data.Length; i++)
                result += String.Format("{0:X2}", data[i]);

            return result;
        }
    }
}