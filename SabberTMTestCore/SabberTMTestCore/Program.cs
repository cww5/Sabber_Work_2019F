#region copyright
// SabberStone, Hearthstone Simulator in C# .NET Core
// Copyright (C) 2017-2019 SabberStone Team, darkfriend77 & rnilva
//
// SabberStone is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License.
// SabberStone is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
#endregion
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneCoreAi.Meta;
using SabberStoneCoreAi.Nodes;
using SabberStoneCoreAi.Score;

//20200203 Connor - Some of Fernando's libraries below:
using System.Collections;
using SabberStoneCore.Tasks;
//using GamePlayer.Meta;
//using GamePlayer.Nodes;
//using GamePlayer.Score;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;



/*
 * Original Author - SabberStone Developers (FullGame, etc)
 *  
 * Second Authors - Amy Hoover and Connor Watson (Utilized for their research)
 *
 * There is some code in this repo that was pulled from Fernando's code as well
 * */


namespace SabberStoneCoreAi
{
    internal class Program
    {
        private static readonly Random Rnd = new Random();



        //20200203 Connor - These parameters are for configuration purposes
        private static int maxDepth; //= 13;//maxDepth = 10 and maxWidth = 500 is optimal 
        private static int maxWidth; //= 4;//keep maxDepth high(around 13) and maxWidth very low (4) for maximum speed
                                     //private static int parallelThreads = 1;// number of parallel running threads//not important
                                     //private static int testsInEachThread = 1;//number of games in each thread//ae ere
                                     //you are advised not to set more than 3 parallel threads if you are doing this on your laptop, otherwise the laptop will not survive
                                     //private static int parallelThreadsInner = 1;//this his what is important
                                     //private static int testsInEachThreadInner = 2;//linearly

        private static string GPUID;
        private static bool parallelGames;
        private static string folderName;
        private static int numGames;
        private static int stepSize;
        private static bool record_log;
		//private static string player1_deck_file;
		//private static string player2_deck_file;
		private static string players_decks_file;
		private static string opponents_decks_file;

        // 20190128 Amy - This function takes command line arguments and parses them.
        // That is, the strings are converted to variables that the program will understand.

        #region Region: ParseArgs Function
        public static void parseArgs(string[] args)
        {
            /*
			 *This function was retrieved from Fernando
			 *
			 */
            for (int i = 0; i < args.Length; i++)
            {
				string argument = args[i];//.ToLower();

                if (argument.Contains("gpuid="))
                {
                    GPUID = argument.Substring(6);//int.Parse(argument.Substring(6)) - 1;
                    parallelGames = true;
                }
                else if (argument.Contains("folder="))
                {
                    folderName = args[i].Substring(7);
                }
                else if (argument.Contains("numgames="))
                {
                    numGames = int.Parse(argument.Substring(9));
                }
                else if (argument.Contains("stepsize="))
                {
                    stepSize = int.Parse(argument.Substring(9));
                }
                else if (argument.Contains("playerdecks="))
                { // 20200204 Connor - Get the remainder of the string string.Substring(N)
                  // Get the remainder of the string starting from index position N to the end of the string
                    players_decks_file = argument.Substring(12);
                }
                else if (argument.Contains("opponentdecks="))
                {
                    opponents_decks_file = argument.Substring(14);
                }
                else if (argument.Contains("log="))
                {
                    if (argument[4] == 't')
                    {
                        record_log = true;
                    }
                    else
                    {
                        record_log = false;
                    }
                }
                /*else if (argument.Contains("nerf="))
				{
					string nerf_data_filepath = argument.Substring(5);

					NerfCards(nerf_data_filepath);
				}*/
                else if (argument.Contains("maxwidth="))
                {
                    maxWidth = int.Parse(argument.Substring(9));
                }
                else if (argument.Contains("maxdepth="))
                {
                    maxDepth = int.Parse(argument.Substring(9));
                }
            }
        }
        #endregion
        //20200203 Connor - End ParseArgs() function from Fernando





