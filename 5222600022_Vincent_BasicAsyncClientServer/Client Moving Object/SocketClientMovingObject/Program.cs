using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace ClientMovingObject
{
    public class ClientForm : Form
    {
        private Socket sender;
        private Rectangle rect = new Rectangle(20, 20, 30, 30);
        private SolidBrush fillBlue = new SolidBrush(Color.Blue);
        private System.Windows.Forms.Timer timer1; 

        public ClientForm()
        {
            this.Text = "Client Moving Object";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;

            SetupClient();
        }

        private void SetupClient()
        {
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);

                sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(localEndPoint);

                timer1 = new System.Windows.Forms.Timer(); 
                timer1.Interval = 50; 
                timer1.Tick += new EventHandler(ReceiveData);
                timer1.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error setting up client: " + e.Message);
            }
        }

        private void ReceiveData(object sender, EventArgs e)
        {
            try
            {
                byte[] messageReceived = new byte[1024];

                int byteRecv = this.sender.Receive(messageReceived);
                string data = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);

                string[] coordinates = data.Split(':');
                if (coordinates.Length == 2)
                {
                    int x = int.Parse(coordinates[0]);
                    int y = int.Parse(coordinates[1]);

                    rect.X = x;
                    rect.Y = y;

                    this.Invalidate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error receiving data: " + ex.Message);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.FillRectangle(fillBlue, rect); 
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ClientForm());
        }
    }
}