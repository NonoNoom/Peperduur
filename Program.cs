using System;
using System.Data;
using MyBoard;
using MyFeatures;

namespace MyGame{
    public class Play
    {
        public static void Main(string[] args)
        {
            Program.Main2();
        }
    }


    public static class PlayerMovement
    {
        public static void MovePlayer(Board gameBoard, Player player)
        {
            if (gameBoard.PlayerPositions.TryGetValue(player, out Tuple<int, int> playerPosition))
            {
                int steps = Throw();
                //Console.WriteLine($"{player.Name} throws {steps} steps.");

                Board.SquareFeature[] features;
                
                int minDistance = int.MaxValue;
                Board.SquareFeature nearestFeature = Board.SquareFeature.Shop;

                if (player.Coins < 3 || (player.Products.Count == 0 && player.Coins < 6))
                {
                    features = new Board.SquareFeature[] {Board.SquareFeature.Shop};
                    //Console.WriteLine("Has to go to Shop");
                }
                else if ((player.Products.Count == 0 && (player.Coins == 6 || player.Coins == 7)) || (player.Products.Count > 0 && (player.Coins == 3 || player.Coins == 4)))
                {
                    features = new Board.SquareFeature[] {Board.SquareFeature.Shop, Board.SquareFeature.Star1};
                    //Console.WriteLine("Has to go to Shop, Star1");
                }
                else if ((player.Products.Count == 0 && player.Coins == 8) || (player.Products.Count > 0 && player.Coins == 5))
                {
                    features = new Board.SquareFeature[] {Board.SquareFeature.Shop, Board.SquareFeature.Star1, Board.SquareFeature.Star2};
                    //Console.WriteLine("Has to go to Shop, Star1, Star2");
                }
                else if (player.Products.Count > 0 && (player.Coins == 6 || player.Coins == 7 || player.Coins == 8))
                {
                    features = new Board.SquareFeature[] {Board.SquareFeature.Shop, Board.SquareFeature.Star1, Board.SquareFeature.Star2, Board.SquareFeature.Star3};
                    //Console.WriteLine("Has to go to Shop, Star1, Star2, Star3");
                }
                else if (player.Coins > 8)
                {
                    features = new Board.SquareFeature[] {Board.SquareFeature.Star1, Board.SquareFeature.Star2, Board.SquareFeature.Star3};
                    //Console.WriteLine("Has to go to Star1, Star2, Star3");
                }
                else
                {
                    features = new Board.SquareFeature[] {Board.SquareFeature.Shop, Board.SquareFeature.Star1, Board.SquareFeature.Star2, Board.SquareFeature.Star3};
                    //Console.WriteLine("Can choose");
                }

                foreach (var feature in features)
                {
                    int distance = TotalDifference(gameBoard, player, feature);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestFeature = feature;
                    }
                }

                Tuple<int, int> destination = CalculateDestination(gameBoard, player, nearestFeature, steps);

                if (destination.Item1 != playerPosition.Item1 || destination.Item2 != playerPosition.Item2)
                {
                    //Console.WriteLine($"The distance for {nearestFeature} is {TotalDifference(gameBoard, player, nearestFeature)} and is on sea: {!gameBoard.HasFeature(destination.Item1, destination.Item2, Board.SquareFeature.Land)}");
                    MovePlayerToDestination(gameBoard, player, nearestFeature, destination);
                }
            }
            else
            {
                Console.WriteLine($"Player {player.Name} not found on the board.");
            }
        }


        public static Tuple<int, int> CalculateDestination(Board gameBoard, Player player, Board.SquareFeature feature, int steps)
        {
            if (gameBoard.PlayerPositions.TryGetValue(player, out Tuple<int, int> playerPosition))
            {
                Tuple<int, int> targetSquare = gameBoard.FindClosestSquareWithFeature(player, feature);
                //Console.WriteLine($"{player.Name} wants to go to {feature} at {targetSquare}");

                // Check if the feature is on the sea and player doesn't have permission to cross
                if (!gameBoard.HasFeature(targetSquare.Item1, targetSquare.Item2, Board.SquareFeature.Land) && !player.OnSea)
                {
                    // Find the closest port and calculate the destination based on it
                    Tuple<int, int> closestPort = gameBoard.FindClosestSquareWithFeature(player, Board.SquareFeature.Port);
                    int rowDifference = closestPort.Item1 - playerPosition.Item1;
                    int colDifference = closestPort.Item2 - playerPosition.Item2;
                    int newRow = playerPosition.Item1 + Math.Min(steps, Math.Abs(rowDifference)) * Math.Sign(rowDifference);
                    int newCol = playerPosition.Item2 + Math.Min(steps, Math.Abs(colDifference)) * Math.Sign(colDifference);

                    targetSquare = Tuple.Create(newRow, newCol);
                }
                // Check if the player is on the sea and wants to reach a feature on land
                else if (gameBoard.HasFeature(targetSquare.Item1, targetSquare.Item2, Board.SquareFeature.Land) && player.OnSea)
                {
                    // Find the closest port and calculate the destination based on it
                    Tuple<int, int> closestPort = gameBoard.FindClosestSquareWithFeature(player, Board.SquareFeature.Port);
                    int rowDifference = closestPort.Item1 - playerPosition.Item1;
                    int colDifference = closestPort.Item2 - playerPosition.Item2;
                    int newRow = playerPosition.Item1 + Math.Min(steps, Math.Abs(rowDifference)) * Math.Sign(rowDifference);
                    int newCol = playerPosition.Item2 + Math.Min(steps, Math.Abs(colDifference)) * Math.Sign(colDifference);

                    targetSquare = Tuple.Create(newRow, newCol);
                }
                else
                {
                    // Calculate the destination without closest feature on the sea
                    int rowDifference = targetSquare.Item1 - playerPosition.Item1;
                    int colDifference = targetSquare.Item2 - playerPosition.Item2;
                    int newRow = playerPosition.Item1 + Math.Min(steps, Math.Abs(rowDifference)) * Math.Sign(rowDifference);
                    int newCol = playerPosition.Item2 + Math.Min(steps, Math.Abs(colDifference)) * Math.Sign(colDifference);

                    targetSquare = Tuple.Create(newRow, newCol);
                }
                return targetSquare;
            }
            return Tuple.Create(-1, -1);
        }


