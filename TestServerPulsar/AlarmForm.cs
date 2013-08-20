using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace TestServerPulsar
{
    public partial class AlarmForm : Form
    {
        private delegate void DConnect();

        private delegate void DListen();

        private Socket _serverSocket;
        private Socket _client;
        private EndPoint _end;
        private byte[] _buffer;
        private bool _connection;
        private string _ipAddress = "192.168.0.100";

        public AlarmForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            _connection = false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void UiStartServerButton_Click(object sender, EventArgs e)
        {
            StartServer();
        }

        private void StartServer()
        {
            try
            {
                _ipAddress = uiIpAddressTextBox.Text;
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ip = IPAddress.Parse(_ipAddress);
                var ipe = new IPEndPoint(ip, 5501);
                _end = ipe;
                _serverSocket.Bind(ipe);
                _serverSocket.Listen(1);
                new DConnect(Connect).BeginInvoke(null, null);
                AddMessage("сервер запущен");
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }
        }

        private void Connect()
        {
            _client = _serverSocket.Accept();
            var pulsarIp = _client.RemoteEndPoint;

            AddMessage("К нам подсоединились с адреса: " + pulsarIp);

            uiMainNotifyIcon.ShowBalloonTip(1000);
            _connection = true;
            new DListen(Listening).BeginInvoke(null, null);

        }

        private void Listening()
        {
            while (!(_client.Poll(0, SelectMode.SelectRead) && _client.Available == 0))
            {
                try
                {
                    GetMessage();
                }
                catch (Exception ex)
                {
                    uiMessageTextBox.Text += ex.Message;
                    uiMessageTextBox.Text += Environment.NewLine;
                }
            }
        }

        private void GetMessage()
        {
            _buffer = new byte[1000];
            _client.ReceiveFrom(_buffer, ref _end);
            uiMessageTextBox.Text += Encoding.Default.GetString(_buffer);
            uiMessageTextBox.Text += Environment.NewLine;

        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void UiMainNotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void UiSendToPulsarButton_Click(object sender, EventArgs e)
        {
            if (_connection)
            {
                var ip = _client.RemoteEndPoint;
                var b = (ip.ToString().Split(':')[0]).Split('.');
                var i1 = BitConverter.GetBytes(Convert.ToInt16(b[0]));
                var i2 = BitConverter.GetBytes(Convert.ToInt16(b[1]));
                var i3 = BitConverter.GetBytes(Convert.ToInt16(b[2]));
                var i4 = BitConverter.GetBytes(Convert.ToInt16(b[3]));
                byte[] ipAddressBytes = {i1[0], i2[0], i3[0], i4[0]};
                SendToPulsar(ipAddressBytes);
            }
            else
            {
                AddMessage("для отправки нужно устанвоить соединение");
            }
        }

        private void SendToPulsar(byte[] ipAddressBytes)
        {
            byte[] commandIdByte = {0x21, 0x22};
            const byte commandGetTimeCode = 0x04;
            const byte commandGetTimeLength = 0x0A;
            byte[] bytesForCrc =
                {
                    ipAddressBytes[0], ipAddressBytes[1], ipAddressBytes[2], ipAddressBytes[3],
                    commandGetTimeCode,
                    commandGetTimeLength,
                    commandIdByte[0], commandIdByte[1]
                };
            var crc16Uint = crc16converter.GetCrc16(bytesForCrc);
            var crc16Bytes = BitConverter.GetBytes(crc16Uint);
            byte[] allBytes =
                {
                    ipAddressBytes[0], ipAddressBytes[1], ipAddressBytes[2], ipAddressBytes[3],
                    commandGetTimeCode,
                    commandGetTimeLength,
                    commandIdByte[0],
                    commandIdByte[1],
                    crc16Bytes[0], crc16Bytes[1]
                };
            AddMessage("отправлено сообщение:");
            AddMessage(BitConverter.ToString(allBytes));
            _client.Send(allBytes, allBytes.Length, 0);
        }

        private void AddMessage(string txt)
        {
            DateTime time = DateTime.Now;              // Use current time
            string format = "HH:mm:ss";    // Use this format
            Console.WriteLine();
            uiMessageTextBox.Text = time.ToString(format)+ ": " + txt + Environment.NewLine + uiMessageTextBox.Text;
        }
    }
}
