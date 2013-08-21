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
            const byte commandGetTimeCode = 0x04;
            const byte commandGetTimeLength = 0x0A;
            SendQuery(commandGetTimeCode, commandGetTimeLength, null);
        }

        private void uiSendSetTimeToPulsarButton_Click(object sender, EventArgs e)
        {
            SendQuerySetTime();
        }

        private void SendQuerySetTime()
        {
            const byte commandSetTimeCode = 0x05;
            const byte commandSetTimeLength = 0x10;
            byte[] bodyByte = GetDateBytes();
            SendQuery(commandSetTimeCode, commandSetTimeLength, bodyByte);
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

        private void uiSendCustomCommandToPulsarButton_Click(object sender, EventArgs e)
        {
            SendCustomCommand();
        }

        private void SendCustomCommand()
        {
            if (_connection)
            {
                try
                {
                    byte[] customBytes = GetCustomBytes();
                    AddMessage("отправлено сообщение: " + BitConverter.ToString(customBytes));
                    _client.Send(customBytes, customBytes.Length, 0);
                }
                catch (Exception)
                {
                    AddMessage("ошибка преобразования команды");
                }
            }
            else
            {
                AddMessage("для отправки нужно устанвоить соединение");
            }
        }

        private byte[] GetCustomBytes()
        {
            var parts = uiCommandTextBox.Text.Split(' ');
            var newArray = new byte[parts.Length];
            for (int i = 0; i < newArray.Length; i++)
            {
                newArray[i] = BitConverter.GetBytes(Convert.ToInt16(parts[i]))[0];
            }
            return newArray;
        }

        private void SendQuery(byte commandGetTimeCode, byte commandGetTimeLength, byte[] bodyByte)
        {
            if (_connection)
            {
                byte[] addressBytes = GetAddressBytes();
                SendToPulsar(commandGetTimeCode, commandGetTimeLength, addressBytes, bodyByte);
            }
            else
            {
                AddMessage("для отправки нужно устанвоить соединение");
            }
        }

        private byte[] GetAddressBytes()
        {
            var ip = _client.RemoteEndPoint;
            var b = (ip.ToString().Split(':')[0]).Split('.');
            var i1 = BitConverter.GetBytes(Convert.ToInt16(b[0]));
            var i2 = BitConverter.GetBytes(Convert.ToInt16(b[1]));
            var i3 = BitConverter.GetBytes(Convert.ToInt16(b[2]));
            var i4 = BitConverter.GetBytes(Convert.ToInt16(b[3]));
            byte[] ipAddressBytes = {i1[0], i2[0], i3[0], i4[0]};
            return ipAddressBytes;
        }

        private void SendToPulsar(byte commandCode, byte commandLength, byte[] addressBytes, byte[] bodyBytes)
        {
            byte[] bytes = GetFirstSixBytes(addressBytes, commandCode, commandLength);
            if(bodyBytes != null)
            {
                bytes = GetBytesWithBodyBytes(bodyBytes, bytes);
            }
            byte[] commandIdByte = GetRandomIdByte();
            bytes = GetBytesWithIdBytes(commandIdByte, bytes);
            bytes = GetBytesWithCrcBytes(bytes);
            AddMessage("отправлено сообщение: " + BitConverter.ToString(bytes));
            _client.Send(bytes, bytes.Length, 0);
        }

        private byte[] GetFirstSixBytes(byte[] ipAddressBytes, byte commandCode, byte commandLength)
        {
            byte[] newArray = {
                    ipAddressBytes[0], ipAddressBytes[1], ipAddressBytes[2], ipAddressBytes[3],
                    commandCode,
                    commandLength
                };
            return newArray;
        }

        private byte[] GetBytesWithBodyBytes(byte[] bodyBytes, byte[] firstSixBytes)
        {
            var newArray = new byte[firstSixBytes.Length + bodyBytes.Length];
            firstSixBytes.CopyTo(newArray, 0);
            for (int i = 0; i < bodyBytes.Length; i++)
            {
                newArray[firstSixBytes.Length + i] = bodyBytes[i];
            }
            return newArray;
        }

        private byte[] GetRandomIdByte()
        {
            var r = new Random();
            var i1 = BitConverter.GetBytes(r.Next(0, 255));
            var i2 = BitConverter.GetBytes(r.Next(0, 255));
            byte[] idBytes = { i1[0], i2[0] };
            return idBytes;
        }

        private byte[] GetBytesWithIdBytes(byte[] commandIdByte, byte[] bytes)
        {
            var newArray = new byte[bytes.Length + 2];
            bytes.CopyTo(newArray, 0);
            newArray[bytes.Length + 0] = commandIdByte[0];
            newArray[bytes.Length + 1] = commandIdByte[1];
            return newArray;
        }

        private byte[] GetBytesWithCrcBytes(byte[] bytes)
        {
            var crc16Uint = crc16converter.GetCrc16(bytes);
            var crc16Bytes = BitConverter.GetBytes(crc16Uint);

            var newArray = new byte[bytes.Length + 2];
            bytes.CopyTo(newArray, 0);
            newArray[bytes.Length + 0] = crc16Bytes[0];
            newArray[bytes.Length + 1] = crc16Bytes[1];
            return newArray;
        }

        private void AddMessage(string txt)
        {
            DateTime time = DateTime.Now;
            const string format = "HH:mm:ss";
            uiMessageTextBox.Text += time.ToString(format)+ ": " + txt + Environment.NewLine;
        }
    }
}
