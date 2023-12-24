using LZ77_Library;
using System;

namespace Geisha_Compressor
{
    class Program
    {
        static void Main(string[] args)
        {
            string text = System.IO.File.ReadAllText("input//in.txt");
            byte[] unicodeBytes = System.Text.UnicodeEncoding.UTF8.GetBytes(text);
           
            LZ77_Compressor compressor = new LZ77_Compressor(ushort.MaxValue);
            byte[] output = compressor.Encode(unicodeBytes);
            System.IO.File.WriteAllBytes("out.lz77", output);
        }
    }
}