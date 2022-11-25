using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data.Odbc;
using static FindMyMineUI.GameLogic;
using System.Runtime.InteropServices;

namespace FindMyMineUI
{
    class GameLogic
    {

        public static List<User> users = new List<User>();

        // <lobbyid,lobbyinfo>
        public static Dictionary<int, Lobby> lobbies = new Dictionary<int, Lobby>();
        public static void Update()
        {
            ThreadManager.UpdateMain();
        }

        public static void GameOver(int lobbyid)
        {
            Console.WriteLine("GameOver in lobby "+lobbyid);
            Lobby lobby = lobbies[lobbyid];
            lobby.isOver = true;
            User Winner = lobby.getWinner();
            ServerSend.SendState("3,"+Winner.name);
        }

        public static void StartGame(int lobby)
        {
            Server.UpdateText("Starting game in lobby "+lobby);
            lobbies[lobby].randomizeTurn();
            SendUserGenericData(lobbies[lobby].User1.Id);
            SendUserGenericData(lobbies[lobby].User2.Id);
            ServerSend.SendState("0");
            ServerSend.SendState("1,"+ lobbies[lobby].playerTurn);
            Server.UpdateText("Player " + lobbies[lobby].playerTurn);
        }
        public static void AddUser(int client,string name)
        {
            User newuser = new User(name,client);
            users.Add(newuser);
        }

        public static void CreateLobby(int userid,int lobbyid,int width = 6,int height = 6,int mine = 11,int supermine = 0,GameMode gamemode = GameMode.Normal,int char1 = 0)
        {

            Server.UpdateText("Creating lobby");
            Lobby newlobby = new Lobby(GetUserFromUserId(userid),lobbyid, width, height, mine, supermine, gamemode, char1) ;
            Server.UpdateText($"4,{lobbyid},{width},{height},{mine},{supermine},{(int)gamemode}");
            ServerSend.SendStateToUser(userid,$"4,{lobbyid},{width},{height},{mine},{supermine},{(int)gamemode}");
            lobbies.Add(lobbyid, newlobby);
        }

        public static void RemoveLobby(int lobbyid)
        {
            lobbies.Remove(lobbyid);
        }

        public static void PutUserToLobby(int userid, int lobbyid,int char2)
        {
            //get lobby info

            System.Threading.Thread.Sleep(500);
            User user = GetUserFromUserId(userid);

            //if no lobby
            //add error
            if(!lobbies.TryGetValue(lobbyid, out Lobby lobby))
            {
                ServerSend.Error(userid, 0);
                return;
            }
            //

            //$"4,{lobbyid},{width},{height},{mine},{supermine},{(int)gamemode}"
            ServerSend.SendStateToUser(userid, $"4,{lobbyid},{lobby.width},{lobby.height},{lobby.totalBomb},{lobby.SuperMine},{(int)lobby.gameMode}");
            lobby.char2 = char2;
            System.Threading.Thread.Sleep(500);
            if (lobby.User2.Id == -1)
            {
                //lobby has 1 player, add the user and start the game.
                lobby.addUser(user);
                StartGame(lobbyid);
            }

            //if lobby is full, do ...

            //
        }

