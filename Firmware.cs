using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintrFlashr {
    internal class Firmware {
        public Version Version { get; private set; }
        public SortedDictionary<uint, byte[]> Chunks { get; private set; }

        public static Firmware ParseSrec(FileStream fs) {
            var sr = new StreamReader(fs);
            string line;

            var vendorInfo = new List<string>();
            var chunks = new SortedDictionary<uint, byte[]>();
            while((line = sr.ReadLine()) != null) {
                var idx = 0;
                if(line[idx++] != 'S')
                    throw new InvalidDataException("Line does not start with 'S'");
                var type = int.Parse(line.Substring(idx++, 1));
                if(type == 4 || type == 6 || type > 9)
                    throw new InvalidDataException("Unexpected record type: " + type);

                var count = ReadHex(line, ref idx);
                var checksum = count;
                var raw = new byte[count];
                for(var i = 0; i < count - 1; i++) {
                    raw[i] = ReadHex(line, ref idx);
                    checksum += raw[i];
                }
                checksum ^= 0xff;
                var cs = ReadHex(line, ref idx);
                if(cs != checksum)
                    throw new InvalidDataException("Checksum mismatch");
                idx = 0;
                var addrSize = type == 3 || type == 7 ? 4 : (type == 2 || type == 8 ? 3 : 2);
                uint address = 0;
                for(var i = 0; i < addrSize; i++)
                    address |= (uint) (raw[idx++] << (((addrSize - i - 1) * 8)));

                var dataSize = count - addrSize - 1;
                var data = new byte[dataSize];
                Array.Copy(raw, idx, data, 0, dataSize);
                switch(type) {
                    case 0:
                        vendorInfo.Add(Encoding.ASCII.GetString(data));
                        break;
                    case 5:
                        if(chunks.Count != address)
                            throw new InvalidDataException("S5 wrong count");
                        break;
                    case 7:
                    case 8:
                    case 9:
                        break; // Ignore...
                    case 1:
                    case 2:
                    case 3:
                        chunks.Add(address, data);
                        break;
                    default:
                        throw new InvalidDataException("Unexpected record");
                }
            }
            var v = Version.Unknown;
            foreach (var s1 in from s in vendorInfo where s.StartsWith("V:") select s.Substring(2))
                v = Version.Parse(s1);
            return new Firmware() {
                Version = v,
                Chunks = chunks
            };
        }

        private static byte ReadHexNibble(string line, ref int idx) {
            var c = line[idx++];
            if(c >= '0' && c <= '9')
                return (byte) (c - '0');
            if(c >= 'a' && c <= 'f')
                return (byte) ((c - 'a') + 10);
            if(c >= 'A' && c <= 'F')
                return (byte) ((c - 'A') + 10);
            throw new InvalidDataException("Invalid hex character: " + c);
        }

        private static byte ReadHex(string line, ref int idx) {
            return (byte) ((ReadHexNibble(line, ref idx) << 4) | ReadHexNibble(line, ref idx));
        }
    }
}
