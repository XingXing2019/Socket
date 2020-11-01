using System;
using System.Collections.Generic;
using System.IO;
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
            try
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
                var thread = new Thread(Listen) { IsBackground = true };
                thread.Start(socketWatch);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
                    Console.WriteLine(ex.Message);
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
                    Console.WriteLine(ex.Message);
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

        private void btnSendMsg_Click(object sender, EventArgs e)
        {
            try
            {
                var msg = txtMsg.Text;
                var buffer = Encoding.UTF8.GetBytes(msg);
                var list = new List<byte> { 0 };
                list.AddRange(buffer);
                var newBuffer = list.ToArray();
                var endPoint = cmbClients.SelectedItem.ToString();
                dict[endPoint].Send(newBuffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Select file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                InitialDirectory = @"C:\Users\xxing\OneDrive\Desktop",
                Title = "Please select file",
                Filter = "All|*.*"
            };
            ofd.ShowDialog();

            txtFileName.Text = ofd.FileName;
        }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            var path = txtFileName.Text;
            using (var fsRead = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[1024 * 1024 * 5];
                var read = fsRead.Read(buffer, 0, buffer.Length);
                var list = new List<byte> {1};
                list.AddRange(buffer);
                var newBuffer = list.ToArray();
                var endpoint = cmbClients.SelectedItem.ToString();
                dict[endpoint].Send(newBuffer, 0, read + 1, SocketFlags.None);
            }
        }

        private void btnShake_Click(object sender, EventArgs e)
        {
            try
            {
                var buffer = new byte[1];
                buffer[0] = 2;

                var endpoint = cmbClients.SelectedItem.ToString();
                dict[endpoint].Send(buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