        //gameplay 
        public static void handleClickPosition(int client,int x,int y)
        {
            var lobby = lobbies[GetLobbyFromUserId(client)];
            var user = GetUserFromUserId(client);
            vector2d vector = new vector2d(x, y);
            switch (lobby.gameMode)
            {

                case GameMode.Normal:
                    Tile tile = null;
                    if (x != -1)
                    {
                        tile = lobby.tiles[vector];
                        if(tile.type == TileType.Bomb)
                        {
                            lobby.bombFound++;
                            user.score++;
                        }

                        if (tile.type == TileType.Superbomb)
                        {
                            lobby.bombFound++;
                            user.score += 2;
                        }
                    }
                    int tileinfo = tile == null ? 0 : (int)tile.type;
                    ServerSend.ClickInfo($"{x},{y},{tileinfo}");

                    if (lobby.bombFound == lobby.totalBomb + lobby.SuperMine)
                    {
                        GameOver(lobby.id);
                    }

                    else { NextTurn(lobby.id, true); }
                    break;
                case GameMode.Minesweeper:
                    tile = null;
                    if (x != -1)
                    {
                        tile = lobby.tiles[vector];
                        if (tile.type == TileType.Bomb)
                        {
                            lobby.bombFound++;
                            user.score++;
                        }

                        if (tile.type == TileType.Superbomb)
                        {
                            lobby.bombFound++;
                            user.score += 2;
                        }


                    }
                    tileinfo = tile == null ? 0 : (int)tile.type;
                    int surroundingbomb = tile == null ? 0 : tile.surroundingBomb;
                    ServerSend.ClickInfo($"{x},{y},{tileinfo},{surroundingbomb}");
                    if (lobby.bombFound == lobby.totalBomb + lobby.SuperMine)
                    {
                        GameOver(lobby.id);
                    }
                    else { NextTurn(lobby.id, true); }
                    break;
                case GameMode.Reversed:
                    tile = x != -1? lobby.tiles[vector] : null;


                    tileinfo = tile == null ? 0 : (int)tile.type;
                    ServerSend.ClickInfo($"{x},{y},{tileinfo}");
                    if (x == -1 || tile.type == TileType.Bomb || tile.type == TileType.Superbomb)
                    {
                        GameOver(lobby.id);
                    }
                    else { NextTurn(lobby.id, true); }
                    break;
            }
        }


        public static void NextTurn(int lobbyid,bool changePlayer)
        {
            if (changePlayer)
            {
                int player1 = lobbies[lobbyid].User1.Id;
                int player2 = lobbies[lobbyid].User2.Id;
                // change turn
                if (lobbies[lobbyid].playerTurn == player1)
                {
                    lobbies[lobbyid].playerTurn = player2;
                }
                else
                {
                    lobbies[lobbyid].playerTurn = player1;
                }
                //
            }
            Console.WriteLine("player " + lobbies[lobbyid].playerTurn + " turn");
            ServerSend.SendState("1,"+ lobbies[lobbyid].playerTurn);
        }

       
        public static void SendUserGenericData(int userid)
        {
            var lobby = lobbies[GetLobbyFromUserId(userid)];
            var user1 = lobby.User1;
            var user2 = lobby.User2;

            string info = $"{user1.name},{user1.score},{user2.name},{user2.score},{lobby.char1},{lobby.char2}";
            Console.WriteLine(info);
            ServerSend.SendGenericInfo(user1.Id ,info);
            if(!lobby.isOver)ServerSend.SendState("1," + lobby.playerTurn);
        }


