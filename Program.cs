using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PrintrFlashr {
    class Program {
        static void Exit(int code) {
            Console.Write("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(code);
        }

        static void Main(string[] args) {
            if(args.Length < 2) {
                Console.WriteLine("Usage: PrintrFlashr [port] [fw file]");
                Exit(0);
            }

            var prt = args[0];
            var file = args[1];
            if(file[0] == '"') {
                file = file.Substring(1);
                for(var i = 2; i < args.Length; i++) {
                    file += " " + args[i];
                    if(args[i].EndsWith("\"")) {
                        file = file.Substring(0, file.Length - 1);
                        break;
                    }
                }
            }

            Console.WriteLine("PrintrFlashr v1");

            IProgressReporter rep = new ConsoleProgressReporter();

            // Check firmware file exists...
            if(!File.Exists(file)) {
                rep.Message(string.Format("Firmware file specified does not exist: '{0}'", file));
                Exit(0);
            }

            
            rep.Status("Reading firmware");
            var fw = Firmware.ParseSrec(new FileStream(file, FileMode.Open));
            rep.Message(string.Format("Source FW: {0}", fw.Version));

            rep.Status(string.Format("Opening {0}", prt));
            IFlashable flash = new UsartFlashable(new SerialPort(prt, 28800, Parity.None, 8, StopBits.One));
            

            // Handshake
            flash.Handshake(rep);

            rep.Message(string.Format("Target FW: {0}", flash.ReadVersion(rep)));

            rep.Status("Sending firmware update");
            var start = DateTime.Now;
            flash.WriteFirmware(fw, rep);
            var taken = DateTime.Now - start;
            rep.Message("Firmware update completed in " + taken.TotalSeconds.ToString("00.00") + " seconds");
            rep.Message("New reported FW version: " + flash.ReadVersion(rep));
            rep.Message("Booting to application...");

            flash.Boot(rep);
            Thread.Sleep(10);
            flash.Handshake(rep);

            Exit(0);
        }
    }
}