using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using MyFeatures;
using MyGame;

namespace MyBoard
{
    public static class Results
    {
    public static int winsNorthAmerica { get; set; } = 0;
    public static int winsSouthAmerica { get; set; } = 0;
    public static int winsEurope { get; set; } = 0;
    public static int winsAfrica { get; set; } = 0;
    public static int winsAsia { get; set; } = 0;
    public static int winsAustralia { get; set; } = 0;
    public static int errors { get; set; } = 0;
    public static int totalTurns { get; set; } = 0;
    public static int winsStarter { get; set; } = 0;
    public static int plays { get; set; } = 10;
    }

    public class Player
    {
        public string Name { get; set; }
        public int Stars { get; set; }
        public int Coins { get; set; }
        public bool OnSea { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();

        public Player(string name)
        {
            Name = name;
            Coins = 12;
            Stars = 0;
            OnSea = false;
        }

        public void Reset()
        {
            Stars = 0;
            Coins = 12;
            OnSea = false;
            Products.Clear();
        }
    }

    public class Product
    {
        public string Name { get; set; }
        public int PurchasePrice { get; set; }
        public int SellingPrice { get; set; }
        public Tuple<int, int> ShopPosition { get; set; }
    }

    public class GameAssets
    {
        public static List<Product> productCards { get; } = new List<Product>
        {
            new Product { Name = "Peper", PurchasePrice = 2, SellingPrice = 4 },
            new Product { Name = "Kaneel", PurchasePrice = 2, SellingPrice = 4 },
            new Product { Name = "Katoen", PurchasePrice = 2, SellingPrice = 4 },
            new Product { Name = "Rijst", PurchasePrice = 2, SellingPrice = 5 },
            new Product { Name = "Rietsuiker", PurchasePrice = 3, SellingPrice = 6 },
            new Product { Name = "Porselein", PurchasePrice = 4, SellingPrice = 8 },
            new Product { Name = "Zilver", PurchasePrice = 5, SellingPrice = 10 },
            new Product { Name = "Goud", PurchasePrice = 5, SellingPrice = 11 },
            new Product { Name = "Parel", PurchasePrice = 6, SellingPrice = 12 },
        };
    }


    public class Board
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public List<SquareFeature>[,] Squares { get; private set; }
        public Dictionary<Player, Tuple<int, int>> PlayerPositions { get; private set; }
        public List<Player> Players { get; private set; }


        public enum SquareFeature
        {
            Star1,
            Star2,
            Star3,
            Shop,
            Port,
            NorthAmerica,
            SouthAmerica,
            Europe,
            Africa,
            Asia,
            Australia,
            Land,
            PlayerNumber
        }


        public Board(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            Squares = new List<SquareFeature>[rows, columns];
            PlayerPositions = new Dictionary<Player, Tuple<int, int>>();
            Players = new List<Player>();
            InitializeBoard();
        }