        public static User GetUserFromUserId(int id)
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
            User user = GetUserFromUserId(id);
            foreach(int i in lobbies.Keys)
            {
                if (lobbies[i].User1.Id == id || lobbies[i].User2.Id == id)
                {
                    return i;
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

        public enum GameMode
        {
            Normal = 0,
            Minesweeper = 1,
            Reversed = 2,
            Battleship = 3
        }

        public enum TileType
        {
            Normal = 0,
            Bomb = 1,
            Superbomb = 2
        }

        public class Lobby
        {
            public int id;
            public User User1;
            public User User2 = new User();
            public int bombFound = 0;
            public int totalBomb;
            public GameMode gameMode;
            public int width, height;
            public int SuperMine;
            public int char1, char2;
            public bool isOver = false;
            public Dictionary<vector2d, Tile> tiles = new Dictionary<vector2d, Tile>();
            public int playerTurn;

            public Lobby(User user1,int id, int width = 6, int height = 6, int totalBomb = 11, int superbomb = 0,GameMode gameMode=GameMode.Normal,int char1 = 0,int char2 = 0)
            {
                User1 = user1;
                this.id = id;
                this.totalBomb = totalBomb;
                this.SuperMine= superbomb;
                this.gameMode = gameMode;
                this.width = width;
                this.height = height;
                this.char1 = char1;
                this.char2 = char2;
                createBoard();

            }
            public void resetBoard()
            {
                tiles = new Dictionary<vector2d, Tile>();
                isOver = false;
                bombFound = 0;
            }
            public void resetScore()
            {
                User1.score = 0;
                User2.score = 0;
            }
            public void createBoard()
            {
                resetBoard();
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        vector2d vector = new vector2d(x, y);
                        tiles.Add(vector, new Tile(x, y));
                    }
                }

                generateBomb();
            }
            public void generateBomb()
            {
                Random rand = new Random();
                int bombSpawned = 0;
                while (bombSpawned < totalBomb)
                {
                    vector2d position = new vector2d(rand.Next(0, width), rand.Next(0, height));
                    if (tiles[position].type != TileType.Bomb)
                    {
                        tiles[position].type = TileType.Bomb;
                        bombSpawned++;
                    }

                }
                bombSpawned = 0;
                while(bombSpawned < SuperMine)
                {
                    vector2d position = new vector2d(rand.Next(0, width), rand.Next(0, height));
                    if (tiles[position].type != TileType.Bomb && tiles[position].type != TileType.Superbomb)
                    {
                        tiles[position].type = TileType.Superbomb;
                        bombSpawned++;
                    }
                }
                if (gameMode != GameMode.Minesweeper) return;

                //for minesweeper mode only
                foreach (Tile tile in tiles.Values)
                {
                    tile.surroundingBomb = getSurroudingbombcount(tile);
                }

            }

            public bool isBomb(int x,int y)
            {
                vector2d position = new vector2d(x, y);
                return tiles[position].type != TileType.Normal ;
            }
            public void addUser(User user)
            {
                User2 = user;
            }

            public void randomizeTurn()
            {
                playerTurn = new Random().Next(0, 2) == 1 ? User1.Id : User2.Id;
            }
            public int getSurroudingbombcount(Tile tile) 
            {
                int count = 0;
                vector2d[] surroundingVector = { new vector2d(-1, -1),   new vector2d(0, -1),    new vector2d(1, -1) , 
                                                 new vector2d(-1, 0),                           new vector2d(1,0),
                                                 new vector2d(-1, 1),    new vector2d(0,1),      new vector2d(1,1)};
                vector2d tilevector = tile.vector;
                foreach (vector2d vector in surroundingVector)
                {
                    count += getTilefromVector(vector.add(tilevector)) != null && (getTilefromVector(vector.add(tilevector)).type == TileType.Bomb || getTilefromVector(vector.add(tilevector)).type == TileType.Superbomb) ? 1 : 0;
                }
                return count;
            }

            public Tile getTilefromVector(vector2d vector)
            {
                return tiles.TryGetValue(vector, out Tile value) ? value : null;
            }
            public User getWinner()
            {
                return User1.score > User2.score ? User1 : User2;
            }
        }

        public class vector2d
        {
            public int x;
            public int y;
            public vector2d(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public vector2d add(vector2d vector2)
            {
                return new vector2d(x + vector2.x, y + vector2.y);
            }
            public override bool Equals(object vector)
            {
                var vector2 = vector as vector2d;
                if(vector2 == null)return false;
                return vector2.x == this.x && vector2.y == this.y;
                
            }

            public override int GetHashCode()
            {
                return x + y * 17;
            }
        }
        
        public class Tile
        {
            public vector2d vector = new vector2d(0,0);
            public TileType type = TileType.Normal;
            public int surroundingBomb;
            public Tile(int posx, int posy)
            {
                vector.x = posx;
                vector.y = posy;
            }
        }


        public class User {
            public string name;
            public int Id = -1;
            public int score = 0;
            public int lobby = -1;

            public User() {}
            public User(string name, int id)
            {
                this.name = name;
                this.Id = id;
            }
        }

        
    }
}
