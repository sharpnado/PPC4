using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace PPC
{
    public class ClientClass
    {
        private TcpClient client;
        private IPAddress ipAddress;
        private Int32 portNum;

        private StreamReader sReader;
        private StreamWriter sWriter;
        public NetworkStream stream;
        

        

        public ClientClass(IPAddress ipAddress, int portNum)
        {
            this.ipAddress = ipAddress;
            this.portNum = portNum;
            client = new TcpClient();
        }

        public bool connect()
        {
            try
            {
                client.Connect(ipAddress, portNum);

                this.sReader = new StreamReader(client.GetStream(), Encoding.UTF8);
                this.sWriter = new StreamWriter(client.GetStream(), Encoding.UTF8);
                this.stream = client.GetStream();
                return true;
            }
            catch (Exception sockEx)
            {
                ParametersErrorWindow errConn = new ParametersErrorWindow(false, "Errore Connessione");
                errConn.ShowDialog();
                client.Close();
                return false;
            }
        }

       public void write(String parameter)
        {
            this.sWriter.WriteLine(parameter);
            this.sWriter.Flush();
        }
        public String read()
        {
           String result = sReader.ReadLine();
           return result; 
        }
        

        public void disconnect()
        {
            this.client.Close();
        }
      

        
    }
}