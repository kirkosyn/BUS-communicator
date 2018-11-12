using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Communicator
{
    class ClientSocket
    {
        /// <summary>
        /// Socket clienta
        /// </summary>
        private Socket clientSocket;
        /// <summary>
        /// Tablica bitów wiadomości
        /// </summary>
        private byte[] buffer = new byte[1024];
        /// <summary>
        /// Zakończenie gniazdka
        /// </summary>
        EndPoint remoteEndPoint;
        /// <summary>
        /// Odebrana wiadomość
        /// </summary>
        string receivedMessage = "";

        /// <summary>
        /// Konstruktor socketa clienta
        /// </summary>
        /// <param name="ip">IPAddress</param>
        /// <param name="port">Port</param>
        public ClientSocket(string ip, int port)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            clientSocket.Bind(localEndPoint);

            
        }

        /// <summary>
        /// Zwraca socket clienta
        /// </summary>
        /// <returns>Socket</returns>
        public Socket ReturnSocket()
        {
            return clientSocket;
        }

        /// <summary>
        /// Łączy z socketem
        /// </summary>
        /// <param name="toIp">IPAddress</param>
        /// <param name="toPort">Port</param>
        public void SocketConnect(string toIp, int toPort)
        {
            IPAddress ipAddress = IPAddress.Parse(toIp);
            remoteEndPoint = new IPEndPoint(ipAddress, toPort);
            clientSocket.Connect(remoteEndPoint);
        }

        /// <summary>
        /// Rozpoczyna odbieranie wiadomości
        /// </summary>
        public void Receive()
        {
            buffer = new byte[1024];
            clientSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remoteEndPoint,
                new AsyncCallback(MessageCallback), buffer);
        }

        /// <summary>
        /// Przy zdarzeniu dostania wiadomości pobiera ją
        /// </summary>
        /// <param name="result">zdarzenie</param>
        public void MessageCallback(IAsyncResult result)
        {
            try
            {
                byte[] receivedData = new byte[1024];
                receivedData = (byte[])result.AsyncState;
                ASCIIEncoding encoding = new ASCIIEncoding();
                int i;

                if (receivedData.Length > 0)
                {
                    i = receivedData.Length - 1;
                    while (receivedData[i] == 0)
                    {
                        --i;
                        if (i < 0)
                            break;
                    }
                }
                else
                    i = 0;
                
                byte[] auxtrim = new byte[i + 1];
                Array.Copy(receivedData, auxtrim, i + 1);

                receivedMessage = encoding.GetString(auxtrim);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Zamyka socket
        /// </summary>
        public void SocketClose()
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        /// <summary>
        /// Wysyłanie wiadomości do socketa
        /// </summary>
        /// <param name="data">treść wiadomości</param>
        public void SendMessage(string data)
        {
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] sending = new byte[1024];
            sending = enc.GetBytes(data);
            clientSocket.Send(sending);
        }

        /// <summary>
        /// Zwraca wiadomość, którą socket dostał
        /// </summary>
        /// <returns>wiadomość tekstowa</returns>
        public String ReturnMessage()
        {
            string msg = receivedMessage;
            receivedMessage = "";
            return msg;
        }



        public void Accept()
        {
            clientSocket.Listen(200);
            clientSocket.BeginAccept(AcceptCallback, null);
        }

        public void SendCallback(IAsyncResult result)
        {
            Socket friend = (Socket)result.AsyncState;
            int bytesSent = friend.EndSend(result);
        }

        public void ConnectCallback(IAsyncResult result)
        {
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceivedCallback, null);
        }

        public void AcceptCallback(IAsyncResult result)
        {
            Socket friend = clientSocket.EndAccept(result);
            Accept();
            friend.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceivedCallback, friend);
        }

        public void ReceivedCallback(IAsyncResult result)
        {
            Socket friend = result.AsyncState as Socket;
            int bufferSize = friend.EndReceive(result);
            byte[] packet = new byte[bufferSize];
            Array.Copy(buffer, packet, packet.Length);
            friend.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceivedCallback, friend);

        }

    }
}
