using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCM2WAV
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("请输入文件名");
                return;
            }

            string sourcePath = args[0];
            var data = File.ReadAllBytes(sourcePath);
            
            WaveFile.Create(sourcePath+".wav", 16000, 16, 1, data);
        }
    }
}
