using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace tongsingo
{
    public partial class Form1 : Form
    {
        private string serverIP = "127.0.0.1"; // 서버의 IP 주소
        private string clientIP = "127.0.0.1";
        private int serverPort = 12345; // 서버의 포트 번호
        private int clientPort = 12346; // 클라이언트의 포트 번호

        private Thread listenThread;
        private Socket clientSocket;
        private Socket serverSocket;

        public Form1()
        {
            InitializeComponent();
        }

        private void Log(string msg)
        {
            string timeString = DateTime.Now.ToString("HH:mm");
            if (richTextBox1.InvokeRequired)
            { 
                richTextBox1.Invoke(new Action( () => richTextBox1.SelectionAlignment = HorizontalAlignment.Left));
                richTextBox1.Invoke(new Action(() => richTextBox1.AppendText(string.Format("{0}  [{1}]", msg, timeString) + "\r\n")));
            }
            else
            {
                richTextBox1.SelectionAlignment = HorizontalAlignment.Left;
                richTextBox1.AppendText(string.Format("{0}  [{1}]", msg, timeString) + "\r\n");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listenThread = new Thread(new ThreadStart(Listen));
            listenThread.IsBackground = true;
            listenThread.Start();
        }

        private void ConnectToServer()
        {
            try
            {
                IPAddress serverIPAddress = IPAddress.Parse(clientIP);
                IPEndPoint serverEndPoint = new IPEndPoint(serverIPAddress, clientPort);

                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(serverEndPoint);
                Log("서버에 연결되었습니다.");
            }
            catch (Exception ex)
            {
                Log("서버 연결 중 오류 발생: " + ex.Message);
            }
        }

        private void Listen()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, serverPort);

            serverSocket.Bind(localEndPoint);
            serverSocket.Listen(10);

            Log("클라이언트 요청 대기중...");

            while (true)
            {
                Socket connectedClientSocket = serverSocket.Accept();
                Log("클라이언트 접속됨");

                Thread receiveThread = new Thread(new ParameterizedThreadStart(Receive));
                receiveThread.IsBackground = true;
                receiveThread.Start(connectedClientSocket);
            }
        }


        private void Receive(object clientSocket)
        {
            Socket connectedClientSocket = (Socket)clientSocket;

            while (true)
            {
                byte[] receiveBuffer = new byte[512];
                int length = connectedClientSocket.Receive(receiveBuffer, receiveBuffer.Length, SocketFlags.None);

                string msg = Encoding.UTF8.GetString(receiveBuffer, 0, length);

                Log("상대: " + msg);
            }
        }

        private void SendToClient(string message)
        {
            try
            {
                byte[] sendBuffer = Encoding.UTF8.GetBytes(message);
                clientSocket.Send(sendBuffer);
                mesend("나: " + message);
            }
            catch (Exception ex)
            {
                Log("메시지 전송 중 오류 발생: " + ex.Message);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendToClient(textBox1.Text.Trim());
                textBox1.Text = "";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                SendToClient(textBox1.Text.Trim());
                textBox1.Text = "";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ConnectToServer();
        }

        private void mesend(string msg)
        {
            string timeString = DateTime.Now.ToString("HH:mm");
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new Action(() => richTextBox1.SelectionAlignment = HorizontalAlignment.Right));
                richTextBox1.Invoke(new Action(() => richTextBox1.AppendText(string.Format("[{0}]  {1}", timeString, msg) + "\r\n")));
            }
            else
            {
                richTextBox1.SelectionAlignment = HorizontalAlignment.Right;
                richTextBox1.AppendText(string.Format("[{0}]  {1}", timeString, msg) + "\r\n");
            }
        }
    }
}
