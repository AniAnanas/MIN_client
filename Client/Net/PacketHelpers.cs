using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Client.Net
{
    public static class PacketBuilder
    {
        public static byte[] Create(PacketType type, Action<BinaryWriter> writePayload = null)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                // Placeholder for Length (4 bytes)
                bw.Write((int)0);

                // Packet ID (2 bytes)
                bw.Write((ushort)type);

                // Payload
                writePayload?.Invoke(bw);

                // Update Length
                // C++ logic: length includes header. ms.Length is the total size.
                var length = (int)ms.Length;
                ms.Position = 0;
                bw.Write(length);

                return ms.ToArray();
            }
        }

        // C++ server expects 2 bytes length + UTF8 bytes
        public static void WriteString(this BinaryWriter bw, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            bw.Write((ushort)bytes.Length);
            bw.Write(bytes);
        }
    }

    public static class PacketReader
    {
        /// <summary>
        /// Reads a ushort-length-prefixed UTF8 string.
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public static string ReadEncodedString(this BinaryReader br)
        {
            ushort length = br.ReadUInt16();
            byte[] bytes = br.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
