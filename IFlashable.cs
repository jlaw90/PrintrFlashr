using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintrFlashr {
    interface IFlashable
    {
        void Handshake(IProgressReporter rep);

        Version ReadVersion(IProgressReporter rep);

        void WriteFirmware(Firmware fw, IProgressReporter rep);

        void Boot(IProgressReporter rep);

        void Close(IProgressReporter rep);
    }
}