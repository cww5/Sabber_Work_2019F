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



/*
 * Original Author - SabberStone Developers (FullGame, etc)
 *  
 * Second Author - Connor Watson (Utilized for his research)
 * 
 * */


public static class StringExtensions
{
	/*
	 * 20200203 - Obtained from Fernando
	 */
	public static string[] Split2(this string source, char delim)
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
}




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
		private static string folderName;
		private static int numGames;
		private static int stepSize;
		private static bool record_log;
		private static string player_decks_file;
		private static string opponent_decks_file;

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
				string argument = args[i].ToLower();

				if (argument.Contains("gpuid="))
				{
					GPUID = argument.Substring(6);//int.Parse(argument.Substring(6)) - 1;
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
				{
					player_decks_file = argument.Substring(12);
				}
				else if (argument.Contains("opponentdecks="))
				{
					opponent_decks_file = argument.Substring(14);
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
			//Console.WriteLine("Argument length: " + args.Length);
			//Console.WriteLine("Supplied Arguments are:");
			/*foreach (object obj in args)
			{
				Console.WriteLine(obj);
			}*/

			//20200203 Connor - Started adding parallelism code
			//20190128 Amy - No Clue
			ParallelOptions parallelOptions = new ParallelOptions();
			parallelOptions.MaxDegreeOfParallelism = 1;






			Tuple<List<Card>, string> player1Tup;
			Tuple<List<Card>, string> player2Tup;
			var Player1DeckList = new List<Card>();
			var Player2DeckList = new List<Card>();
			string player1Name = "FitzVonGerald";
			string player2Name = "RehHausZuckFuchs";
			if (args.Length > 1)
			{
				player1Tup = CreateDeckFromFile(args[1]);
				Player1DeckList = player1Tup.Item1;
				player1Name = player1Tup.Item2;
				player2Tup = CreateDeckFromFile(args[2]);
				Player2DeckList = player2Tup.Item1;
				player2Name = player2Tup.Item2;
			}
			else
			{
				Player1DeckList = Decks.AggroPirateWarrior;
				Player2DeckList = Decks.AggroPirateWarrior;
			}

			Console.WriteLine("Starting test setup.");

			//OneTurn();
			//FullGame();
			FullGame(Player1DeckList, Player2DeckList, player1Name, player2Name);
			//RandomGames();
			//TestFullGames();

			Console.WriteLine("Test end!");
			//Console.ReadLine();
		}

		/*public static void PrintEndOfTurnOptions(Game game, PlayerTask task, string allTurnTasks)
		{
			Console.WriteLine("______________________________________________________________________");
			Console.WriteLine($"Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState} - turn no " + game.Turn);
			//$"ROUND {(game.Turn + 1) / 2} - {game.CurrentPlayer.Name}");
			Console.WriteLine($"Hero[P1]: {game.Player1.Hero.Health} / Hero[P2]: {game.Player2.Hero.Health}");
			if (task != null)
			{
				Console.WriteLine($">>>>>>>>>TASK TYPE CHECK (is EOT?): {task.PlayerTaskType.Equals(PlayerTaskType.END_TURN)}");
				Console.WriteLine($">>>>>>>>>TASK TYPE: {task.PlayerTaskType}");
			}
			Console.WriteLine(allTurnTasks);
			if (game.CurrentPlayer == game.Player1)
			{
				Console.WriteLine($"CURRENT PLAYER: P1 {game.CurrentPlayer.Name}");
			}
			else
			{
				Console.WriteLine($"CURRENT PLAYER: P2 {game.CurrentPlayer.Name}");
			}
			
			Console.WriteLine($"AMOUNTHEALEDTHISTURN {game.CurrentPlayer.AmountHeroHealedThisTurn}");
			Console.WriteLine($"HEROPOWERACTIVATIONSTHIS TURN {game.CurrentPlayer.HeroPowerActivationsThisTurn}");
			Console.WriteLine($"NUMATTACKSTHISTURN {game.CurrentPlayer.NumAttacksThisTurn}");
			Console.WriteLine($"NUMCARDSDRAWNTHISTURN {game.CurrentPlayer.NumCardsDrawnThisTurn}");
			Console.WriteLine($"NUMCARDSPLAYEDTHISTURN {game.CurrentPlayer.NumCardsPlayedThisTurn}");
			Console.WriteLine($"NUMCARDSTODRAW {game.CurrentPlayer.NumCardsToDraw}");
			Console.WriteLine($"NUMELEMENTALSPLAYEDLASTTURN {game.CurrentPlayer.NumElementalsPlayedLastTurn}");
			Console.WriteLine($"NUMELEMENTALSPLAYEDTHISTURN {game.CurrentPlayer.NumElementalsPlayedThisTurn}");
			Console.WriteLine($"NUMFRIENDLYMINIONSTHATATTACKEDTHISTURN {game.CurrentPlayer.NumFriendlyMinionsThatAttackedThisTurn}");
			Console.WriteLine($"NUMFRIENDLYMINIONSTHATDIEDTHISTURN {game.CurrentPlayer.NumFriendlyMinionsThatDiedThisTurn}");
			Console.WriteLine($"NUMMINIONSPLAYEDTHISTURN {game.CurrentPlayer.NumMinionsPlayedThisTurn}");
			Console.WriteLine($"NUMMINIONSPLAYERKILLEDTHISTURN {game.CurrentPlayer.NumMinionsPlayerKilledThisTurn}");
			Console.WriteLine($"NUMOPTIONSPLAYEDTHISTURN {game.CurrentPlayer.NumOptionsPlayedThisTurn}");
			Console.WriteLine($"NUMSECRETSPLAYEDTHISGAME {game.CurrentPlayer.NumSecretsPlayedThisGame}");
			Console.WriteLine($"NUMSPELLSPLAYEDTHISGAME {game.CurrentPlayer.NumSpellsPlayedThisGame}");
			Console.WriteLine($"NUMTIMESHEROPOWERUSEDTHISGAME {game.CurrentPlayer.NumTimesHeroPowerUsedThisGame}");
			Console.WriteLine($"REMAININGMANA {game.CurrentPlayer.RemainingMana}");
			Console.WriteLine($"TOTALMANASPENTTHISGAME {game.CurrentPlayer.TotalManaSpentThisGame}");
			Console.WriteLine($"USEDMANATHISTURN {game.CurrentPlayer.UsedMana}"); //This represents how much was used this turn
																				  //game.CurrentPlayer.HandZone
																				  //game.CurrentPlayer.BoardZone
																				  //string cardsplayedthisturn = "";

			//string cardsPlayedThisTurn = "";

			foreach(Card c in game.CurrentPlayer.CardsPlayedThisTurn)
			{
				Console.WriteLine($"PlayedCard: {c.ToString()}");
			}
			Console.WriteLine("No more cards played this turn.");

			Console.WriteLine("______________________________________________________________________|");
		}
		*/

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

			foreach(Card c in game.CurrentPlayer.CardsPlayedThisTurn)
			{
				eotbuilder += ($"PlayedCard: {c.ToString()}\n");
			}
			eotbuilder += ("No more cards played this turn.\n");

			eotbuilder += ("______________________________________________________________________|\n");
			return eotbuilder;
		}

		/*public static void PrintDeckOfCards(List<Card> deckOfCards)
		{
			Console.WriteLine("***********Deck of cards***********");
			foreach (Card card in deckOfCards)
			{
				Console.WriteLine(card);
			}
			Console.WriteLine("The deck size is: ");
			Console.WriteLine(deckOfCards.Count);
			Console.WriteLine("***********************************");

		}
		*/
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


		//public static List<Card> CreateDeckFromFile(string FileName)
		public static Tuple<List<Card>, string> CreateDeckFromFile(string FileName)
		{
			var deck = new List<Card>();
			string playerName = "";
			try
			{
				string[] sep = { "><" };
				string[] headerSep = { " " };
				short c = 2;
				string[] lines = System.IO.File.ReadAllLines(FileName);  //opens and closes the file
				foreach (string line in lines)
				{
					if (line.Contains(">>Author:"))
					{
						string[] authorParts = line.Split(headerSep, c, StringSplitOptions.RemoveEmptyEntries);
						playerName = authorParts[1];
					}
					else if (line.Contains("><"))
					{
						//Console.WriteLine(line);
						string[] parts = line.Split(sep, c, StringSplitOptions.RemoveEmptyEntries);
						Console.WriteLine(parts[0]);
						Console.WriteLine(parts[1]);
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
			return Tuple.Create(deck, playerName);
		}

		public static void RandomGames()
		{
			int total = 1000;
			var watch = Stopwatch.StartNew();

			var gameConfig = new GameConfig()
			{
				StartPlayer = -1,
				Player1Name = "FitzVonGerald",
				Player1HeroClass = CardClass.PALADIN,
				Player1Deck = new List<Card>()
						{
						Cards.FromName("Blessing of Might"),
						Cards.FromName("Blessing of Might"),
						Cards.FromName("Gnomish Inventor"),
						Cards.FromName("Gnomish Inventor"),
						Cards.FromName("Goldshire Footman"),
						Cards.FromName("Goldshire Footman"),
						Cards.FromName("Hammer of Wrath"),
						Cards.FromName("Hammer of Wrath"),
						Cards.FromName("Hand of Protection"),
						Cards.FromName("Hand of Protection"),
						Cards.FromName("Holy Light"),
						Cards.FromName("Holy Light"),
						Cards.FromName("Ironforge Rifleman"),
						Cards.FromName("Ironforge Rifleman"),
						Cards.FromName("Light's Justice"),
						Cards.FromName("Light's Justice"),
						Cards.FromName("Lord of the Arena"),
						Cards.FromName("Lord of the Arena"),
						Cards.FromName("Nightblade"),
						Cards.FromName("Nightblade"),
						Cards.FromName("Raid Leader"),
						Cards.FromName("Raid Leader"),
						Cards.FromName("Stonetusk Boar"),
						Cards.FromName("Stonetusk Boar"),
						Cards.FromName("Stormpike Commando"),
						Cards.FromName("Stormpike Commando"),
						Cards.FromName("Stormwind Champion"),
						Cards.FromName("Stormwind Champion"),
						Cards.FromName("Stormwind Knight"),
						Cards.FromName("Stormwind Knight")
						},
				Player2Name = "RehHausZuckFuchs",
				Player2HeroClass = CardClass.PALADIN,
				Player2Deck = new List<Card>()
						{
						Cards.FromName("Blessing of Might"),
						Cards.FromName("Blessing of Might"),
						Cards.FromName("Gnomish Inventor"),
						Cards.FromName("Gnomish Inventor"),
						Cards.FromName("Goldshire Footman"),
						Cards.FromName("Goldshire Footman"),
						Cards.FromName("Hammer of Wrath"),
						Cards.FromName("Hammer of Wrath"),
						Cards.FromName("Hand of Protection"),
						Cards.FromName("Hand of Protection"),
						Cards.FromName("Holy Light"),
						Cards.FromName("Holy Light"),
						Cards.FromName("Ironforge Rifleman"),
						Cards.FromName("Ironforge Rifleman"),
						Cards.FromName("Light's Justice"),
						Cards.FromName("Light's Justice"),
						Cards.FromName("Lord of the Arena"),
						Cards.FromName("Lord of the Arena"),
						Cards.FromName("Nightblade"),
						Cards.FromName("Nightblade"),
						Cards.FromName("Raid Leader"),
						Cards.FromName("Raid Leader"),
						Cards.FromName("Stonetusk Boar"),
						Cards.FromName("Stonetusk Boar"),
						Cards.FromName("Stormpike Commando"),
						Cards.FromName("Stormpike Commando"),
						Cards.FromName("Stormwind Champion"),
						Cards.FromName("Stormwind Champion"),
						Cards.FromName("Stormwind Knight"),
						Cards.FromName("Stormwind Knight")
						},
				FillDecks = false,
				Shuffle = true,
				SkipMulligan = false,
				Logging = false,
				History = false
			};

			int turns = 0;
			int[] wins = new[] { 0, 0 };
			for (int i = 0; i < total; i++)
			{
				var game = new Game(gameConfig);
				game.StartGame();

				game.Process(ChooseTask.Mulligan(game.Player1, new List<int>()));
				game.Process(ChooseTask.Mulligan(game.Player2, new List<int>()));

				game.MainReady();

				while (game.State != State.COMPLETE)
				{
					List<PlayerTask> options = game.CurrentPlayer.Options();
					PlayerTask option = options[Rnd.Next(options.Count)];
					//Console.WriteLine(option.FullPrint());
					game.Process(option);


				}
				turns += game.Turn;
				if (game.Player1.PlayState == PlayState.WON)
					wins[0]++;
				if (game.Player2.PlayState == PlayState.WON)
					wins[1]++;

			}
			watch.Stop();

			Console.WriteLine($"{total} games with {turns} turns took {watch.ElapsedMilliseconds} ms => " +
							  $"Avg. {watch.ElapsedMilliseconds / total} per game " +
							  $"and {watch.ElapsedMilliseconds / (total * turns)} per turn!");
			Console.WriteLine($"playerA {wins[0] * 100 / total}% vs. playerB {wins[1] * 100 / total}%!");
		}

		public static void OneTurn()
		{
			var game = new Game(
				new GameConfig()
				{
					StartPlayer = 1,
					Player1Name = "FitzVonGerald",
					Player1HeroClass = CardClass.WARRIOR,
					Player1Deck = Decks.AggroPirateWarrior,
					Player2Name = "RehHausZuckFuchs",
					Player2HeroClass = CardClass.SHAMAN,
					Player2Deck = Decks.MidrangeJadeShaman,
					FillDecks = false,
					Shuffle = false,
					SkipMulligan = false
				});
			game.Player1.BaseMana = 10;
			game.StartGame();

			var aiPlayer1 = new AggroScore();
			var aiPlayer2 = new AggroScore();

			game.Process(ChooseTask.Mulligan(game.Player1, aiPlayer1.MulliganRule().Invoke(game.Player1.Choice.Choices.Select(p => game.IdEntityDic[p]).ToList())));
			game.Process(ChooseTask.Mulligan(game.Player2, aiPlayer2.MulliganRule().Invoke(game.Player2.Choice.Choices.Select(p => game.IdEntityDic[p]).ToList())));

			game.MainReady();

			while (game.CurrentPlayer == game.Player1)
			{
				Console.WriteLine($"* Calculating solutions *** Player 1 ***");

				List<OptionNode> solutions = OptionNode.GetSolutions(game, game.Player1.Id, aiPlayer1, 10, 500);

				var solution = new List<PlayerTask>();
				solutions.OrderByDescending(p => p.Score).First().PlayerTasks(ref solution);
				Console.WriteLine($"- Player 1 - <{game.CurrentPlayer.Name}> ---------------------------");

				foreach (PlayerTask task in solution)
				{
					Console.WriteLine(task.FullPrint());
					game.Process(task);
					if (game.CurrentPlayer.Choice != null)
						break;
				}
			}

			Console.WriteLine(game.Player1.HandZone.FullPrint());
			Console.WriteLine(game.Player1.BoardZone.FullPrint());
		}

		public static void FullGame(List<Card> Player1Cards, List<Card> Player2Cards, string PlayerOneName, string PlayerTwoName)
		//public static void FullGame()
		{
			//20200130 Connor - Changing Console.WriteLine() calls to agree with logsbuild

			string logbuild = "";

			logbuild += PrintDeckOfCards(Player1Cards);
			logbuild += PrintDeckOfCards(Player2Cards);
			//Console.WriteLine(logbuild);

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
					List <OptionNode> solutions = OptionNode.GetSolutions(game, game.Player1.Id, aiPlayer1, 10, 500);
					var solution = new List<PlayerTask>();
					solutions.OrderByDescending(p => p.Score).First().PlayerTasks(ref solution);
					//Console.WriteLine($"- Player 1 - <{game.CurrentPlayer.Name}> ---------------------------");
					logbuild += $"- Player 1 - <{game.CurrentPlayer.Name}> ---------------------------\n";

					string allTasks = "ALL TURN TASKS:\n";
					foreach (PlayerTask task in solution)
					{
						string printedTask = task.FullPrint()+"\n";
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
			Console.WriteLine(logbuild);
		}

		public static void TestFullGames()
		{

			int maxGames = 1000;
			int maxDepth = 10;
			int maxWidth = 14;
			int[] player1Stats = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			int[] player2Stats = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

			var gameConfig = new GameConfig()
			{
				StartPlayer = -1,
				Player1Name = "FitzVonGerald",
				Player1HeroClass = CardClass.PALADIN,
				Player1Deck = new List<Card>()
						{
						Cards.FromName("Blessing of Might"),
						Cards.FromName("Blessing of Might"),
						Cards.FromName("Gnomish Inventor"),
						Cards.FromName("Gnomish Inventor"),
						Cards.FromName("Goldshire Footman"),
						Cards.FromName("Goldshire Footman"),
						Cards.FromName("Hammer of Wrath"),
						Cards.FromName("Hammer of Wrath"),
						Cards.FromName("Hand of Protection"),
						Cards.FromName("Hand of Protection"),
						Cards.FromName("Holy Light"),
						Cards.FromName("Holy Light"),
						Cards.FromName("Ironforge Rifleman"),
						Cards.FromName("Ironforge Rifleman"),
						Cards.FromName("Light's Justice"),
						Cards.FromName("Light's Justice"),
						Cards.FromName("Lord of the Arena"),
						Cards.FromName("Lord of the Arena"),
						Cards.FromName("Nightblade"),
						Cards.FromName("Nightblade"),
						Cards.FromName("Raid Leader"),
						Cards.FromName("Raid Leader"),
						Cards.FromName("Stonetusk Boar"),
						Cards.FromName("Stonetusk Boar"),
						Cards.FromName("Stormpike Commando"),
						Cards.FromName("Stormpike Commando"),
						Cards.FromName("Stormwind Champion"),
						Cards.FromName("Stormwind Champion"),
						Cards.FromName("Stormwind Knight"),
						Cards.FromName("Stormwind Knight")
						},
				Player2Name = "RehHausZuckFuchs",
				Player2HeroClass = CardClass.PALADIN,
				Player2Deck = new List<Card>()
						{
						Cards.FromName("Blessing of Might"),
						Cards.FromName("Blessing of Might"),
						Cards.FromName("Gnomish Inventor"),
						Cards.FromName("Gnomish Inventor"),
						Cards.FromName("Goldshire Footman"),
						Cards.FromName("Goldshire Footman"),
						Cards.FromName("Hammer of Wrath"),
						Cards.FromName("Hammer of Wrath"),
						Cards.FromName("Hand of Protection"),
						Cards.FromName("Hand of Protection"),
						Cards.FromName("Holy Light"),
						Cards.FromName("Holy Light"),
						Cards.FromName("Ironforge Rifleman"),
						Cards.FromName("Ironforge Rifleman"),
						Cards.FromName("Light's Justice"),
						Cards.FromName("Light's Justice"),
						Cards.FromName("Lord of the Arena"),
						Cards.FromName("Lord of the Arena"),
						Cards.FromName("Nightblade"),
						Cards.FromName("Nightblade"),
						Cards.FromName("Raid Leader"),
						Cards.FromName("Raid Leader"),
						Cards.FromName("Stonetusk Boar"),
						Cards.FromName("Stonetusk Boar"),
						Cards.FromName("Stormpike Commando"),
						Cards.FromName("Stormpike Commando"),
						Cards.FromName("Stormwind Champion"),
						Cards.FromName("Stormwind Champion"),
						Cards.FromName("Stormwind Knight"),
						Cards.FromName("Stormwind Knight")
						},
				FillDecks = false,
				Shuffle = true,
				SkipMulligan = false,
				Logging = false,
				History = false
			};

			for (int i = 0; i < maxGames; i++)
			{
				var game = new Game(gameConfig);
				game.StartGame();

				var aiPlayer1 = new AggroScore();
				var aiPlayer2 = new AggroScore();

				List<int> mulligan1 = aiPlayer1.MulliganRule().Invoke(game.Player1.Choice.Choices.Select(p => game.IdEntityDic[p]).ToList());
				List<int> mulligan2 = aiPlayer2.MulliganRule().Invoke(game.Player2.Choice.Choices.Select(p => game.IdEntityDic[p]).ToList());

				game.Process(ChooseTask.Mulligan(game.Player1, mulligan1));
				game.Process(ChooseTask.Mulligan(game.Player2, mulligan2));

				game.MainReady();

				while (game.State != State.COMPLETE)
				{
					while (game.State == State.RUNNING && game.CurrentPlayer == game.Player1)
					{
						List<OptionNode> solutions = OptionNode.GetSolutions(game, game.Player1.Id, aiPlayer1, maxDepth, maxWidth);
						var solution = new List<PlayerTask>();
						solutions.OrderByDescending(p => p.Score).First().PlayerTasks(ref solution);
						foreach (PlayerTask task in solution)
						{
							game.Process(task);
							if (game.CurrentPlayer.Choice != null)
								break;
						}
					}
					while (game.State == State.RUNNING && game.CurrentPlayer == game.Player2)
					{
						List<OptionNode> solutions = OptionNode.GetSolutions(game, game.Player2.Id, aiPlayer2, maxDepth, maxWidth);
						var solution = new List<PlayerTask>();
						solutions.OrderByDescending(p => p.Score).First().PlayerTasks(ref solution);
						foreach (PlayerTask task in solution)
						{
							game.Process(task);
							if (game.CurrentPlayer.Choice != null)
								break;
						}
					}
				}

				player1Stats[(int)game.Player1.PlayState]++;
				player2Stats[(int)game.Player2.PlayState]++;

				Console.WriteLine($"{i}.Game: {game.State}, Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState}");
			}

			Console.WriteLine($"Player1: {String.Join(",", player1Stats)}");
			Console.WriteLine($"Player2: {String.Join(",", player2Stats)}");
		}

	}
}
