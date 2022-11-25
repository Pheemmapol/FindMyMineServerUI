using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Linq.Expressions;

namespace FindMyMineUI
{

    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            Server server = new Server(textBox1, listView1,textBox2);
        }

        public void updateTextBox(string message)
        {
            textBox1.Text += message + "\r\n";
        }

        //start server
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Server.isRunning)
                {
                    Server.Start(40, 26950);
                    Server.isRunning = true;

                    Server.UpdateText($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
                    DateTime _nextLoop = DateTime.Now;
                    Thread t = new Thread(MainThread);
                    t.Name = "Server Listener Thread";
                    t.IsBackground = true;
                    t.Start();

                }
                else
                {
                    textBox1.Text += "Server already running!" + "\r\n";
                }
            }
            catch(Exception ex)
            {
                textBox1.Text += "Error! Invalid IP." + "\r\n";
            }
        }

        //end server
        private void button2_Click(object sender, EventArgs e)
        {

        }

        //reset board
        private void button3_Click(object sender, EventArgs e)
        {
            ServerSend.SendState("0");
            foreach(GameLogic.Lobby lobby in GameLogic.lobbies.Values)
            {
                lobby.createBoard();
                lobby.resetScore();
                lobby.randomizeTurn();
                GameLogic.SendUserGenericData(lobby.User1.Id);
                GameLogic.SendUserGenericData(lobby.User2.Id);
            }
        }

        private static void MainThread()
        {
            DateTime _nextLoop = DateTime.Now;
            while (Server.isRunning)
            {
                while (_nextLoop < DateTime.Now)
                {
                    GameLogic.Update();
                    _nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if (_nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(_nextLoop - DateTime.Now);
                    }
                }
            }

        }

    }
}
