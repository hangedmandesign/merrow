using System;
using System.IO;

namespace Merrow
{
    public static class CRC
    {
        const int N64_HEADER_SIZE = 0x40;
        const int N64_BC_SIZE = 0x1000 - N64_HEADER_SIZE;

        const int N64_CRC1 = 0x10;
        const int N64_CRC2 = 0x14;

        const int CHECKSUM_START = 0x00001000;
        const int CHECKSUM_LENGTH = 0x00100000;
        const uint CHECKSUM_CIC6102 = 0xF8CA4DDC;
        const uint CHECKSUM_CIC6103 = 0xA3886759;
        const uint CHECKSUM_CIC6105 = 0xDF26F436;
        const uint CHECKSUM_CIC6106 = 0x1FEA617A;

        static readonly uint[] crcTable = new uint[256];

        public static uint Rol(uint value, int bits) =>
            (value << bits) | (value >> (32 - bits));

        static uint BytesToUInt32(byte[] buffer, int offset) =>
            (uint)((buffer[offset] << 24) | (buffer[offset + 1] << 16) |
                (buffer[offset + 2] << 8) | buffer[offset + 3]);

        static void WriteUInt32(byte[] buffer, int offset, uint value)
        {
            buffer[offset] = (byte)((value >> 24) & 0xFF);
            buffer[offset + 1] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 3] = (byte)(value & 0xFF);
        }

        static void GenCRCTableInPlace()
        {
            const uint poly = 0xEDB88320;
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int j = 0; j < 8; j++)
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ poly : crc >> 1;

                crcTable[i] = crc;
            }
        }

        public static uint[] GenCRCTable()
        {
            GenCRCTableInPlace();
            return crcTable;
        }

        static uint Crc32(byte[] data, int offset, int length)
        {
            uint crc = 0xFFFFFFFF;
            for (int i = offset; i < offset + length; i++)
            {
                crc = (crc >> 8) ^ crcTable[(crc ^ data[i]) & 0xFF];
            }
            return ~crc;
        }

        static int N64GetCIC(byte[] data)
        {
            uint crc = Crc32(data, N64_HEADER_SIZE, N64_BC_SIZE);
            switch (crc)
            {
                case 0x6170A4A1: return 6101;
                case 0x90BB6CB5: return 6102;
                case 0x0B050EE0: return 6103;
                case 0x98BC2C86: return 6105;
                case 0xACC8580A: return 6106;
                default: return 6105;
            }
        }

        static uint SeedFromBootcode(int bootcode)
        {
            switch (bootcode)
            {
                case 6101: return CHECKSUM_CIC6102;
                case 6102: return CHECKSUM_CIC6102;
                case 6103: return CHECKSUM_CIC6103;
                case 6105: return CHECKSUM_CIC6105;
                case 6106: return CHECKSUM_CIC6106;
                default: return 0;
            }
        }

        static bool N64CalcCRC(out uint[] crc, byte[] data)
        {
            crc = new uint[2];
            int bootcode = N64GetCIC(data);
            uint seed = SeedFromBootcode(bootcode);

            if (seed == 0)
                return true;

            uint t1 = seed, t2 = seed, t3 = seed, t4 = seed, t5 = seed, t6 = seed;

            for (int i = CHECKSUM_START; i < CHECKSUM_START + CHECKSUM_LENGTH; i += 4)
            {
                uint d = BytesToUInt32(data, i);
                if ((t6 + d) < t6) t4++;
                t6 += d;
                t3 ^= d;
                uint r = Rol(d, (int)(d & 0x1F));
                t5 += r;
                t2 ^= (t2 > d) ? r : t6 ^ d;

                if (bootcode == 6105)
                {
                    int idx = N64_HEADER_SIZE + 0x0710 + (i & 0xFF);
                    uint refVal = BytesToUInt32(data, idx);
                    t1 += refVal ^ d;
                }
                else
                {
                    t1 += t5 ^ d;
                }
            }

            if (bootcode == 6103)
            {
                crc[0] = (t6 ^ t4) + t3;
                crc[1] = (t5 ^ t2) + t1;
            }
            else if (bootcode == 6106)
            {
                crc[0] = (t6 * t4) + t3;
                crc[1] = (t5 * t2) + t1;
            }
            else
            {
                crc[0] = t6 ^ t4 ^ t3;
                crc[1] = t5 ^ t2 ^ t1;
            }

            return false;
        }

        public static int FixCrc(string filePath)
        {
            GenCRCTableInPlace();

            byte[] buffer;
            try
            {
                buffer = File.ReadAllBytes(filePath);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Unable to open \"{filePath}\": {ex.Message}");
                return 1;
            }

            if (buffer.Length < CHECKSUM_START + CHECKSUM_LENGTH)
            {
                Console.WriteLine("File too small or invalid N64 image.");
                return 1;
            }

            int cic = N64GetCIC(buffer);
            Console.WriteLine($"BootChip: {(cic != 0 ? $"CIC-NUS-{cic}" : "Unknown")}");

            if (N64CalcCRC(out uint[] crc, buffer))
            {
                Console.WriteLine("Unable to calculate CRC");
                return 1;
            }

            uint crc1Read = BytesToUInt32(buffer, N64_CRC1);
            uint crc2Read = BytesToUInt32(buffer, N64_CRC2);

            Console.WriteLine($"CRC 1: 0x{crc1Read:X8}  Calculated: 0x{crc[0]:X8} {(crc[0] == crc1Read ? "(Good)" : "(Bad, fixed)")}");
            if (crc[0] != crc1Read)
            {
                WriteUInt32(buffer, N64_CRC1, crc[0]);
            }

            Console.WriteLine($"CRC 2: 0x{crc2Read:X8}  Calculated: 0x{crc[1]:X8} {(crc[1] == crc2Read ? "(Good)" : "(Bad, fixed)")}");
            if (crc[1] != crc2Read)
            {
                WriteUInt32(buffer, N64_CRC2, crc[1]);
            }

            try
            {
                File.WriteAllBytes(filePath, buffer);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Failed to write file: {ex.Message}");
                return 1;
            }

            return 0;
        }
    }
}