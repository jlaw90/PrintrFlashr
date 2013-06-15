using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintrFlashr {
    interface IProgressReporter
    {
        void Message(string message);

        void DeviceFeedback(string message);

        void DeviceError(string message);

        void Status(string message);

        void Progress(float perc);
    }
}