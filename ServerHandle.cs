using System;
using System.Collections.Generic;
using System.Text;

namespace FindMyMineUI
{
    class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();

            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint},named {_username} connected successfully and is now player {_fromClient}.");
            if (_fromClient != _clientIdCheck)
            {
                Console.WriteLine($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }
            Server.AddPlayerOnline(_username);
            GameLogic.AddUser(_fromClient,_username);
        }
        

        public static void GetClickPos(int _fromClient, Packet _packet)
        {
            string clickpos = _packet.ReadString();
            //Console.WriteLine($"Player {_fromClient} has clicked {clickpos}");
            ServerSend.SendClickPos(_fromClient,clickpos);
            GameLogic.NextTurn(0);
            GameLogic.SendUserGenericData(_fromClient);
            
        }

        public static void UserJoinLobby(int _fromClient, Packet _packet)
        {
            if(_packet.ReadBool())
            {
                Console.WriteLine("User " + _fromClient + " create a lobby.");
                GameLogic.CreateLobby(_fromClient, 0);
            }
            else
            {
                Console.WriteLine("User " + _fromClient + " join a lobby.");
                GameLogic.PutUserToLobby(_fromClient, 0);
            }

        }

        public static void GetBoardInfo()
        {

        }


        public static void GetUserState(int _fromClient, Packet _packet)
        {

        }


    }
}