        private static int TotalDifference(Board gameBoard, Player player, Board.SquareFeature feature)
        {
            if (gameBoard.PlayerPositions.TryGetValue(player, out Tuple<int, int> playerPosition))
            {
                Tuple<int, int> targetSquare = gameBoard.FindClosestSquareWithFeature(player, feature);

                int totalDifference = 0;

                // Check if the feature is on the sea and player doesn't have permission to cross
                if (!gameBoard.HasFeature(targetSquare.Item1, targetSquare.Item2, Board.SquareFeature.Land) && !player.OnSea)
                {
                    // Find the closest port and calculate the distance to it
                    Tuple<int, int> closestPort = gameBoard.FindClosestSquareWithFeature(player, Board.SquareFeature.Port);
                    int rowDifferenceToPort = closestPort.Item1 - playerPosition.Item1;
                    int colDifferenceToPort = closestPort.Item2 - playerPosition.Item2;
                    int distanceToPort = Math.Abs(rowDifferenceToPort) + Math.Abs(colDifferenceToPort);

                    // Find the target square and calculate the distance from the port to it
                    targetSquare = gameBoard.FindClosestSquareWithFeature(player, feature);
                    int rowDifferenceFromPort = targetSquare.Item1 - closestPort.Item1;
                    int colDifferenceFromPort = targetSquare.Item2 - closestPort.Item2;
                    int distanceFromPortToSquare = Math.Abs(rowDifferenceFromPort) + Math.Abs(colDifferenceFromPort);

                    // Total difference is the sum of distances from player to port and port to square
                    totalDifference = distanceToPort + distanceFromPortToSquare;

                    return totalDifference;
                }
                else if (gameBoard.HasFeature(targetSquare.Item1, targetSquare.Item2, Board.SquareFeature.Land) && player.OnSea)
                {
                    // Find the closest port and calculate the distance to it
                    Tuple<int, int> closestPort = gameBoard.FindClosestSquareWithFeature(player, Board.SquareFeature.Port);
                    int rowDifferenceToPort = closestPort.Item1 - playerPosition.Item1;
                    int colDifferenceToPort = closestPort.Item2 - playerPosition.Item2;
                    int distanceToPort = Math.Abs(rowDifferenceToPort) + Math.Abs(colDifferenceToPort);

                    // Find the closest port and calculate the distance from it to the target square
                    targetSquare = gameBoard.FindClosestSquareWithFeature(player, feature);
                    int rowDifferenceFromPort = targetSquare.Item1 - closestPort.Item1;
                    int colDifferenceFromPort = targetSquare.Item2 - closestPort.Item2;
                    int distanceFromPortToSquare = Math.Abs(rowDifferenceFromPort) + Math.Abs(colDifferenceFromPort);

                    // Total difference is the sum of distances from player to port and port to square
                    totalDifference = distanceToPort + distanceFromPortToSquare;

                    return totalDifference;
                }
                else
                {
                    // Calculate the distance to the target feature without closest feature on the sea
                    int rowDifference = targetSquare.Item1 - playerPosition.Item1;
                    int colDifference = targetSquare.Item2 - playerPosition.Item2;

                    totalDifference = Math.Abs(rowDifference) + Math.Abs(colDifference);

                    return totalDifference;
                }
            }
            return 0;
        }


