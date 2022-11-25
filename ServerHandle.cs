using System;
using System.Collections.Generic;
using System.Text;
using static FindMyMineUI.GameLogic;

namespace FindMyMineUI
{
    class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();

            Server.UpdateText($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint},named {_username} connected successfully and is now player {_fromClient}.");
            if (_fromClient != _clientIdCheck)
            {
                Console.WriteLine($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }
            Server.AddPlayerOnline(_username);
            AddUser(_fromClient,_username);
        }
        

        public static void GetClickPos(int _fromClient, Packet _packet)
        {
            string clickpos = _packet.ReadString();
            //Console.WriteLine($"Player {_fromClient} has clicked {clickpos}");
            string[] pos = clickpos.Split(',');
            GameLogic.handleClickPosition(_fromClient,int.Parse(pos[0]), int.Parse(pos[1]));
            GameLogic.SendUserGenericData(_fromClient);
            
        }

        public static void UserJoinLobby(int _fromClient, Packet _packet)
        {
            string _msg = _packet.ReadString();
            string[] message = _msg.Split(',');

            if (int.Parse(message[0]) == 1)
            {
                //0{create},1{lobbyid},2{width},3{height},4{bombcount},5{supermine},6{gamemode} , 7char1
                Console.WriteLine("User " + _fromClient + " create a lobby.");
                GameLogic.CreateLobby(_fromClient, int.Parse(message[1]), int.Parse(message[2]),
                                        int.Parse(message[3]), int.Parse(message[4]),
                                        int.Parse(message[5]), (GameLogic.GameMode)int.Parse(message[6]),
                                        int.Parse(message[7])
                                        );
            }
            else
            {
                Console.WriteLine("User " + _fromClient + " join a lobby.");
                GameLogic.PutUserToLobby(_fromClient, int.Parse(message[1]), int.Parse(message[2]));
            }

        }

        public static void GetChatMessage(int _fromClient, Packet _packet)
        {
            ServerSend.SendChatMessage(_fromClient,_packet.ReadString());
        }


        public static void GetUserState(int _fromClient, Packet _packet)
        {
            int state = _packet.ReadInt();
            
            Lobby lobby = lobbies[GetLobbyFromUserId(_fromClient)];
            switch (state)
            {
                case 0:
                    ServerSend.SendState(_fromClient,"0");
                    ServerSend.SendState(lobby.User1.Id == _fromClient? lobby.User2.Id : lobby.User1.Id, "0");
                    lobby.resetScore();
                    lobby.randomizeTurn();
                    lobby.createBoard();
                    GameLogic.SendUserGenericData(_fromClient);
                    break;
                case 1:
                    ServerSend.SendState(lobby.User1.Id == _fromClient ? lobby.User2.Id : lobby.User1.Id, "-1");
                    RemoveLobby(lobby.id);
                    break;
            }
        }


    }
}
