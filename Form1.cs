using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
//using Timer = System.Threading.Timer;
using Timer = System.Windows.Forms.Timer;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using System.Timers;

namespace SUPERCHAT
{
    public partial class Сhatterbox : Form
    {
        bool alive = false;
        UdpClient client;
        int temp = 0;
        const int LOCALPORT = 10113; // порт отправки
        const int REMOTEPORT = 8001; // порт приема
        const int TTL = 20;
        const string HOST = "235.5.5.25"; // хост для групповой рассылки
        IPAddress groupAddress; // адрес для групповой рассылк
        static IPAddress address = IPAddress.Parse("26.45.37.25");
        Stopwatch stopwatch = new Stopwatch();//создаем экземпляр класса
        
        //stopwatch.Start();

        string userName; // имя пользователя в чате
        public Сhatterbox()
        {
            InitializeComponent();
            LoginButton.Enabled = true; // кнопка входа
            LogoutButton.Enabled = false; // кнопка выхода
            sendButton.Enabled = false; // кнопка отправки
            chatTextBox.ReadOnly = true; // поле для сообщений
            groupAddress = IPAddress.Parse(HOST);
        }
        private void ReceiveMessages()
        {
            alive = true;
            try
            {
                while (alive)
                {
                    IPEndPoint remoteIp = null;
                    byte[] data = client.Receive(ref remoteIp);
                    string message = Encoding.Unicode.GetString(data);
                    this.Invoke(new MethodInvoker(() =>
                    {
                        string time = DateTime.Now.ToShortTimeString();//отображает текущее время
                        chatTextBox.Text = time + " " + message + "\r\n" + chatTextBox.Text;
                    }));
                }
            }
            catch (ObjectDisposedException)
            {
                if (!alive)
                    return;
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            if (userNameTextBox.Text == "")
            {
                MessageBox.Show("Введите имя");
            }
            else
            {
                userName = userNameTextBox.Text;
                userNameTextBox.ReadOnly = true;
                Random rnd = new Random();
                temp = rnd.Next(10000, 11000);
                try
                {
                    client = new UdpClient(LOCALPORT);
                    client.JoinMulticastGroup(groupAddress, TTL);
                    // запускаем задачу на прием сообщений
                    Task receiveTask = new Task(ReceiveMessages);
                    receiveTask.Start();
                    // отправляем первое сообщение о входе нового пользователя
                    string message = userName + " вошел в чат. Добро пожаловать";
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    client.Send(data, data.Length, HOST, REMOTEPORT);
                    client.Send(data, data.Length, HOST, LOCALPORT);
                    LoginButton.Enabled = false;
                    LogoutButton.Enabled = true;
                    sendButton.Enabled = true;
                    stopwatch.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void LogoutButton_Click(object sender, EventArgs e)
        {
            ExitChat();
        }

        private void ExitChat()
        {
            string message = userName + " покинул чат.";
            byte[] data = Encoding.Unicode.GetBytes(message);
            client.Send(data, data.Length, HOST, REMOTEPORT);
            client.Send(data, data.Length, HOST, LOCALPORT);
            client.DropMulticastGroup(groupAddress);

            alive = false;
            client.Close();

            LoginButton.Enabled = true;
            LogoutButton.Enabled = false;
            sendButton.Enabled = false;
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            try
            {
                string message = String.Format("{0}: {1}", userName, messageTextBox.Text);
                byte[] data = Encoding.Unicode.GetBytes(message);
                client.Send(data, data.Length, HOST, REMOTEPORT);
                client.Send(data, data.Length, HOST, LOCALPORT);
                messageTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopwatch.Stop();
            if (alive)
                ExitChat();
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            chatTextBox.Text = "";

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan timeSpan = stopwatch.Elapsed;
            chatTextBox.Text = string.Format("{0:00}:{1:00}:{2:00}",timeSpan.Hours, timeSpan.Minutes,timeSpan.Seconds);
        }

        private void Сhatterbox_Load(object sender, EventArgs e)
        {
            Timer timer1 = new Timer();
            timer1.Interval = 120000;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Start();
        }
    }
}
