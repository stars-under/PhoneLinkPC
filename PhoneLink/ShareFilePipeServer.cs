using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace PhoneLink
{
    class ShareFilePipeServer
    {
        NamedPipeServerStream pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
        StreamWriter sw = null;
        ShareFilePipeServer()
        {

        }
    }
}
