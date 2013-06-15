using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintrFlashr {
    class ConsoleProgressReporter: IProgressReporter {
        private string status = "Unknown job";
        private float perc;

        public void Message(string message) {
            WriteLine(message);
        }

        public void DeviceFeedback(string message) {
            WriteLine(string.Format("<< {0}", message));
        }

        public void DeviceError(string message) {
            WriteLine(string.Format("Error: {0}", message));
        }

        private void ClearStatus() {
            // Clear status message...
            var len = status.Length + 15;
            for(var i = 0; i < len; i++)
                Console.Write(" ");
            Console.Write("\r");
        }

        private void WriteLine(string line) {
            ClearStatus();
            Console.WriteLine(line);
        }

        private void ProgressReport() {
            Console.Write("{0}...", status);
            if(perc > 0)
                Console.Write(" ({0}%)", (perc * 100).ToString("000.00"));
            Console.Write("\r");
        }

        public void Status(string message) {
            ClearStatus();
            status = message;
            ProgressReport();
        }

        public void Progress(float perc) {
            this.perc = perc;
            ProgressReport();
        }
    }
}
