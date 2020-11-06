using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class ClientForm : Form
    {
        private Socket socketSend;
        public ClientForm()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ipAddress = IPAddress.Parse(txtServer.Text);
                var point = new IPEndPoint(ipAddress, Convert.ToInt32(txtPort.Text));
                socketSend.Connect(point);
                ShowMsg("Success Connect");
                Task.Run(Receive);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void ShowMsg(string msg)
        {
            txtLog.AppendText(msg + "\r\n");
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                var msg = txtMsg.Text.Trim();
                var buffer = Encoding.UTF8.GetBytes(msg);
                socketSend.Send(buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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

                    if (buffer[0] == 0)
                    {
                        var msg = Encoding.UTF8.GetString(buffer, 1, receive - 1);
                        ShowMsg($"{socketSend.RemoteEndPoint} : {msg}");
                    }
                    else if (buffer[0] == 1)
                    {
                        var sfd = new SaveFileDialog
                        {
                            InitialDirectory = @"C:\Users\xxing\OneDrive\Desktop",
                            Title = "Please select file",
                            Filter = "All|*.*"
                        };
                        sfd.ShowDialog(this);
                        var path = sfd.FileName;
                        using (var fsWrite = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            fsWrite.Write(buffer, 1, receive - 1);
                        }

                        MessageBox.Show("Success");
                    }
                    else if (buffer[0] == 2)
                    {
                        Shake();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void Shake()
        {
            for (int i = 0; i < 1000; i++)
            {
                this.Location = new Point(200, 200);
                this.Location = new Point(205, 200);
            }
        }
    }
}