        public void InitializeBoard()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    Squares[i, j] = new List<SquareFeature>();
                }
            }
        }


        public void AddPlayer(Player player, int row, int column)
        {
            Players.Add(player);
            PlayerPositions.Add(player, Tuple.Create(row, column));
        }


        public void RemovePlayer(Player player)
        {
            if (Players.Contains(player))
            {
                Players.Remove(player);
                Tuple<int, int> position = PlayerPositions[player];
                PlayerPositions.Remove(player);
                RemoveFeature(position.Item1, position.Item2, SquareFeature.PlayerNumber);
            }
            else
            {
                throw new ArgumentException("Player not found.");
            }
        }


        public void ShufflePlayers()
        {
            Random rng = new Random();
            int n = Players.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Player value = Players[k];
                Players[k] = Players[n];
                Players[n] = value;
            }
        }


        public Player AddPlayerOnRandomLandFeature(int playerNumber)
        {
            Random random = new Random();
            int row, column;
            
            SquareFeature selectedContinent = GetContinentForPlayer(playerNumber);
            string continentName = selectedContinent.ToString().Replace("SquareFeature.", "");

            do
            {
                row = random.Next(Rows);
                column = random.Next(Columns);
            } while (!HasFeature(row, column, SquareFeature.Land) || !HasFeature(row, column, selectedContinent));

            Player player = new Player(continentName);
            AddPlayer(player, row, column);
            AddFeature(row, column, SquareFeature.PlayerNumber);

            return player;
        }


        private SquareFeature GetContinentForPlayer(int playerNumber)
        {
            switch (playerNumber)
            {
                case 1:
                    return SquareFeature.NorthAmerica;
                case 2:
                    return SquareFeature.SouthAmerica;
                case 3:
                    return SquareFeature.Europe;
                case 4:
                    return SquareFeature.Africa;
                case 5:
                    return SquareFeature.Asia;
                case 6:
                    return SquareFeature.Australia;
                default:
                    throw new ArgumentException("Invalid player number.");
            }
        }


        public void AddFeature(int row, int column, SquareFeature feature)
        {
            if (row >= 0 && row < Rows && column >= 0 && column < Columns)
            {
                Squares[row, column].Add(feature);
            }
            else
            {
                Console.WriteLine($"{row} & {column} are invalid coordinates. Unable to add feature.");
            }
        }

        public void RemoveFeature(int row, int column, SquareFeature feature)
        {
            if (row >= 0 && row < Rows && column >= 0 && column < Columns)
            {
                if (Squares[row, column].Contains(feature))
                {
                    Squares[row, column].Remove(feature);
                }
            }
            else
            {
                Console.WriteLine($"{row} & {column} are invalid coordinates. Unable to remove feature.");
            }
        }


        public bool HasFeature(int row, int column, SquareFeature feature)
        {
            if (row >= 0 && row < Rows && column >= 0 && column < Columns)
            {
                return Squares[row, column].Contains(feature);
            }
            else
            {
                Console.WriteLine("Invalid coordinates. Unable to check for feature.");
                return false;
            }
        }


        public void PrintBoard()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    Console.Write(GetSquareSymbol(i, j) + " ");
                }
                Console.WriteLine();
            }
        }


        public char GetSquareSymbol(int row, int column)
        {
            List<SquareFeature> features = Squares[row, column];
            if (features.Count == 0)
            {
                return '~';
            }
            else
            {
                return GetFeatureSymbol(features[features.Count - 1]);
            }
        }


        public char GetFeatureSymbol(SquareFeature feature)
        {
            switch (feature)
            {
                case SquareFeature.Land:
                    return 'L';
                case SquareFeature.Star1:
                    return '*';
                case SquareFeature.Star2:
                    return '@';
                case SquareFeature.Star3:
                    return '!';
                case SquareFeature.Shop:
                    return '$';
                case SquareFeature.Port:
                    return 'P';
                case SquareFeature.NorthAmerica:
                    return 'N';
                case SquareFeature.SouthAmerica:
                    return 'S';
                case SquareFeature.Europe:
                    return 'E';
                case SquareFeature.Africa:
                    return 'F';
                case SquareFeature.Asia:
                    return 'A';
                case SquareFeature.Australia:
                    return 'U';
                case SquareFeature.PlayerNumber:
                    return '#';
                default:
                    return '?';
            }
        }


        public Tuple<int, int> FindClosestSquareWithFeature(Player player, SquareFeature targetFeature)
        {
            if (!PlayerPositions.TryGetValue(player, out Tuple<int, int> playerPosition))
            {
                Console.WriteLine($"Player {player.Name} not found on the board.");
                return Tuple.Create(-1, -1);
            }

            int playerX = playerPosition.Item1;
            int playerY = playerPosition.Item2;
            int minDistance = int.MaxValue;
            Tuple<int, int> closestSquare = Tuple.Create(-1, -1);

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (i == playerX && j == playerY)
                        continue;

                    if (HasFeature(i, j, targetFeature))
                    {
                        int distance = Math.Abs(i - playerX) + Math.Abs(j - playerY);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestSquare = Tuple.Create(i, j);
                        }
                    }
                }
            }
            return closestSquare;
        }


        public void PlayGame()
        {
            //Console.WriteLine("------------------------------------------------------------------------------------");
            int turns = 0;
            Player winner = null;
            while (winner == null)
            {
                turns++;
                foreach (var player in Players)
                {
                    PlayerMovement.MovePlayer(Program.gameBoard, player);
                    //Console.WriteLine("------------------------------------------");
                    if (player.Stars >= 8) 
                    {
                        winner = player;
                        break; 
                    }
                }
            }

            if (winner != null)
            {
                //Console.WriteLine($"{winner.Name} has won in {turns} turns!!");
                UpdateResults(winner);
            }
            else{
                //Console.WriteLine($"No one has won in {turns} turns");
            }

            Results.totalTurns += turns;
            
            foreach (var player in Players)
            {
                //Console.WriteLine($"{player.Name} has {player.Stars} stars, {player.Coins} coins, and {player.Products.Count} products: {string.Join(", ", player.Products.Select(p => p.Name))}");
            }            
        }


        private void UpdateResults(Player winner)
        {
            if(Players[0] == winner)
            {
                Results.winsStarter++;
            }
            if (winner.Name == "NorthAmerica")
            {
                Results.winsNorthAmerica++;
            }
            else if (winner.Name == "SouthAmerica")
            {
                Results.winsSouthAmerica++;
            }
            else if (winner.Name == "Europe")
            {
                Results.winsEurope++;
            }
            else if (winner.Name == "Africa")
            {
                Results.winsAfrica++;
            }
            else if (winner.Name == "Asia")
            {
                Results.winsAsia++;
            }
            else if (winner.Name == "Australia")
            {
                Results.winsAustralia++;
            }
            else
            {
                Results.errors++;
            }
        }
    }



    public class Program
    {
        public static Board gameBoard;

        public static void Main2()
        {
            int rows = 31;
            int columns = 45;

            gameBoard = new Board(rows, columns);

            ListOfFeatures.AddListOfFeatures(gameBoard);

            Player player1 = gameBoard.AddPlayerOnRandomLandFeature(1);
            Player player2 = gameBoard.AddPlayerOnRandomLandFeature(2);
            Player player3 = gameBoard.AddPlayerOnRandomLandFeature(3);
            Player player4 = gameBoard.AddPlayerOnRandomLandFeature(4);
            Player player5 = gameBoard.AddPlayerOnRandomLandFeature(5);
            Player player6 = gameBoard.AddPlayerOnRandomLandFeature(6);

            for (int i = 0; i <= Results.plays - 1; i++)
            {
                gameBoard.RemovePlayer(player1);
                gameBoard.RemovePlayer(player2);
                gameBoard.RemovePlayer(player3);
                gameBoard.RemovePlayer(player4);
                gameBoard.RemovePlayer(player5);
                gameBoard.RemovePlayer(player6);

                player1 = gameBoard.AddPlayerOnRandomLandFeature(1);
                player2 = gameBoard.AddPlayerOnRandomLandFeature(2);
                player3 = gameBoard.AddPlayerOnRandomLandFeature(3);
                player4 = gameBoard.AddPlayerOnRandomLandFeature(4);
                player5 = gameBoard.AddPlayerOnRandomLandFeature(5);
                player6 = gameBoard.AddPlayerOnRandomLandFeature(6);
                
                player1.Reset();
                player2.Reset();
                player3.Reset();
                player4.Reset();
                player5.Reset();
                player6.Reset();

                gameBoard.ShufflePlayers();

                Tuple<int, int> player1Position = gameBoard.PlayerPositions[player1];
                Tuple<int, int> player2Position = gameBoard.PlayerPositions[player2];
                Tuple<int, int> player3Position = gameBoard.PlayerPositions[player3];
                Tuple<int, int> player4Position = gameBoard.PlayerPositions[player4];
                Tuple<int, int> player5Position = gameBoard.PlayerPositions[player5];
                Tuple<int, int> player6Position = gameBoard.PlayerPositions[player6];

                //Console.WriteLine("\nStart Board:");
                //gameBoard.PrintBoard();
                gameBoard.PlayGame();
            }

            int averageTurns = Results.totalTurns / Results.plays;

            Console.WriteLine($"\nAmount of plays: {Results.plays}");

            Console.WriteLine($"\nNorthAmerica won : {Results.winsNorthAmerica} times");
            Console.WriteLine($"SouthAmerica won : {Results.winsSouthAmerica} times")  ;
            Console.WriteLine($"Europe won       : {Results.winsEurope} times");
            Console.WriteLine($"Africa won       : {Results.winsAfrica} times");
            Console.WriteLine($"Asia won         : {Results.winsAsia} times");
            Console.WriteLine($"Australia won    : {Results.winsAustralia} times\n");
            Console.WriteLine($"So many errors   : {Results.errors}\n");
            Console.WriteLine($"Amount of 1st starter wins: {Results.winsStarter} times\n");
            Console.WriteLine($"Average amount of turns: {averageTurns}\n");
        }
    }
}
