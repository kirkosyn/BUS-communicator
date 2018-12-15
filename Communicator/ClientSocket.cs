using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
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
        private byte[] buffer = new byte[2048];
        /// <summary>
        /// Zakończenie gniazdka
        /// </summary>
        EndPoint remoteEndPoint;
        /// <summary>
        /// Odebrana wiadomość
        /// </summary>
        string receivedMessage = "";

        /// <summary>
        /// Zaszyfrowana wiadomość oraz podpis - do wysłania
        /// </summary>
        public Tuple<byte[], byte[]> signed;

        /// <summary>
        /// Zaszyfrowana odebrana wiadomość
        /// </summary>
        public byte[] encryptMsg;
        /// <summary>
        /// Odebrany podpis
        /// </summary>
        private byte[] encryptSig;
        /// <summary>
        /// Klucze klienta
        /// </summary>
        RSAParameters clientSign;

        /// <summary>
        /// Obiekt klasy do podpisu cyfrowego
        /// </summary>
        public SignProgram sign;
        /// <summary>
        /// Obiekt klasy do uzgodnienia kluczy (Diffie-Hellman)
        /// </summary>
        public Protocol protocol;
        //public RSAParameters PublicParameters;

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

            sign = new SignProgram();
            protocol = new Protocol();
            
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
                //ASCIIEncoding encoding = new ASCIIEncoding();
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

                receivedMessage = Encoding.UTF8.GetString(auxtrim);
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
        /// Wysyłanie wiadomości do socketa z pomocą opcji
        /// </summary>
        /// <param name="data">treść wiadomości</param>
        /// <param name="option">opcja wiadomości</param>
        /* możliwe opcje:
            1 - wyślij wartość modulo
            2 - wyślij wartość exponenty
            3 - wyślij zaszyfrowaną wiadomość
            4 - wyślij podpis
            5 - wyślij liczbę pierwszą
            6 - wyślij pierwiastek pierwotny
            7 - wyślij wyznaczoną wartość, wyliczoną za pomocą tajnej liczby własnej (g^t mod p)
            8 - wyślij Q
            9 - wyślij P
            10 - wyślij DP
            11 - wyślij DQ
            12 - wyślij InverseQ
            13 - wyślij D
             */
        public void SendMessage(string data, int option)
        {
            //ASCIIEncoding enc = new ASCIIEncoding();
            string sending;
            string msgOption;
            byte[] endMsg = new byte[1024];

            switch (option)
            {
                case 1:
                    msgOption = "M";
                    sending = sign.ownPubKey.Item1;
                    break;
                case 2:
                    msgOption = "E";
                    sending = sign.ownPubKey.Item2;
                    break;
                case 3:
                    msgOption = "K";
                    //clientSign = sign.GetClientPublicKeys();
                    ExchangeKeysMsg();
                    sending = Convert.ToBase64String(signed.Item1);
                    break;
                case 4:
                    msgOption = "S";
                    //ExchangeKeysMsg(sign.GetClientKeys());
                    sending = Convert.ToBase64String(signed.Item2);
                    break;
                case 5:
                    msgOption = "P";
                    sending = protocol.GetPrimeNumber().ToString();
                    break;
                case 6:
                    msgOption = "R";
                    sending = protocol.GetPrimitiveRoot().ToString();
                    break;
                case 7:
                    msgOption = "B";
                    protocol.CreateNumberToSend();
                    sending = protocol.GetNumberToSend();
                    break;
                case 8:
                    msgOption = "Q"; //q
                    sending = sign.ownPrivKey.ElementAt(3);
                    break;
                case 9:
                    msgOption = "T"; //p
                    sending = sign.ownPrivKey.ElementAt(2);
                    break;
                case 10:
                    msgOption = "U"; //dp
                    sending = sign.ownPrivKey.ElementAt(4);
                    break;
                case 11:
                    msgOption = "V"; //dq
                    sending = sign.ownPrivKey.ElementAt(5);
                    break;
                case 12:
                    msgOption = "W"; //inverseq
                    sending = sign.ownPrivKey.ElementAt(6);
                    break;
                case 13:
                    msgOption = "X"; //d
                    sending = sign.ownPrivKey.ElementAt(7);
                    break;
                default:
                    msgOption = "D";
                    sending = data;
                    break;
            }
            
            endMsg = Encoding.UTF8.GetBytes(String.Concat(msgOption, sending));
            clientSocket.Send(endMsg);
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

        /// <summary>
        /// Tworzy zaszyfrowane klucze do wysłania
        /// </summary>
        /// <param name="clientSign">Klucze klienta</param>
        //RSAParameters clientSign
        public void ExchangeKeysMsg()
        {
            byte[] toEncrypt;
            byte[] encrypted;
            byte[] signature;

            //string original = String.Concat(protocol.GetReceivedNumber());
            string original = "hello";
            //ASCIIEncoding myAscii = new ASCIIEncoding();

            //signature = sign.HashSign(Encoding.UTF8.GetBytes(original));

            toEncrypt = Encoding.UTF8.GetBytes(original);

            clientSign = sign.GetClientPublicKeys();
            encrypted = sign.EncryptData(clientSign, toEncrypt);
            signature = sign.HashSign(encrypted);

            //sign.VerifyHash(clientSign, encrypted, signature).ToString();


            signed = Tuple.Create(encrypted, signature);

            //return tuple;
        }

        /// <summary>
        /// Weryfikacja wiadomości
        /// </summary>
        /// <returns>wartość prawda lub fałsz jako string</returns>
        public string VerifyMsg()
        {
            byte[] encrypted = encryptMsg;
            byte[] signature = encryptSig;
            clientSign = sign.GetClientPublicKeys();
            //if (sign.VerifyHash(clientSign, encrypted, signature))
            //{
            //MessageBox.Show(sign.DecryptData(encrypted));

            return sign.VerifyHash(clientSign, encrypted, signature).ToString();
            //return sign.DecryptData(encrypted);
            //}
            //else
            //{
                //MessageBox.Show("Invalid");
             //   return "Invalid";
            //}
        }

        /// <summary>
        /// Ustawienie zaszyfrowanej wiadomości, która została odebrana
        /// </summary>
        /// <param name="msg">tekst</param>
        public void SetEncryptMsg(string msg)
        {
            encryptMsg = Encoding.UTF8.GetBytes(msg);
        }

        /// <summary>
        /// Ustawienie podpisu, który został odebrany
        /// </summary>
        /// <param name="msg"></param>
        public void SetEncryptSig(string msg)
        {
            encryptSig = Encoding.UTF8.GetBytes(msg);
        }




        //możliwe że do usunięcia
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
