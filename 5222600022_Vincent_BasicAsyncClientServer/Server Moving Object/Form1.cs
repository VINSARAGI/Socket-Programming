using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace MovingObject/
{
    public partial class Form1 : Form
    {
        Pen red = new Pen(Color.Red);
        Rectangle rect = new Rectangle(20, 20, 30, 30);
        SolidBrush fillBlue = new SolidBrush(Color.Blue);
        int slide = 10;

        private Socket listenerSocket;
        private List<Socket> clientSockets = new List<Socket>(); 

        public Form1()
        {
            InitializeComponent();
            timer1.Interval = 50;
            timer1.Enabled = true;

            SetupServerSocket();
        }

        private void SetupServerSocket()
        {
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);

                listenerSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listenerSocket.Bind(localEndPoint);
                listenerSocket.Listen(10);

                listenerSocket.BeginAccept(new AsyncCallback(AcceptCallback), listenerSocket);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting up server: " + ex.Message);
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket listener = (Socket)ar.AsyncState;
                Socket clientSocket = listener.EndAccept(ar);

                lock (clientSockets)
                {
                    clientSockets.Add(clientSocket);
                }

                MessageBox.Show("Client connected!");

                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error accepting client: " + ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            back();

            rect.X += slide;
            Invalidate(); 

            lock (clientSockets)
            {
                foreach (Socket clientSocket in clientSockets)/
                {
                    if (clientSocket.Connected)
                    {
                        try
                        {
                            string dataToSend = rect.X + ":" + rect.Y;
                            byte[] message = Encoding.ASCII.GetBytes(dataToSend);
                            clientSocket.Send(message);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error sending data to client: " + ex.Message);
                        }
                    }
                }
            }
        }

        private void back()
        {
            if (rect.X >= this.Width - rect.Width * 2)
                slide = -10;
            else if (rect.X <= rect.Width / 2)
                slide = 10;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawRectangle(red, rect);
            g.FillRectangle(fillBlue, rect);
        }
    }
}
