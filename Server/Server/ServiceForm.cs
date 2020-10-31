using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    public partial class ServiceForm : Form
    {
        private Dictionary<string, Socket> dict;
        public ServiceForm()
        {
            InitializeComponent();
            dict = new Dictionary<string, Socket>();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // Create listening socket
            var socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Create Ip address 
            var ip = IPAddress.Parse(txtServer.Text);
            // Create Port
            var point = new IPEndPoint(ip, Convert.ToInt32(txtPort.Text));
            // Bind listen socket to Ip address and port
            socketWatch.Bind(point);
            ShowMsg("Success");
            // Create listening queue
            socketWatch.Listen(10);

            // Run Listen in new thread
            var thread = new Thread(Listen) {IsBackground = true};
            thread.Start(socketWatch);
        }

        /// <summary>
        /// Wait for client connect and create its socket for communicating
        /// </summary>
        void Listen(object obj)
        {
            var socketWatch = obj as Socket;
            if(socketWatch == null) return;

            while (true)
            {
                try
                {
                    var socketSend = socketWatch.Accept();
                    dict[socketSend.RemoteEndPoint.ToString()] = socketSend;
                    cmbClients.Items.Add(socketSend.RemoteEndPoint.ToString());
                    ShowMsg($"{socketSend.RemoteEndPoint} Connect Success");
                    var thread = new Thread(Receive) {IsBackground = true};
                    thread.Start(socketSend);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        void Receive(object obj)
        {
            var socketSend = obj as Socket;
            if(socketSend == null) return;

            while (true)
            {
                try
                {
                    var buffer = new byte[1024 * 1024 * 2];
                    int receive = socketSend.Receive(buffer);
                    if (receive == 0) break;
                    var msg = Encoding.UTF8.GetString(buffer, 0, receive);
                    ShowMsg($"{socketSend.RemoteEndPoint} : {msg}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        void ShowMsg(string msg)
        {
            txtLog.AppendText(msg + "\r\n");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            var msg = txtMsg.Text;
            var buffer = Encoding.UTF8.GetBytes(msg);

            var endPoint = cmbClients.SelectedItem.ToString();
            dict[endPoint].Send(buffer);
        }
    }
}
