using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FindMyMineUI
{
    class GameLogic
    {
        //right now we have 1 lobby

        // these variable should change when implementing lobby
        public static int playerTurn; 
        public static int bombfound  = 0;
        public static int[][] bombpos = new int[11][];
        //
        public static List<User> users = new List<User>();
        public static Dictionary<int, User[]> usersinLobby= new Dictionary<int, User[]>();

        public static void Update()
        {
            ThreadManager.UpdateMain();
        }

        public static void GameOver(int lobby)
        {
            Console.WriteLine("GameOver in lobby 0");
            string winner = usersinLobby[lobby][0].score > usersinLobby[lobby][1].score ? usersinLobby[lobby][0].name : usersinLobby[lobby][1].name;
            ServerSend.SendState("3,"+winner);
        }

        public static void StartGame(int lobby)
        {
            Console.WriteLine("Starting game in lobby 0");
            playerTurn = new Random().Next(0,2) == 1? usersinLobby[lobby][0].Id : usersinLobby[lobby][1].Id;
            GenerateBombPos();
            SendUserGenericData(usersinLobby[lobby][0].Id);
            SendUserGenericData(usersinLobby[lobby][1].Id);
            ServerSend.SendState("0");
            ServerSend.SendState("1,"+playerTurn);
        }
        public static void AddUser(int client,string name)
        {
            User newuser = new User(name,client);
            users.Add(newuser);
        }

        public static void CreateLobby(int userid,int lobbyid)
        {
            //if lobby already exists then do...


            //

            User[] userlobby = new User[2];
            userlobby[0] = GetUserFromId(userid);
            usersinLobby.Add(lobbyid, userlobby);
        }

        public static void RemoveLobby(int lobbyid)
        {
            usersinLobby.Remove(lobbyid);
        }

        public static void NextTurn(int lobbyid)
        {
            int player1 = usersinLobby[lobbyid][0].Id;
            int player2 = usersinLobby[lobbyid][1].Id;
            // change turn
            if (playerTurn == player1)
            {
                playerTurn = player2;
            }
            else
            {
                playerTurn = player1;
            }
            //
            Console.WriteLine("player " + playerTurn + " turn");
            ServerSend.SendState("1,"+playerTurn);
        }

        public static void PutUserToLobby(int userid,int lobby)
        {
            ServerSend.SendState("4,6,6");
            User user = GetUserFromId(userid);
            if (usersinLobby[lobby][1] == null)
            {
                //lobby has 1 player, add the user and start the game.
                usersinLobby[lobby][1] = user;
                StartGame(lobby);
            }

            //if lobby is full, do ...
            
            //
        }
        public static void SendUserGenericData(int userid)
        {
            int lobby = GetLobbyFromUserId(userid);
            var user1 = usersinLobby[lobby][0];
            var user2 = usersinLobby[lobby][1];

            string info = $"{user1.name},{user1.score},{user2.name},{user2.score}";
            Console.WriteLine(info);
            ServerSend.SendGenericInfo(user2.Id, info);
            ServerSend.SendGenericInfo(user1.Id ,info);
            ServerSend.SendState("1," + playerTurn);
        }

        public static void GenerateBombPos()
        {
            bombpos = new int[11][];
            Random rand = new Random();
            int bombSpawned = 0;
            while (bombSpawned < 11)
            {
                int[] newpos = new int[2] { rand.Next(0, 6), rand.Next(0, 6) };
                if (!isIn(bombpos, newpos))
                {
                    bombpos[bombSpawned] = newpos;
                    bombSpawned++;
                }

            }
        }

        
        public static bool isIn(int[][] big, int[] small)
        {
            for (int i = 0; i < big.Count(x => x != null); i++)
            {
                if (big[i][0] == small[0] && big[i][1] == small[1])
                {
                    return true;
                }
            }
            return false;
        }

        public static User GetUserFromId(int id)
        {
            foreach (User user in users)
            {
                if(user.Id == id)
                {
                    return user;
                }
            }
            return null;
        }

        public static int GetLobbyFromUserId(int id)
        {
            User user = GetUserFromId(id);
            foreach(int i in usersinLobby.Keys)
            {
                foreach(User eachuser in usersinLobby[i])
                {
                    if(eachuser.Id == id)
                    {
                        return i;
                    }
                }
            }
            return -99;
        }

         /* GameState
        {
            Reset = 0,
            PlayerTurn = 1,
            Waiting = 2
        }
         */

        public class lobbyInfo
        {
            public int lobby = -1;
            public User User1;
            public User User2;
            public int bombFound = 0;
            public static int[][] bombpos;
        }
        public class User {
            public string name;
            public int Id = -1;
            public int score = 0;
            public int lobby = -1;
            public User(string name, int id)
            {
                this.name = name;
                this.Id = id;
            }
        }
    }
}
