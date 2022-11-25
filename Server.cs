using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.ComponentModel;

namespace FindMyMineUI
{
    class Server
    {
        delegate void SetTextCallback(string text);
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;
        private static string IPADDRESS = "192.168.22.156";
        private static TcpListener tcpListener;
        public static TextBox textbox;
        public static ListView playerOnline;
        public static TextBox noPlayer;
        public static bool isRunning;
        static int noplayeronline = 1;
        public Server(TextBox txb,ListView lsv,TextBox noplayer)
        {
            textbox = txb;
            playerOnline = lsv;
            noPlayer = noplayer;
            TextBox.CheckForIllegalCrossThreadCalls = false;
            ListView.CheckForIllegalCrossThreadCalls = false;
        }

        public static void UpdateText(string message)
        {

            if (textbox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(UpdateText);
                textbox.Invoke(d, new object[] { message+Constants.CRLF });
            }
            else
            {
                textbox.Text +=  message+Constants.CRLF;
            }

        }
        public static void AddPlayerOnline(string username)
        {
            playerOnline.Items.Add(username);
            noPlayer.Text = noplayeronline.ToString();
            noplayeronline++;

        }
        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            UpdateText("Starting server...");
            InitializeServerData();
            IPAddress address = IPAddress.Parse(IPADDRESS);
            UpdateText($"IP address : {address}");
            tcpListener = new TcpListener(address, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            UpdateText($"Server started on port {Port}.");
        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            UpdateText($"Incoming connection from {_client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }

            UpdateText($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.clickpos, ServerHandle.GetClickPos },
                {(int)ClientPackets.lobby, ServerHandle.UserJoinLobby },
                {(int)ClientPackets.chat, ServerHandle.GetChatMessage},
                {(int)ClientPackets.state, ServerHandle.GetUserState}
            };
            UpdateText("Initialized packets.");
        }

    }
}
