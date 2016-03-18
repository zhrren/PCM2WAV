using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCM2WAV
{
    public class WaveFile
    {
        public const int WAVE_FORMAT_PCM = 1;

        public static void Create(string fileName, uint samplesPerSecond, short bitsPerSample, short channels, Byte[] data)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            WaveFileHeader header = CreateNewWaveFileHeader(samplesPerSecond, bitsPerSample, channels, (uint)(data.Length), 44 + data.Length);
            WriteHeader(fileName, header);
            WriteData(fileName, header.DATAPos, data);
        }

        public static void AppendData(string fileName, Byte[] data)
        {
            WaveFileHeader header = ReadHeader(fileName);

            if (header.DATASize > 0)
            {
                WriteData(fileName, (int)(header.DATAPos + header.DATASize), data);

                header.DATASize += (uint)data.Length;
                header.RiffSize += (uint)data.Length;

                WriteHeader(fileName, header);
            }
        }

        public static WaveFileHeader Read(string fileName)
        {
            WaveFileHeader header = ReadHeader(fileName);

            return header;
        }

        private static WaveFileHeader CreateNewWaveFileHeader(uint SamplesPerSecond, short BitsPerSample, short Channels, uint dataSize, long fileSize)
        {
            WaveFileHeader Header = new WaveFileHeader();

            Array.Copy(new char[] { 'R', 'I', 'F', 'F' }, Header.RIFF, 4);
            Header.RiffSize = (uint)(fileSize - 8);
            Array.Copy(new char[] { 'W', 'A', 'V', 'E' }, Header.RiffFormat, 4);
            Array.Copy(new char[] { 'f', 'm', 't', ' ' }, Header.FMT, 4);
            Header.FMTSize = 16;
            Header.AudioFormat = WAVE_FORMAT_PCM;
            Header.Channels = (short)Channels;
            Header.SamplesPerSecond = (uint)SamplesPerSecond;
            Header.BitsPerSample = (short)BitsPerSample;
            Header.BlockAlign = (short)((BitsPerSample * Channels) >> 3);
            Header.BytesPerSecond = (uint)(Header.BlockAlign * Header.SamplesPerSecond);
            Array.Copy(new char[] { 'd', 'a', 't', 'a' }, Header.DATA, 4);
            Header.DATASize = dataSize;

            return Header;
        }

        private static WaveFileHeader ReadHeader(string fileName)
        {
            WaveFileHeader header = new WaveFileHeader();

            if (File.Exists(fileName))
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryReader rd = new BinaryReader(fs, Encoding.UTF8);

                if (fs.CanRead)
                {
                    header.RIFF = rd.ReadChars(4);
                    header.RiffSize = (uint)rd.ReadInt32();
                    header.RiffFormat = rd.ReadChars(4);

                    header.FMT = rd.ReadChars(4);
                    header.FMTSize = (uint)rd.ReadInt32();
                    header.FMTPos = fs.Position;
                    header.AudioFormat = (short)rd.ReadInt16();
                    header.Channels = (short)rd.ReadInt16();
                    header.SamplesPerSecond = (uint)rd.ReadInt32();
                    header.BytesPerSecond = (uint)rd.ReadInt32();
                    header.BlockAlign = (short)rd.ReadInt16();
                    header.BitsPerSample = (short)rd.ReadInt16();

                    fs.Seek(header.FMTPos + header.FMTSize, SeekOrigin.Begin);

                    header.DATA = rd.ReadChars(4);
                    header.DATASize = (uint)rd.ReadInt32();
                    header.DATAPos = (int)fs.Position;

                    if (new String(header.DATA).ToUpper() != "DATA")
                    {
                        uint DataChunkSize = header.DATASize + 8;
                        fs.Seek(DataChunkSize, SeekOrigin.Current);
                        header.DATASize = (uint)(fs.Length - header.DATAPos - DataChunkSize);
                    }

                    header.Payload = rd.ReadBytes((int)header.DATASize);
                }

                rd.Close();
                fs.Close();
            }

            return header;
        }

        private static void WriteHeader(string fileName, WaveFileHeader header)
        {
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryWriter wr = new BinaryWriter(fs, Encoding.UTF8);

            wr.Write(header.RIFF);
            wr.Write(Int32ToBytes((int)header.RiffSize));
            wr.Write(header.RiffFormat);

            wr.Write(header.FMT);
            wr.Write(Int32ToBytes((int)header.FMTSize));
            wr.Write(Int16ToBytes(header.AudioFormat));
            wr.Write(Int16ToBytes(header.Channels));
            wr.Write(Int32ToBytes((int)header.SamplesPerSecond));
            wr.Write(Int32ToBytes((int)header.BytesPerSecond));
            wr.Write(Int16ToBytes((short)header.BlockAlign));
            wr.Write(Int16ToBytes((short)header.BitsPerSample));

            wr.Write(header.DATA);
            wr.Write(Int32ToBytes((int)header.DATASize));

            wr.Close();
            fs.Close();
        }

        private static void WriteData(string fileName, int pos, Byte[] data)
        {
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryWriter wr = new BinaryWriter(fs, Encoding.UTF8);

            wr.Seek(pos, System.IO.SeekOrigin.Begin);
            wr.Write(data);
            wr.Close();
            fs.Close();
        }

        private static int BytesToInt32(ref Byte[] bytes)
        {
            int Int32 = 0;
            Int32 = (Int32 << 8) + bytes[3];
            Int32 = (Int32 << 8) + bytes[2];
            Int32 = (Int32 << 8) + bytes[1];
            Int32 = (Int32 << 8) + bytes[0];
            return Int32;
        }

        private static short BytesToInt16(ref Byte[] bytes)
        {
            short Int16 = 0;
            Int16 = (short)((Int16 << 8) + bytes[1]);
            Int16 = (short)((Int16 << 8) + bytes[0]);
            return Int16;
        }

        private static Byte[] Int32ToBytes(int value)
        {
            Byte[] bytes = new Byte[4];
            bytes[0] = (Byte)(value & 0xFF);
            bytes[1] = (Byte)(value >> 8 & 0xFF);
            bytes[2] = (Byte)(value >> 16 & 0xFF);
            bytes[3] = (Byte)(value >> 24 & 0xFF);
            return bytes;
        }

        private static Byte[] Int16ToBytes(short value)
        {
            Byte[] bytes = new Byte[2];
            bytes[0] = (Byte)(value & 0xFF);
            bytes[1] = (Byte)(value >> 8 & 0xFF);
            return bytes;
        }
    }

    public class WaveFileHeader
    {
        public Char[] RIFF = new Char[4];
        public uint RiffSize = 8;
        public Char[] RiffFormat = new Char[4];
        public Char[] FMT = new Char[4];
        public uint FMTSize = 16;
        public short AudioFormat;
        public short Channels;
        public uint SamplesPerSecond;
        public uint BytesPerSecond;
        public short BlockAlign;
        public short BitsPerSample;
        public Char[] DATA = new Char[4];
        public uint DATASize;
        public Byte[] Payload = new Byte[0];
        public int DATAPos = 44;
        public long FMTPos = 20;

        public TimeSpan Duration
        {
            get
            {
                int blockAlign = ((BitsPerSample * Channels) >> 3);
                int bytesPerSec = (int)(blockAlign * SamplesPerSecond);
                double value = (double)Payload.Length / (double)bytesPerSec;

                return new TimeSpan(0, 0, (int)value);
            }
        }
    }
}
