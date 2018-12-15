using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Communicator
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Socket pomocniczy
        /// </summary>
        Socket mySocket;
        /// <summary>
        /// Socket clienta
        /// </summary>
        ClientSocket client;
        /// <summary>
        /// Zakończenie gniazdka
        /// </summary>
        EndPoint endRemote;
        /// <summary>
        /// Tablica bitów wiadomości
        /// </summary>
        byte[] buffer;

        /// <summary>
        /// Czy można już odbierać wiadomości
        /// </summary>
        private bool receive = false;
        /// <summary>
        /// Czy odebrano klucze
        /// </summary>
        private bool gotKeys = false;
        /// <summary>
        /// Czy wysłano klucze
        /// </summary>
        private bool sentKeys = false;
        /// <summary>
        /// Czy wysłano wyznaczone liczby: pierwszą oraz pierwiastek pierwotny
        /// </summary>
        private bool sentNumber = false;
        /// <summary>
        /// Czy odebrano podpis
        /// </summary>
        private bool gotSignature = false;

        /// <summary>
        /// Konstruktor okna
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Wysłanie wiadomości po kliknięciu entera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                string sending = Message.Text;

                client.SendMessage(sending, 0);
                lock (List)
                {
                    List.AppendText("You: " + Message.Text + "\n");
                    List.SelectionStart = List.Text.Length;
                    List.ScrollToEnd();
                }

                Message.Clear();
            }
        }

        /// <summary>
        /// Utworzenie socketu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetMySocket_Click(object sender, RoutedEventArgs e)
        {
            string myIp;
            int myPort;
            myIp = myIP.Text;
            myPort = Int32.Parse(myPORT.Text);

            client = new ClientSocket(myIp, myPort);
            mySocket = client.ReturnSocket();
            AddLogs("Modulo: " + client.sign.ownPrivKey.ElementAt(0) + "\n" +
                "Wykladnik: " + client.sign.ownPrivKey.ElementAt(1) + "\n" +
                "P: " + client.sign.ownPrivKey.ElementAt(2) + "\n" +
                "Q: " + client.sign.ownPrivKey.ElementAt(3) + "\n" +
                "DP: " + client.sign.ownPrivKey.ElementAt(4) + "\n" +
                "DQ: " + client.sign.ownPrivKey.ElementAt(5) + "\n" +
                "InverseQ: " + client.sign.ownPrivKey.ElementAt(6) + "\n" +
                "D: " + client.sign.ownPrivKey.ElementAt(7) + "\n", 1);
            AddLogs("Modulo: " + client.sign.ownPubKey.Item1 +
                "\n" + "Wykladnik: " + client.sign.ownPubKey.Item2, 2);

            MessageBox.Show("Socket set.");

        }

        /// <summary>
        /// Łączenie z socketem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SocketConnect_Click(object sender, RoutedEventArgs e)
        {
            string toIp;
            int toPort;
            toPort = Int32.Parse(toPORT.Text);
            toIp = toIP.Text;
            endRemote = new IPEndPoint(IPAddress.Parse(toIp), toPort);

            client.SocketConnect(toIp, toPort);

            buffer = new byte[1024];
            receive = true;
           

            mySocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endRemote,
               new AsyncCallback(MessageCallback), buffer);

            MessageBox.Show("Connected.");
        }

        /// <summary>
        /// Wysyłanie wiadomości i wypisanie do chatu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendMessage(object sender, RoutedEventArgs e)
        {
            /*string sending = Message.Text;
            //Tuple<byte[], byte[]> tuple = Tuple.Create(client.ExchangeKeysMsg(client.sign).Item1, client.ExchangeKeysMsg(client.sign).Item2);
            //ASCIIEncoding myAscii = new ASCIIEncoding();

            lock (List)
            {
                List.AppendText("You: " + sending + "\n");
                List.SelectionStart = List.Text.Length;
                List.ScrollToEnd();
            }
            //Convert.ToBase64String(tuple.Item1) 
            Message.Clear();*/
            //client.SendMessage(sending, 0);

            if (!sentKeys)
            {
                client.SendMessage("", 1);
                client.SendMessage("", 2);

                client.SendMessage("", 8);
                client.SendMessage("", 9);
                client.SendMessage("", 10);
                client.SendMessage("", 11);
                client.SendMessage("", 12);
                client.SendMessage("", 13);


                client.SendMessage("", 5);
                client.SendMessage("", 6);
                sentKeys = true;
            }
            else if (!sentNumber && sentKeys)
            {
                client.SendMessage("", 7);
                sentNumber = true;
            }
            else if (gotKeys && !gotSignature)
            {
                client.SendMessage("", 3);
                client.SendMessage("", 4);
            }
                

        }

        /// <summary>
        /// Zamyka socket
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SocketDisconnect_Click(object sender, RoutedEventArgs e)
        {
            receive = false;
            client.SocketClose();
            MessageBox.Show("Disconnected.");
        }

        /// <summary>
        /// Przy zdarzeniu odebrania wiadomości pobiera ją, wypisuje do chatu bądź do obszaru logów
        /// </summary>
        /// <param name="result">zdarzenie</param>
        private void MessageCallback(IAsyncResult result)
        {
            if (receive)
            {
                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        client.MessageCallback(result);

                        string msg = client.ReturnMessage();
                        char firstLetter = msg.ElementAt(0);
                        string msgCut = msg.Substring(1, msg.Length - 1);


                        switch (firstLetter)
                        {
                            case 'M':
                                client.sign.SetClientModulus(msgCut);
                                AddLogs(msg.Substring(1, msg.Length-1), 3);
                                break;
                            case 'E':
                                client.sign.SetClientExponent(msgCut);
                                AddLogs(msgCut, 4);
                                break;
                            case 'D':
                                lock (List)
                                {
                                    List.AppendText("Friend: " + msg + "\n");
                                    List.SelectionStart = List.Text.Length;
                                    List.ScrollToEnd();
                                }
                                break;
                            case 'K':
                                client.SetEncryptMsg(msgCut);
                                AddLogs(Convert.ToBase64String(client.encryptMsg) + "\n\n" + msgCut, 5);
                                break;
                            case 'S':
                                gotSignature = true;
                                client.SetEncryptSig(msgCut);
                                AddLogs(msgCut, 6);
                                AddLogs(client.VerifyMsg(), 10);
                                break;
                            case 'P':
                                client.protocol.SetPrimeNumber(Int32.Parse(msgCut));
                                AddLogs(msgCut, 7);
                                break;
                            case 'R':
                                client.protocol.SetPrimitiveRoot(Int32.Parse(msgCut));
                                AddLogs(msgCut, 8);
                                break;
                            case 'B':
                                client.protocol.SetReceivedNumber(msgCut);
                                client.protocol.CalculateReceivedNumber();
                                AddLogs(String.Concat(msgCut, " ", client.protocol.GetSecretKey()), 9);
                                break;
                            case 'Q':
                                client.sign.SetClientQ(msgCut);
                                AddLogs(msgCut, 11);
                                break;
                            case 'T':
                                client.sign.SetClientP(msgCut);
                                AddLogs(msgCut, 12);
                                break;
                            case 'U':
                                client.sign.SetClientDP(msgCut);
                                AddLogs(msgCut, 13);
                                break;
                            case 'V':
                                client.sign.SetClientDQ(msgCut);
                                AddLogs(msgCut, 14);
                                break;
                            case 'W':
                                client.sign.SetClientInverseQ(msgCut);
                                AddLogs(msgCut, 15);
                                break;
                            case 'X':
                                client.sign.SetClientD(msgCut);
                                AddLogs(msgCut, 16);
                                gotKeys = true;
                                break;
                            default:
                                break;
                        }
                        
                        buffer = new byte[1024];
                        mySocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endRemote,
                            new AsyncCallback(MessageCallback), buffer);
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Dodawanie logów
        /// </summary>
        /// <param name="text">Tekst do wrzucenia do loga</param>
        /// <param name="option">Opcja</param>
        private void AddLogs(string text, int option)
        {
            switch (option)
            {
                case 1:
                    Logs.AppendText("Wygenerowano klucz prywatny RSA\n");
                    break;
                case 2:
                    Logs.AppendText("Wygenerowano klucz publiczny RSA\n");
                    break;
                case 3:
                    Logs.AppendText("Odebrano klucz klienta modulo\n");
                    break;
                case 4:
                    Logs.AppendText("Odebrano klucz klienta wykładnik\n");
                    break;
                case 5:
                    Logs.AppendText("Odebrano podpisaną wiadomość\n");
                    break;
                case 6:
                    Logs.AppendText("Odebrano sygnaturę podpisu\n");
                    break;
                case 7:
                    Logs.AppendText("Odebrano losową liczbę pierwszą\n");
                    break;
                case 8:
                    Logs.AppendText("Odebrano pierwiastek pierwotny\n");
                    break;
                case 9:
                    Logs.AppendText("Odebrano liczbę do obliczenia tajnego klucza oraz wyliczono współdzielony klucz\n");
                    break;
                case 10:
                    Logs.AppendText("Sprawdzono przysłaną wiadomość i sygnaturę\n");
                    break;
                case 11:
                    Logs.AppendText("Odebrano wartość Q\n");
                    break;
                case 12:
                    Logs.AppendText("Odebrano wartość P\n");
                    break;
                case 13:
                    Logs.AppendText("Odebrano wartość DP\n");
                    break;
                case 14:
                    Logs.AppendText("Odebrano wartość DQ\n");
                    break;
                case 15:
                    Logs.AppendText("Odebrano wartość InverseQ\n");
                    break;
                case 16:
                    Logs.AppendText("Odebrano wartość D\n");
                    break;

            }

            Logs.AppendText(text + "\n");
            Logs.SelectionStart = Logs.Text.Length;
            Logs.ScrollToEnd();
        }

    }
}