        private static void MovePlayerToDestination(Board gameBoard, Player player, Board.SquareFeature feature, Tuple<int, int> destination)
        {
            if (gameBoard.PlayerPositions.TryGetValue(player, out Tuple<int, int> playerPosition))
            {
                // Get the feature of the current square
                Board.SquareFeature currentSquareFeature = gameBoard.Squares[playerPosition.Item1, playerPosition.Item2][0];

                int rowDifference = destination.Item1 - playerPosition.Item1;
                int colDifference = destination.Item2 - playerPosition.Item2;

                int newRow = destination.Item1;
                int newCol = destination.Item2;
                
                gameBoard.RemoveFeature(playerPosition.Item1, playerPosition.Item2, Board.SquareFeature.PlayerNumber);

                Board.SquareFeature destinationSquareFeature = Board.SquareFeature.Land;

                // Check for features on the destination square based on priority
                foreach (var featureObject in Enum.GetValues(typeof(Board.SquareFeature)))
                {
                    Board.SquareFeature currentFeature = (Board.SquareFeature)featureObject;
                    if (gameBoard.HasFeature(newRow, newCol, currentFeature))
                    {
                        destinationSquareFeature = currentFeature;
                        ApplyFeatureEffect(player, currentFeature, gameBoard);
                        break;
                    }
                }

                gameBoard.AddFeature(newRow, newCol, Board.SquareFeature.PlayerNumber);
                gameBoard.PlayerPositions[player] = Tuple.Create(newRow, newCol);

                // Check if the player lands on a shop square
                if (feature == Board.SquareFeature.Shop)
                {
                    // Get the position of the current shop
                    Tuple<int, int> shopPosition = Tuple.Create(playerPosition.Item1, playerPosition.Item2);

                    // Check if the player has products to sell
                    if (player.Products.Count > 0)
                    {
                        SellProduct(player, gameBoard, shopPosition);
                    }
                    else
                    {
                        // Randomly select a product to buy
                        Random rnd = new Random();
                        int index = rnd.Next(GameAssets.productCards.Count);

                        // Check if the player has enough coins to buy the product
                        if (player.Coins >= GameAssets.productCards[index].PurchasePrice)
                        {
                            BuyProduct(player, index, shopPosition);
                        }
                        else
                        {
                            //Console.WriteLine("You don't have enough coins to buy a product.");
                        }
                    }
                }

                //Console.WriteLine($"{player.Name} was at {playerPosition} that was on sea: {player.OnSea}");
                //Console.WriteLine($"{player.Name} moved to {destinationSquareFeature} at {destination}");
                //Console.WriteLine($"{player.Name} now has {player.Stars} stars, {player.Coins} coins, and {player.Products.Count} products: {string.Join(", ", player.Products.Select(p => p.Name))}");

                //Console.WriteLine("\nUpdated Board:");
                //gameBoard.PrintBoard();
            }
        }


        private static void ApplyFeatureEffect(Player player, Board.SquareFeature feature, Board gameBoard)
        {
            switch (feature)
            {
                case Board.SquareFeature.Star3:
                    if(player.Coins >= 6)
                    {
                        player.Coins -= 6;
                        player.Stars += 3;
                    }
                    else
                    {
                        Console.WriteLine("You don't have enough coins to buy 3 stars.");
                    }
                    break;

                case Board.SquareFeature.Star2:
                    if(player.Coins > 4)
                    {
                        player.Coins -= 5;
                        player.Stars += 2;
                    }
                    else
                    {
                        //Console.WriteLine("You don't have enough coins to buy 2 stars.");
                        //Console.WriteLine($"{player.Coins} coins, {player.Products.Count} products");
                    }
                    break;

                case Board.SquareFeature.Star1:
                    if(player.Coins > 2)
                    {
                        player.Coins -= 3;
                        player.Stars += 1;
                    }
                    else
                    {
                        //Console.WriteLine("You don't have enough coins to buy 1 star.");
                        //Console.WriteLine($"{player.Coins} coins, {player.Products.Count} products");
                    }
                    break;
                    
                case Board.SquareFeature.Port:
                    player.OnSea = !player.OnSea;
                    //Console.WriteLine($"{player.Name} can go to the sea: {player.OnSea}");
                    break;
            }
        }


        private static void BuyProduct(Player player, int index, Tuple<int, int> shopPosition)
        {
            Product boughtProduct = GameAssets.productCards[index];
            boughtProduct.ShopPosition = shopPosition;
            player.Coins -= boughtProduct.PurchasePrice;
            player.Products.Add(boughtProduct);
            //Console.WriteLine($"{player.Name} bought {boughtProduct.Name} for {boughtProduct.PurchasePrice} coins.");
        }


        private static void SellProduct(Player player, Board gameBoard, Tuple<int, int> currentShopPosition)
        {
            if (player.Products.Count > 0)
            {
                Product soldProduct = player.Products[0];
                if (soldProduct.ShopPosition != currentShopPosition)
                {
                    player.Coins += soldProduct.SellingPrice;
                    player.Products.RemoveAt(0);
                    //Console.WriteLine($"{soldProduct.Name} was bought at {currentShopPosition} and is sold at {gameBoard.PlayerPositions[player]}");
                    //Console.WriteLine($"{player.Name} sold {soldProduct.Name} for {soldProduct.SellingPrice} coins.");
                }
                else
                {
                    Console.WriteLine($"You cannot sell {soldProduct.Name} at the same shop where you bought it.");
                }
            }
            else
            {
                Console.WriteLine("You don't have any products to sell.");
            }
        }


        public static int Throw()
        {
            Random rnd = new Random();
            int dice = rnd.Next(1, 7) + rnd.Next(1, 7);
            return dice;
        }
    }
}