        private static void Main(string[] args)
        {
            //20200203 Connor - Started adding parallelism code
            //20190128 Amy - No Clue
            ParallelOptions parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = 1;

            //20190128 Amy - Important variables for the execution of the code.
            //20190128 Amy - Max Depth and Max Width refer to the game tree for the AI agent
            maxDepth = 13;
            maxWidth = 4;
            GPUID = "0";
            parallelGames = false;
            folderName = "";
            numGames = 2;
            stepSize = 0;
            players_decks_file = "";
            opponents_decks_file = "";

            parseArgs(args);

            Console.WriteLine("maxDepth = " + maxDepth);
            Console.WriteLine("maxWidth = " + maxWidth);


			if (folderName == "")
			{
				string assemblyFolderName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToString();
				string[] parts = assemblyFolderName.Split("SabberTMTestCore");
				string rootFolderName = parts[0];
				folderName = rootFolderName + DateTime.Now.ToString("yyyy-MM-dd.hh.mm.ss");
			}

            if (stepSize == 0) { stepSize = numGames; }
            if (numGames < stepSize || (numGames % stepSize) > 0)
            {
                Console.WriteLine("\'numGames / stepSize\' must result in an integer bigger than 0");
                return;
            }

            int number_of_loops = numGames / stepSize;

            //20190128 Amy - Load the data we need for the list of players and opponents
            //players = getPlayersFromFile(player_decks_file);
            //opponents = getPlayersFromFile(opponent_decks_file);

            //20200203 Connor - Directory to store stuff
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            Thread.Sleep(10000);

			List<Tuple<List<Card>, string, string, string, string>> playersList;
			List<Tuple<List<Card>, string, string, string, string>> opponentsList;
			//Tuple<List<Card>, string, string, string, string> playersList;
			//Tuple<List<Card>, string, string, string, string> opponentsList;
			if ((players_decks_file != "") && (opponents_decks_file != ""))
			{

			}
			else
			{
				/*
				 * playerInfo[0] = Budget_Zoolock
				 * playerInfo[1] = Roffle
				 * playerInfo[2] = Warlock
				 * playerInfo[3] = Aggro
				 * playerInfo[4] = Flame Imp*....  (deck of cards)
				 * */
				var player1TempTup = Tuple.Create(Decks.AggroPirateWarrior, "AggroPirateWarrior1", "Pirate1", "Rogue", "Aggro");
				var player2TempTup = Tuple.Create(Decks.AggroPirateWarrior, "AggroPirateWarrior2", "Pirate2", "Rogue", "Aggro");
				playersList = new List<Tuple<List<Card>, string, string, string, string>>();
				opponentsList = new List<Tuple<List<Card>, string, string, string, string>>();
				playersList.Add(player1TempTup);
				opponentsList.Add(player1TempTup);

			}

			//__________________________________________________________-
			Tuple<List<Card>, string, string> player1Tup;
            Tuple<List<Card>, string, string> player2Tup;
            var Player1DeckList = new List<Card>();
            var Player2DeckList = new List<Card>();
            string player1Name = "FitzVonGerald";
            string player2Name = "RehHausZuckFuchs";
            string player1DeckName = "AggroPirateWarrior";
            string player2DeckName = "AggroPirateWarrior";
            if ((players_decks_file != "") && (opponents_decks_file != ""))
            {
                player1Tup = CreateDeckFromFile(player1_deck_file);
                Player1DeckList = player1Tup.Item1;
                player1Name = player1Tup.Item2;
                player1DeckName = player1Tup.Item3;
                player2Tup = CreateDeckFromFile(player1_deck_file);
                Player2DeckList = player2Tup.Item1;
                player2Name = player2Tup.Item2;
                player2DeckName = player2Tup.Item3;
            }
            else
            {
                Player1DeckList = Decks.AggroPirateWarrior;
                Player2DeckList = Decks.AggroPirateWarrior;
            }

			//__________________________________________________________-

            Console.WriteLine("Starting test setup.");

            string allGamesOutput = ""; //20200130 Connor - this is the output of all the parallel games
            if (!parallelGames)
            {
                //20200204 Connor - Here, allGamesOutput is the output of only ONE game
                allGamesOutput = FullGame(Player1DeckList, Player2DeckList, player1Name, player2Name, player1DeckName, player2DeckName);
                Console.WriteLine(allGamesOutput);

				try
				{
					string outputFileName =  (folderName + "/Output-" + player1Name + "-" + player2Name + ".txt");
					using (StreamWriter tw = File.AppendText(outputFileName))
					{//20200130 Connor - Changed this WriteLine to write the output of allGamesOutput
						tw.WriteLine(allGamesOutput);
						tw.Close();
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}





			}
            else
            {
                int j = 0;

                while (j < number_of_loops)
                {
                    bool retry = true;
                    int tries = 0;

                    while (retry)
                    {
                        try
                        {
                            //Console.WriteLine("Start Thread");
                            var thread = new Thread(() =>
                            {   //20200130 Connor - This variable is the thing that gets written to the output file
                                allGamesOutput = PlayParallelGames(Player1DeckList, Player2DeckList, player1Name, player2Name, player1DeckName, player2DeckName);
                            });

                            thread.Start();

                            bool finished = thread.Join(600000);

                            //Console.WriteLine("Thread End");

                            if (!finished)
                            {
                                retry = true;

                                tries++;
                                continue;
                            }
                            else
                            {
                                retry = false;

                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        if (tries > 3)
                        {
                            break;
                        }
                    }
                    //20200204 Connor - It looks like this is the thing that creates the nested directory...unclear if this is needed.
                    string overallGameStat = folderName + "/" + player1Name + "/" + player2Name;
                    if (!Directory.Exists(overallGameStat))
                    {
                        Directory.CreateDirectory(overallGameStat);
                    }
                    try
                    {
                        overallGameStat = overallGameStat + "/Output-" + GPUID + "-" + j + ".txt";
                        using (StreamWriter tw = File.AppendText(overallGameStat))
                        {//20200130 Connor - Changed this WriteLine to write the output of allGamesOutput
                            tw.WriteLine(allGamesOutput);
                            tw.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    j++;
                }

            }

            //OneTurn();
            //FullGame();
            //RandomGames();
            //TestFullGames();

            Console.WriteLine("Test end!");
            //Console.ReadLine();
        }

        public static string PrintEndOfTurnOptions(Game game, PlayerTask task, string allTurnTasks)
        {
            string eotbuilder = "";
            eotbuilder += ("______________________________________________________________________\n");
            eotbuilder += ($"Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState} - turn no " + game.Turn + "\n");
            //$"ROUND {(game.Turn + 1) / 2} - {game.CurrentPlayer.Name}");
            eotbuilder += ($"Hero[P1]: {game.Player1.Hero.Health} / Hero[P2]: {game.Player2.Hero.Health}\n");
            if (task != null)
            {
                eotbuilder += ($">>>>>>>>>TASK TYPE CHECK (is EOT?): {task.PlayerTaskType.Equals(PlayerTaskType.END_TURN)}\n");
                eotbuilder += ($">>>>>>>>>TASK TYPE: {task.PlayerTaskType}\n");
            }
            eotbuilder += (allTurnTasks + "\n");
            if (game.CurrentPlayer == game.Player1)
            {
                eotbuilder += ($"CURRENT PLAYER: P1 {game.CurrentPlayer.Name}\n");
            }
            else
            {
                eotbuilder += ($"CURRENT PLAYER: P2 {game.CurrentPlayer.Name}\n");
            }

            eotbuilder += ($"AMOUNTHEALEDTHISTURN {game.CurrentPlayer.AmountHeroHealedThisTurn}\n");
            eotbuilder += ($"HEROPOWERACTIVATIONSTHIS TURN {game.CurrentPlayer.HeroPowerActivationsThisTurn}\n");
            eotbuilder += ($"NUMATTACKSTHISTURN {game.CurrentPlayer.NumAttacksThisTurn}\n");
            eotbuilder += ($"NUMCARDSDRAWNTHISTURN {game.CurrentPlayer.NumCardsDrawnThisTurn}\n");
            eotbuilder += ($"NUMCARDSPLAYEDTHISTURN {game.CurrentPlayer.NumCardsPlayedThisTurn}\n");
            eotbuilder += ($"NUMCARDSTODRAW {game.CurrentPlayer.NumCardsToDraw}\n");
            eotbuilder += ($"NUMELEMENTALSPLAYEDLASTTURN {game.CurrentPlayer.NumElementalsPlayedLastTurn}\n");
            eotbuilder += ($"NUMELEMENTALSPLAYEDTHISTURN {game.CurrentPlayer.NumElementalsPlayedThisTurn}\n");
            eotbuilder += ($"NUMFRIENDLYMINIONSTHATATTACKEDTHISTURN {game.CurrentPlayer.NumFriendlyMinionsThatAttackedThisTurn}\n");
            eotbuilder += ($"NUMFRIENDLYMINIONSTHATDIEDTHISTURN {game.CurrentPlayer.NumFriendlyMinionsThatDiedThisTurn}\n");
            eotbuilder += ($"NUMMINIONSPLAYEDTHISTURN {game.CurrentPlayer.NumMinionsPlayedThisTurn}\n");
            eotbuilder += ($"NUMMINIONSPLAYERKILLEDTHISTURN {game.CurrentPlayer.NumMinionsPlayerKilledThisTurn}\n");
            eotbuilder += ($"NUMOPTIONSPLAYEDTHISTURN {game.CurrentPlayer.NumOptionsPlayedThisTurn}\n");
            eotbuilder += ($"NUMSECRETSPLAYEDTHISGAME {game.CurrentPlayer.NumSecretsPlayedThisGame}\n");
            eotbuilder += ($"NUMSPELLSPLAYEDTHISGAME {game.CurrentPlayer.NumSpellsPlayedThisGame}\n");
            eotbuilder += ($"NUMTIMESHEROPOWERUSEDTHISGAME {game.CurrentPlayer.NumTimesHeroPowerUsedThisGame}\n");
            eotbuilder += ($"REMAININGMANA {game.CurrentPlayer.RemainingMana}\n");
            eotbuilder += ($"TOTALMANASPENTTHISGAME {game.CurrentPlayer.TotalManaSpentThisGame}\n");
            eotbuilder += ($"USEDMANATHISTURN {game.CurrentPlayer.UsedMana}\n"); //This represents how much was used this turn
                                                                                 //game.CurrentPlayer.HandZone
                                                                                 //game.CurrentPlayer.BoardZone
                                                                                 //string cardsplayedthisturn = "";

            //string cardsPlayedThisTurn = "";

            foreach (Card c in game.CurrentPlayer.CardsPlayedThisTurn)
            {
                eotbuilder += ($"PlayedCard: {c.ToString()}\n");
            }
            eotbuilder += ("No more cards played this turn.\n");

            eotbuilder += ("______________________________________________________________________|\n");
            return eotbuilder;
        }

        public static string PrintDeckOfCards(List<Card> deckOfCards)
        {
            string deckbuild = "";
            deckbuild += "***********Deck of cards***********\n";
            foreach (Card card in deckOfCards)
            {
                deckbuild += (card.ToString() + "\n");
            }
            deckbuild += ("The deck size is: \n");
            deckbuild += (deckOfCards.Count.ToString() + "\n");
            deckbuild += ("***********************************\n");
            return deckbuild;
        }

        public static Tuple<List<Card>, string, string> CreateDeckFromFile(string FileName)
        {
            var deck = new List<Card>();
            string playerName = "";
            string deckName = "";
            try
            {
                string[] sep = { "><" };
                string[] headerSep = { ": " };
                short c = 2;
                string[] lines = System.IO.File.ReadAllLines(FileName);  //opens and closes the file
                foreach (string line in lines)
                {
                    if (line.Contains(">>Name:"))
                    {
                        string[] deckNameParts = line.Split(headerSep, c, StringSplitOptions.RemoveEmptyEntries);
                        deckName = deckNameParts[1];
                    }
                    else if (line.Contains(">>Author:"))
                    {
                        string[] authorParts = line.Split(headerSep, c, StringSplitOptions.RemoveEmptyEntries);
                        playerName = authorParts[1];
                    }
                    else if (line.Contains("><"))
                    {
                        //Console.WriteLine(line);
                        string[] parts = line.Split(sep, c, StringSplitOptions.RemoveEmptyEntries);
                        //Console.WriteLine(parts[0]);
                        //Console.WriteLine(parts[1]);
                        for (int i = 0; i < System.Convert.ToInt16(parts[1]); i++)
                        {
                            deck.Add(Cards.FromName(parts[0]));
                        }
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                deck = Decks.AggroPirateWarrior;
            }
            //PrintDeckOfCards(deck);
            return Tuple.Create(deck, playerName, deckName);
        }

		public static Tuple<List<Card>, string, string, string, string> CreatePlayerFromLine(string fileLine)
		{
			/* Each new entry is from the csv file we are reading from.
			 * An example is included below
			 * Budget_Zoolock; Roffle; Warlock; Aggro;Flame Imp*Flame Imp*Grim Rally*Grim Rally*Soul Infusion*Soul Infusion*Soulfire*Soulfire*Voidwalker*Voidwalker*Witchwood Imp*Witchwood Imp*Fiendish Circle*Fiendish Circle*Doubling Imp*Doubling Imp*Abusive Sergeant*Abusive Sergeant*Argent Squire*Argent Squire*Mecharoo*Mecharoo*Saronite Taskmaster*Saronite Taskmaster*Dire Wolf Alpha*Dire Wolf Alpha*Knife Juggler*Knife Juggler*Scarab Egg*Scarab Egg
			 * 
			 * Budget_Zoolock; Roffle; Warlock; Aggro;Flame Imp*....
			 * playerInfo[0] = Budget_Zoolock
			 * playerInfo[1] = Roffle
			 * playerInfo[2] = Warlock
			 * playerInfo[3] = Aggro
			 * playerInfo[4] = Flame Imp*....  (deck of cards)
			*/

			List<Card> deck = new List<Card>();

			string[] playerInfo = fileLine.Split2(';');
			string[] cards = playerInfo[4].Split2('*'); //Cards separated by *

			for (int j = 0; j < 30; j++)
			{
				deck.Add(Cards.FromName(cards[j]));
			}

			string deckName = playerInfo[0].Trim(); //name of the deck 
			string playerName = playerInfo[1].Trim(); //name of the player who created the deck
			string heroType = playerInfo[2].Trim(); //hero character chosen
			string heroStrat = playerInfo[3].Trim(); //score Function


			return Tuple.Create(deck, deckName, playerName, heroType, heroStrat);
		}

		/*
		* 20200203 - Obtained from Fernando
		*/
		public static string[] Split2(string source, char delim)
		{
			// argument null checking etc omitted for brevity
			List<string> result = new List<string>();

			int oldIndex = 0, newIndex;
			while ((newIndex = source.IndexOf(delim, oldIndex)) != -1)
			{
				result.Add(source.Substring(oldIndex, newIndex - oldIndex));
				oldIndex = newIndex + 1;//delim.Length;
			}
			result.Add(source.Substring(oldIndex));

			return result.ToArray();
		}


		public static List<Tuple<List<Card>, string, string, string, string>> GetPlayersFromFile(string path)
		{
			/*
			 * Initial code received from Fernando - This takes a csv file and parses each line into a List of players + their attributes
			 * -- Input : path - a path to a file
			 * -- Output : List of Tuples: <DeckList, DeckName, PlayerName, HeroCharacter, ScoreStrategy>
			 */
			List<Tuple<List<Card>, string, string, string, string>> result = new List<Tuple<List<Card>, string, string, string, string>>();

			string[] file_data = System.IO.File.ReadAllLines(path);

			for (int i = 0; i < file_data.Length; i++)
			{
				string playerLine = file_data[i];
				Tuple<List<Card>, string, string, string, string> playerTup = CreatePlayerFromLine(playerLine);
				result.Add(playerTup);
			}

			return result;
		}
		public static string PlayParallelGames(List<Card> Player1Cards, List<Card> Player2Cards, string PlayerOneName, string PlayerTwoName, string P1DeckName, string P2DeckName)
        {
            //20200204 Connor - I believe wins is not needed...
            //int[] wins = Enumerable.Repeat(0, stepSize).ToArray();

            ParallelOptions parallel_options = new ParallelOptions();
            parallel_options.MaxDegreeOfParallelism = 8;// parallelThreads;// Environment.ProcessorCount;//parallelThreadsInner+10;
                                                        // Console.WriteLine(Environment.ProcessorCountCount);

            string[] game_log_list = new string[stepSize];
            //20200130 Connor - This is the list of games and each should contain all the game info
            for (int k = 0; k < game_log_list.Length; k++)
            {
                game_log_list[k] = "";
            }

            string res = "";
            Parallel.For(0, stepSize, parallel_options, j =>//parallelThreadsInner * testsInEachThreadInner, parallel_options, j =>
            {
                /*20190130 Connor - Run a Parallel For loop to run FullGame on separate threads.
				 * OUTPUT - the resulting string to write to the output file
				 */
                int i = j;
                //Console.WriteLine(i);

                string s = "";
                string game_log = "";
                bool retry = true;

                while (retry)
                {
                    try
                    {
                        //Console.WriteLine("Start Game!");
                        s = FullGame(Player1Cards, Player2Cards, PlayerOneName, PlayerTwoName, P1DeckName, P2DeckName);
                        game_log_list[j] = game_log;
                        //Console.WriteLine("Game End!");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        s = e.Message.ToString();
                    }

                    if (s.ToLower().Contains("present") || s.ToLower().Contains("instance") || s.ToLower().Contains("zone"))
                    {
                        Console.WriteLine("this was s=" + s + "retrying right here");

                        retry = true;
                    }
                    else
                    {
                        retry = false;
                    }
                }
            });
            //_______________________________________________________________________________
            //20200130 Connor - Added this little block of code to get all the games returned
            string allGames = "";
            for (int k = 0; k < game_log_list.Length; k++)
            {
                allGames += (game_log_list[k] + "\n");
            }

            return allGames;
        }


        public static string FullGame(List<Card> Player1Cards, List<Card> Player2Cards, string PlayerOneName, string PlayerTwoName, string P1DeckName, string P2DeckName)
        {
            var game = new Game(
                new GameConfig()
                {
                    StartPlayer = 1,
                    Player1Name = PlayerOneName,
                    Player1HeroClass = CardClass.WARLOCK,
                    //Player1Deck = Decks.AggroPirateWarrior,
                    Player1Deck = Player1Cards,
                    Player2Name = PlayerTwoName,
                    Player2HeroClass = CardClass.WARLOCK,
                    //Player2Deck = Decks.AggroPirateWarrior,
                    Player2Deck = Player2Cards,
                    FillDecks = false,
                    Shuffle = true,
                    SkipMulligan = false
                });

            //20200130 Connor - Changing Console.WriteLine() calls to agree with logsbuild
            string logbuild = "";
            logbuild += "+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+\n";
            logbuild += $"Player1: {game.Player1.Name} vs Player2: {game.Player2.Name}\n";
            logbuild += $"Player1Deck: {P1DeckName} vs Player2Deck: {P2DeckName}\n";

            logbuild += PrintDeckOfCards(Player1Cards);
            logbuild += PrintDeckOfCards(Player2Cards);
            //Console.WriteLine(logbuild);


            game.StartGame();

            var aiPlayer1 = new AggroScore();
            var aiPlayer2 = new AggroScore();

            List<int> mulligan1 = aiPlayer1.MulliganRule().Invoke(game.Player1.Choice.Choices.Select(p => game.IdEntityDic[p]).ToList());
            List<int> mulligan2 = aiPlayer2.MulliganRule().Invoke(game.Player2.Choice.Choices.Select(p => game.IdEntityDic[p]).ToList());

            logbuild += ($"Player1: Mulligan {String.Join(",", mulligan1)}\n");
            logbuild += ($"Player2: Mulligan {String.Join(",", mulligan2)}\n");

            game.Process(ChooseTask.Mulligan(game.Player1, mulligan1));
            game.Process(ChooseTask.Mulligan(game.Player2, mulligan2));

            game.MainReady();

            while (game.State != State.COMPLETE)
            {
                //Console.WriteLine("");
                //Console.WriteLine($"Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState} - " +
                //				  $"ROUND {(game.Turn + 1) / 2} - {game.CurrentPlayer.Name}");
                //Console.WriteLine($"Hero[P1]: {game.Player1.Hero.Health} / Hero[P2]: {game.Player2.Hero.Health}");
                //Console.WriteLine("");
                logbuild += "\n";
                logbuild += ($"Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState} - " +
                                    $"ROUND {(game.Turn + 1) / 2} - {game.CurrentPlayer.Name}\n");
                logbuild += ($"Hero[P1]: {game.Player1.Hero.Health} / Hero[P2]: {game.Player2.Hero.Health}\n");
                logbuild += "\n";

                while (game.State == State.RUNNING && game.CurrentPlayer == game.Player1)
                {
                    //Console.WriteLine($"* Calculating solutions *** Player 1 ***");
                    logbuild += $"* Calculating solutions *** Player 1 ***\n";
                    List<OptionNode> solutions = OptionNode.GetSolutions(game, game.Player1.Id, aiPlayer1, 10, 500);
                    var solution = new List<PlayerTask>();
                    solutions.OrderByDescending(p => p.Score).First().PlayerTasks(ref solution);
                    //Console.WriteLine($"- Player 1 - <{game.CurrentPlayer.Name}> ---------------------------");
                    logbuild += $"- Player 1 - <{game.CurrentPlayer.Name}> ---------------------------\n";

                    string allTasks = "ALL TURN TASKS:\n";
                    foreach (PlayerTask task in solution)
                    {
                        string printedTask = task.FullPrint() + "\n";
                        allTasks += printedTask;
                        logbuild += (printedTask);
                        if (task.PlayerTaskType.Equals(PlayerTaskType.END_TURN))
                        {
                            allTasks += "COMPLETE ALL TURN TASKS";
                            logbuild += PrintEndOfTurnOptions(game, task, allTasks);
                        }
                        game.Process(task);
                        if (game.CurrentPlayer.Choice != null)
                        {
                            logbuild += ($"* Recaclulating due to a final solution ...\n");
                            break;
                        }
                        if (game.Player1.Hero.Health <= 0 || game.Player2.Hero.Health <= 0)
                        {
                            allTasks += "COMPLETE ALL TURN TASKS";
                            logbuild += PrintEndOfTurnOptions(game, task, allTasks);
                        }
                    }
                    //Console.WriteLine("Testing>>>>>>>>>>>>>>>>>>>>>");
                    //Console.WriteLine(game.CurrentPlayer.NumCardsPlayedThisTurn);
                }

                // Random mode for Player 2
                logbuild += ($"- Player 2 - <{game.CurrentPlayer.Name}> ---------------------------\n");
                while (game.State == State.RUNNING && game.CurrentPlayer == game.Player2)
                {
                    //var options = game.Options(game.CurrentPlayer);
                    //var option = options[Rnd.Next(options.Count)];
                    //Log.Info($"[{option.FullPrint()}]");
                    //game.Process(option);
                    logbuild += ($"* Calculating solutions *** Player 2 ***\n");
                    List<OptionNode> solutions = OptionNode.GetSolutions(game, game.Player2.Id, aiPlayer2, 10, 500);
                    var solution = new List<PlayerTask>();
                    solutions.OrderByDescending(p => p.Score).First().PlayerTasks(ref solution);
                    logbuild += ($"- Player 2 - <{game.CurrentPlayer.Name}> ---------------------------\n");
                    string allTasks = "ALL TURN TASKS:\n";
                    foreach (PlayerTask task in solution)
                    {
                        string printedTask = task.FullPrint() + "\n";
                        allTasks += printedTask;
                        logbuild += (printedTask);
                        if (task.PlayerTaskType.Equals(PlayerTaskType.END_TURN))
                        {
                            allTasks += "COMPLETE ALL TURN TASKS";
                            logbuild += PrintEndOfTurnOptions(game, task, allTasks);
                        }
                        game.Process(task);
                        if (game.CurrentPlayer.Choice != null)
                        {
                            logbuild += ($"* Recaclulating due to a final solution ...\n");
                            break;
                        }
                        if (game.Player1.Hero.Health <= 0 || game.Player2.Hero.Health <= 0)
                        {
                            allTasks = allTasks + "COMPLETE ALL TURN TASKS";
                            logbuild += PrintEndOfTurnOptions(game, task, allTasks);
                        }
                    }
                }
            }
            logbuild += ($"Game: {game.State}, Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState}\n");
            logbuild += "+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+\n";
            return logbuild;
        }
    }
}
