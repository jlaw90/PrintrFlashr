using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PrintrFlashr {
    class FlashException: Exception {
        public FlashException()
        {
        }

        public FlashException(string message) : base(message)
        {
        }

        public FlashException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FlashException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
