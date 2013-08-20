using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace TestServerPulsar
{
    public partial class MainForm : Form
    {
        private delegate void DConnect();

        private delegate void DListen();

        private Socket _serverSocket;
        private Socket _client;
        private EndPoint _end;
        private byte[] _buffer;
        private bool _connection;
        private string _ipAddress = "192.168.0.100";

        public MainForm()
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
                var ipe = new IPEndPoint(ip, 5500);
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
                    AddMessage(ex.Message);
                }
            }
        }

        private void GetMessage()
        {
            _buffer = new byte[1000];
            _client.ReceiveFrom(_buffer, ref _end);
            AddMessage(Encoding.Default.GetString(_buffer));
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
            SendQueryGetTime();
        }

        private void SendQueryGetTime()
        {
            byte[] commandIdByte = { 0x21, 0x22 };
            const byte commandGetTimeCode = 0x04;
            const byte commandGetTimeLength = 0x0A;
            SendQuery(commandGetTimeCode, commandGetTimeLength, commandIdByte, null);
        }

        private void uiSendToPulsarButton2_Click(object sender, EventArgs e)
        {
            SendQuerySetTime();
        }

        private void SendQuerySetTime()
        {
            byte[] commandIdByte = { 0x33, 0x34 };
            const byte commandSetTimeCode = 0x05;
            const byte commandSetTimeLength = 0x10;
            byte[] bodyByte = GetDateBytes();
            SendQuery(commandSetTimeCode, commandSetTimeLength, commandIdByte, bodyByte);
        }

        private byte[] GetDateBytes()
        {
            var y = DateTime.Now.ToString("yy");
            var year = BitConverter.GetBytes(Convert.ToInt32(y));
            var month = BitConverter.GetBytes(DateTime.Now.Month);
            var day = BitConverter.GetBytes(DateTime.Now.Day);
            var hour = BitConverter.GetBytes(DateTime.Now.Hour);
            var minute = BitConverter.GetBytes(DateTime.Now.Minute);
            var second = BitConverter.GetBytes(DateTime.Now.Second);
            byte[] dateBytes = {year[0], month[0], day[0], hour[0], minute[0], second[0]};
            return dateBytes;
        }

        private void SendQuery(byte commandGetTimeCode, byte commandGetTimeLength, byte[] commandIdByte, byte[] bodyByte)
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
                SendToPulsar(commandGetTimeCode, commandGetTimeLength, ipAddressBytes, commandIdByte, bodyByte);
            }
            else
            {
                AddMessage("для отправки нужно устанвоить соединение");
            }
        }

        private void SendToPulsar(byte commandCode, byte commandLength, byte[] ipAddressBytes, byte[] commandIdByte, byte[] bodyBytes)
        {              
            byte[] bytesForCrc =
                {
                    ipAddressBytes[0], ipAddressBytes[1], ipAddressBytes[2], ipAddressBytes[3],
                    commandCode,
                    commandLength
                };

            if(bodyBytes != null)
            {
                byte[] newArray = new byte[bytesForCrc.Length + bodyBytes.Length];
                bytesForCrc.CopyTo(newArray, 0);
                for (int i = 0; i < bodyBytes.Length; i++)
                {
                    newArray[bytesForCrc.Length + i] = bodyBytes[i];
                }
                bytesForCrc = newArray;
            }

             byte[] newArray2 = new byte[bytesForCrc.Length + 2];
             bytesForCrc.CopyTo(newArray2, 0);
            newArray2[bytesForCrc.Length + 0] = commandIdByte[0];
            newArray2[bytesForCrc.Length + 1] = commandIdByte[1];
            bytesForCrc = newArray2;
            var crc16Uint = crc16converter.GetCrc16(bytesForCrc);
            var crc16Bytes = BitConverter.GetBytes(crc16Uint);

            byte[] allBytes = new byte[bytesForCrc.Length + 2];
            bytesForCrc.CopyTo(allBytes, 0);
            allBytes[bytesForCrc.Length + 0] = crc16Bytes[0];
            allBytes[bytesForCrc.Length + 1] = crc16Bytes[1];
            AddMessage("отправлено сообщение: " + BitConverter.ToString(allBytes));
            
            _client.Send(allBytes, allBytes.Length, 0);
        }

        private void AddMessage(string txt)
        {
            DateTime time = DateTime.Now;
            const string format = "HH:mm:ss";
            uiMessageTextBox.Text += time.ToString(format)+ ": " + txt + Environment.NewLine;
        }
    }
}
