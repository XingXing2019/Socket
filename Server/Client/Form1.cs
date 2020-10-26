using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        private Socket socketSend;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ipAddress = IPAddress.Parse(txtServer.Text);
            var point = new IPEndPoint(ipAddress, Convert.ToInt32(txtPort.Text));
            socketSend.Connect(point);
            ShowMsg("Success Connect");

            var thread = new Thread(Receive) {IsBackground = true};
            thread.Start();
        }

        void ShowMsg(string msg)
        {
            txtLog.AppendText(msg + "\r\n");
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            var msg = txtMsg.Text.Trim();
            var buffer = Encoding.UTF8.GetBytes(msg);
            socketSend.Send(buffer);
        }

        void Receive()
        {
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

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }
    }
}
