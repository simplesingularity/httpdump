using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace httpdump
{

    /// <summary>
    /// designed to use with following curl command:
    /// curl --data-binary "@./world.zip" http://192.168.1.27:80/
    /// where the file.zip is your file, and the ip is your machine ip
    /// useful for transporting files out of running docker containers
    /// </summary>
    internal class Program
    {
        static StreamWriter logger;
        static TcpListener listener;
        static Queue<string> FileIncoming;
        static void Main(string[] args)
        {
            FileIncoming = new Queue<string>();

            listener = new TcpListener(new IPEndPoint(IPAddress.Any, 80));
            listener.Start();
            listener.BeginAcceptTcpClient(new AsyncCallback(EndAcceptTcpClient), listener);
            Console.WriteLine("Accepting any POST file from any one source...");
            Console.WriteLine("Press any key to stop");
            Console.ReadLine();

            listener.Stop();

        }

        static void EndAcceptTcpClient(IAsyncResult ar)
        {
            TcpClient client = listener.EndAcceptTcpClient(ar);
            Console.WriteLine("incoming request from: {0}", client.Client.RemoteEndPoint.ToString());
            listener.BeginAcceptTcpClient(new AsyncCallback(EndAcceptTcpClient), listener);

            DumpData(client);

            client.Close();

        }

        static void DumpData(TcpClient client)
        {
            List<string> headers = new List<string>();

            Encoding web = new UTF8Encoding(false, false);
            StreamReader r = new StreamReader(client.GetStream(), web);
            string line = "";
            string length = "";
            while ((line = r.ReadLine()) != "")
            {
                if (line.StartsWith("Content-Length: "))
                {
                    length = line.Substring(line.IndexOf(": ") + 2);
                }
                Console.WriteLine("Debug: {0}", line);
                headers.Add(line);
            }

            Console.WriteLine("End of text request");

            if (headers.Any(e => e.StartsWith("POST")) && int.Parse(length) > 0)
            {
                long unixTime = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
                FileStream fs = File.Create(string.Format("data_{0}.dat", unixTime));

                GiveContinue(client);

                Console.WriteLine("Expecting {0}", int.Parse(length));

                byte[] buff = new byte[client.ReceiveBufferSize];
                int rd = 0;
                int tot = 0;
                while ((rd = client.GetStream().Read(buff, 0, buff.Length)) > 0)
                {
                    fs.Write(buff, 0, rd);
                    fs.Flush();
                    tot += rd;
                    Console.WriteLine("Read: {0}", tot);
                    if (tot == int.Parse(length))
                    {
                        Console.WriteLine("Received file: {0}", ((long)tot).ToSize(MyExtension.SizeUnits.MB));
                        break;
                    }
                }
                fs.Flush();
                fs.Close();

                GiveOK(client);
       
            } else
            {
                GiveOK(client);
            }


        }

        static void GiveContinue(TcpClient client)
        {
            byte[] buffer = new UTF8Encoding(false, false).GetBytes(Properties.Resources._continue);
            client.GetStream().Write(buffer, 0, buffer.Length);
            client.GetStream().Flush();
        }

        static void GiveOK(TcpClient client)
        {
            byte[] buffer = new UTF8Encoding(false, false).GetBytes(Properties.Resources.ok);
            client.GetStream().Write(buffer, 0, buffer.Length);
            client.GetStream().Flush();
        }
    }
    public static class MyExtension
    {
        public enum SizeUnits
        {
            Byte, KB, MB, GB, TB, PB, EB, ZB, YB
        }

        public static string ToSize(this Int64 value, SizeUnits unit)
        {
            return (value / (double)Math.Pow(1024, (Int64)unit)).ToString("0.00");
        }
    }
}
