using System;
using System.Collections.Generic;
using System.Text;


namespace FindMyMineUI
{
    class ServerSend
    {
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }

        public static void Welcome(int _toClient,string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void Error(int _client,int error)
        {
            using (Packet _packet = new Packet((int)ServerPackets.error))
            {
                _packet.Write(error);

                SendTCPData(_client, _packet);
            }
        }
 

        public static void ClickInfo(string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.clickpos))
            {
                _packet.Write(_msg);
                SendTCPDataToAll(_packet);
            }
        }
        public static void SendGenericInfo(int client, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.genericinfo))
            {
                _packet.Write(_msg);
                SendTCPDataToAll(_packet);
            }
        }

        /* Get state
 *  0 -> reset board    data [0]
 *  1 -> Change turn    data [1,turns]
 *  2 -> Waiting        data [2]
 *  3 -> Game over      data [3,winner]
 *  4 -> Into game scene data [4,width,height]
 */

        public static void SendState(string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.state))
            {
                _packet.Write(_msg);
                SendTCPDataToAll(_packet);
            }
        }
        public static void SendState(int client ,string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.state))
            {
                _packet.Write(_msg);
                SendTCPData(client, _packet);
            }
        }
        public static void SendStateToUser(int _client,string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.state))
            {
                _packet.Write(_msg);
                SendTCPData(_client,_packet);
            }
        }
        public static void SendUserToLobby(string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.lobby))
            {
                _packet.Write(_msg);
                SendTCPDataToAll(_packet);
            }
        }

        public static void SendChatMessage(int client, string _msg)
        {
            int user1 = GameLogic.lobbies[GameLogic.GetLobbyFromUserId(client)].User1.Id;
            if(GameLogic.lobbies[GameLogic.GetLobbyFromUserId(client)].User2.Id == -1) { return; }
            int user2 = GameLogic.lobbies[GameLogic.GetLobbyFromUserId(client)].User2.Id;
            using (Packet _packet = new Packet((int)ServerPackets.chat))
            {
                _packet.Write(_msg);
                SendTCPData(client == user1 ? user2:user1,_packet);
            }
        }

    }
}
