using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
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

                client.SendMessage(sending);
                lock (List)
                {
                    List.Items.Add("You: " + Message.Text);
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
            string sending = Message.Text;

            lock (List)
            {
                List.Items.Add("You: " + Message.Text);
            }
            
            Message.Clear();
            client.SendMessage(sending);
        }

        /// <summary>
        /// Zamyka socket
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SocketDisconnect_Click(object sender, RoutedEventArgs e)
        {
            client.SocketClose();
            MessageBox.Show("Disconnected.");
        }

        /// <summary>
        /// Przy zdarzeniu odebrania wiadomości pobiera ją i wypisuje do chatu
        /// </summary>
        /// <param name="result">zdarzenie</param>
        private void MessageCallback(IAsyncResult result)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    client.MessageCallback(result);

                    lock (List)
                    {
                        List.Items.Add("Friend: " + client.ReturnMessage());
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
}
