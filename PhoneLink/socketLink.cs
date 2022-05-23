using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using PhoneLink;

namespace PhoneLink
{
    class serverLink
    {
        public SocketStream In;
        public SocketStream Out;
        public serverLink(string host, int port, int key,string name)
        {
            In = new SocketStream(host, port, key, "IN");
            In.SendString(name);
            if (In.ReadString() != "OK")
            {
                return;
            }
            Out = new SocketStream(host, port, key, "OUT");
            Out.SendString(name);
            if (Out.ReadString() != "OK")
            {
                return;
            }
        }
        public string? textSync(string text)
        {
            In.SendString("textSync");
            string? buff;
            buff = In.ReadString();
            if (buff != "OK")
            {
                return buff;
            }
            In.SendString(text);
            buff = In.ReadString();
            if (buff != "OK")
            {
                return buff;
            }
            int len = 0;
            byte[] intBytes = In.Read(ref len);
            if (intBytes.Length > 4)
            {
                return "";
            }
            int deviceNum = BitConverter.ToInt32(intBytes);
            return "同步给了" + deviceNum.ToString() + "台设备";
        }
        public string? GetTemporaryData()
        {
            In.SendString("GetTemporaryData");
            return In.ReadString();
        }

        public byte[] BitmapSoureAsPngByte(BitmapSource image)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();

            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
                byte[] bit = stream.ToArray();
                stream.Close();
                return bit;
            }
        }
        //test
        public string? imageSync(BitmapSource image)
        {
            In.SendString("SyncImage");

            string? buff;
            buff = In.ReadString();
            if (buff != "OK")
            {
                return buff;
            }

            In.SendString(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + ".png");

            buff = In.ReadString();
            if (buff != "OK")
            {
                return buff;
            }

            byte[] bytes = BitmapSoureAsPngByte(image);

            In.Send(bytes, bytes.Length);

            buff = In.ReadString();
            if (buff != "OK")
            {
                return buff;
            }

            int len = 0;
            byte[] intBytes = In.Read(ref len);
            if (intBytes == null)
            {
                return "";
            }
            if (intBytes.Length > 4)
            {
                return "";
            }
            int deviceNum = BitConverter.ToInt32(intBytes);
            return "同步给了" + deviceNum.ToString() + "台设备";
        }
    }
}
