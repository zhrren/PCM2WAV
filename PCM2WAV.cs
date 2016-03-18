using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCM2WAV
{
    public class PCM2WAV
    {

        /// <summary>  
        /// PCM to wav  
        /// 添加Wav头文件  
        /// 参考资料：http://blog.csdn.net/bluesoal/article/details/932395  
        /// </summary>  
        public static void CreateSoundFile(byte[] data, string path)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(stream);
            char[] ChunkRiff = { 'R', 'I', 'F', 'F' };
            char[] ChunkType = { 'W', 'A', 'V', 'E' };
            char[] ChunkFmt = { 'f', 'm', 't', ' ' };
            char[] ChunkData = { 'd', 'a', 't', 'a' };

            short shPad = 1;                // File padding  

            int nFormatChunkLength = 0x10; // Format chunk length.  

            int nLength = data.Length + 36;                // File length, minus first 8 bytes of RIFF description. This will be filled in later.  

            short shBytesPerSample = 0;     // Bytes per sample.  

            short BitsPerSample = 16; //每个采样需要的bit数    

            //这里需要注意的是有的值是short类型，有的是int，如果错了，会导致占的字节长度过长or过短  
            short channels = 1;//声道数目，1-- 单声道；2-- 双声道  

            // 一个样本点的字节数目  
            shBytesPerSample = 2;

            binaryWriter.Write("RIFF".ToCharArray());
            binaryWriter.Write(data.Length + 36);
            binaryWriter.Write("WAVE".ToCharArray());

            binaryWriter.Write("fmt ".ToCharArray());
            binaryWriter.Write(0x12);
            binaryWriter.Write((short)1);

            binaryWriter.Write((short)1); // Mono,声道数目，1-- 单声道；2-- 双声道  
            binaryWriter.Write(8000);// 16KHz 采样频率                     
            binaryWriter.Write(8000); //每秒所需字节数

            binaryWriter.Write((short)1);//数据块对齐单位(每个采样需要的字节数)  
            binaryWriter.Write((short)8);  // 16Bit,每个采样需要的bit数    
            binaryWriter.Write((short)0);

            binaryWriter.Write("fact".ToCharArray());
            binaryWriter.Write(4);
            binaryWriter.Write(data.Length);

            // 数据块  
            binaryWriter.Write("data".ToCharArray());
            binaryWriter.Write(data.Length);   // The sample length will be written in later.  


            binaryWriter.Write(data);
            binaryWriter.Flush();

            File.WriteAllBytes(path, stream.ToArray());

            binaryWriter.Dispose();
            stream.Dispose();
        }

        //        static const byte[] wav_template =
        //{
        //    // RIFF WAVE Chunk
        //    0x52, 0x49, 0x46, 0x46,		// "RIFF"
        //    0x30, 0x00, 0x00, 0x00,		// 总长度 整个wav文件大小减去ID和Size所占用的字节数
        //    0x57, 0x41, 0x56, 0x45,		// "WAVE"

        //    // Format Chunk  格式段
        //    0x66, 0x6D, 0x74, 0x20,		// "fmt "
        //    0x10, 0x00, 0x00, 0x00,		// 块长度
        //    0x01, 0x00,			// 编码方式
        //    0x01, 0x00,			// 声道数目
        //    0x80, 0x3E, 0x00, 0x00,		// 采样频率
        //    0x00, 0x7D, 0x00, 0x00,		// 每秒所需字节数=采样频率*块对齐字节
        //    0x02, 0x00,			// 数据对齐字节=每个样本字节数*声道数目
        //    0x10, 0x00,			// 样本宽度

        //    // Fact Chunk  FACT段
        //    0x66, 0x61, 0x63, 0x74,		// "fact"
        //    0x04, 0x00, 0x00, 0x00,		// 块长度
        //    0x00, 0xBE, 0x00, 0x00, 

        //    // Data Chunk  数据段
        //    0x64, 0x61, 0x74, 0x61,		// "data"
        //    0x00, 0x00, 0x00, 0x00,		// 块长度
        //};


    }
}
