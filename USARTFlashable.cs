using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PrintrFlashr {
    class UsartFlashable: IFlashable {
        private SerialPort Port { get; set; }
        public Version Version { get; private set; }

        public UsartFlashable(SerialPort port) {
            Version = Version.Unknown;
            Port = port;
            Port.Open();
        }

        public void Handshake(IProgressReporter rep) {
            SendAndWait(new string(new[] { (char) 1 }), "OK", rep);
            SendAndWait("HELLO", "READY", rep);
        }

        public Version ReadVersion(IProgressReporter rep) {
            SendAndWait("FWVER", "OK", rep);
            return Version;
        }

        public void WriteFirmware(Firmware fw, IProgressReporter rep) {
            SendAndWait("WRITE", "OK", rep);
            SendAndWait(new string(new[] { (char) 2, (char) fw.Version.Major, (char) fw.Version.Minor }), "OK", rep);
            for(var i = 0; i < fw.Chunks.Count; i++) {
                var kvp = fw.Chunks.ElementAt(i);
                var addr = kvp.Key;
                var data = new byte[kvp.Value.Length + 5];
                data[0] = 0;
                data[1] = (byte) ((addr >> 0) & 0xff);
                data[2] = (byte) ((addr >> 8) & 0xff);
                data[3] = (byte) ((addr >> 16) & 0xff);
                data[4] = (byte) ((addr >> 24) & 0xff);
                Array.Copy(kvp.Value, 0, data, 5, kvp.Value.Length);
                Write(data);
                Wait("OK", rep);
                rep.Progress((float) (i + 1) / (float) fw.Chunks.Count);
            }
            Write(3);
            Wait("OK", rep);
        }

        public void Boot(IProgressReporter rep) {
            SendAndWait("BOOT", "OK", rep);
        }

        public void Close(IProgressReporter rep) {
            Port.Close();
        }

        private void ProcessLine(string line, IProgressReporter rep) {
            if(line.StartsWith("E:")) {
                rep.DeviceError(line.Substring(2));
                throw new FlashException(line.Substring(2));
            }
            if(line.StartsWith("FW:")) {
                Version = Version.Parse(line.Substring(3));
                return;
            }
            if(line.StartsWith("I:"))
                line = line.Substring(2);
            rep.DeviceFeedback(line);
        }

        private void Wait(string resp, IProgressReporter rep) {
            while(true) {
                string line;
                while((line = ReadLine()) != null && line != resp)
                    ProcessLine(line, rep);
                if(line == resp)
                    return;
            }
        }

        private void SendAndWait(string send, string resp, IProgressReporter rep) {
            WriteLine(send);
            Wait(resp, rep);
        }

        private void Write(byte[] b, int off, int len) {
            var data = new byte[len + 6];
            data[0] = (byte) 'P';
            data[1] = (byte) len;
            Array.Copy(b, off, data, 2, len);
            byte checksum = 0;
            for(var i = 0; i < len + 2; i++)
                checksum += data[i];
            checksum ^= 0xff;
            data[len + 3] = checksum;
            data[len + 4] = 0;
            data[len + 5] = 0;
            Port.Write(data, 0, data.Length);
        }

        private void Write(params byte[] b) {
            Write(b, 0, b.Length);
        }

        private void WriteLine(string s) {
            Write(Encoding.ASCII.GetBytes(s));
        }

        StringBuilder _buf = new StringBuilder();
        readonly byte[] _dbuf = new byte[256];
        private string ReadLine() {
            if(Port.BytesToRead > 0) {
                var read = Port.Read(_dbuf, 0, Port.BytesToRead);
                for(var i = 0; i < read; i++)
                    _buf.Append((char) _dbuf[i]);
            }

            var s = _buf.ToString();
            var nidx = s.IndexOf('\n');
            if(nidx < 0)
                return null;
            _buf = _buf.Remove(0, nidx + 1);
            return s.Substring(0, nidx).Replace("\r", "");
        }
    }
}