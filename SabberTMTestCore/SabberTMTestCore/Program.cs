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
using System.Collections;
using SabberStoneCore.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;


using SabberStoneCoreAi.src.DeckEvaluator.Score;


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
        //private static bool record_log;
		private static string players_decks_file;
		private static string opponents_decks_file;
		private static string weights_file;
		private static int numLoops;

		//20200305 Connor - NeuralNetWeights list of weights
		public static List<double[]> WeightsList = new List<double[]>();
		public static int[] layerSizes = { 15, 5, 4, 1 };

		// 20190128 Amy - This function takes command line arguments and parses them.
		// That is, the strings are converted to variables that the program will understand.

		#region Region: ParseArgs Function
		public static void parseArgs(string[] args)
        {
            /*
			 *This function was retrieved from Fernando
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
                    numGames = Int32.Parse(argument.Substring(9));
                }
                else if (argument.Contains("stepsize="))
                {
                    stepSize = Int32.Parse(argument.Substring(9));
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
                /*else if (argument.Contains("log="))
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
                else if (argument.Contains("nerf="))
				{
					string nerf_data_filepath = argument.Substring(5);

					NerfCards(nerf_data_filepath);
				}*/
                else if (argument.Contains("maxwidth="))
                {
                    maxWidth = Int32.Parse(argument.Substring(9));
                }
                else if (argument.Contains("maxdepth="))
                {
                    maxDepth = Int32.Parse(argument.Substring(9));
                }
				else if (argument.Contains("weights="))
				{
					//20200326 - This is the fittest_log.csv file to load ANN weights for the NN score
					weights_file = argument.Substring(8);
				}
            }
		}//End ParseArgs() function from Fernando
		#endregion

		//20200305 Connor - NeuralNetWeights load from fittest_log.csv file
		public static List<double[]> weightsfromfile()
		{
			List<double[]> myWeights = new List<double[]>();
			Console.WriteLine(weights_file);

			if (weights_file == "")
			{
				double[] myDArr1 = new double[109] { -0.0498897955559231, -0.00234206914216949, -0.0167772738910426, -0.0497613659412561, 0.0158904165123556, 0.00395587517538804, -0.0680901684548687, -0.000274599297847171, -0.00147307527018621, -0.0606348702344401, -0.0531911295629245, 0.0163835341916852, 0.0145118832896587, -0.0486657258065906, 0.0246243536861883, 0.0372975973755964, -0.0234505015039713, 0.0273130219907178, 0.0136191072248006, -0.0380902922223028, 0.0189433028983461, 0.0330997781438103, 0.0781158780530853, 0.0380706818293876, 0.0367910672593857, -0.0363243663707961, -0.0762387787748962, -0.0256234289600046, -0.0406613913750856, -0.0300177711471189, 0.0601188459257016, -0.0971969536088867, 0.052876940751843, -0.0684562075973472, -0.0835913010884498, -0.0149464481079092, -0.0344078795295534, -0.022025049876961, -0.0252068303825156, -0.00208087051626745, 0.055499162147784, -0.0802796698767543, -0.046763776990744, 0.0163940481071527, 0.0164167131738451, 0.0478392558733552, -0.0127705551795562, -0.0404529135271101, -0.0629607891595825, -0.00959080590426704, -0.106765326296145, -0.0318693058426412, 0.102299742285363, 0.0159982747940409, -0.00497654995221297, -0.0145787674991394, 0.00939971892283339, 0.0734718775171337, 0.110306759021699, -0.0694232967837653, -0.10161574613107, 0.0386852458745032, -0.0293790407731843, -0.00677428898925851, 0.0596648345530649, 0.0023500868345317, 0.020809126909963, -0.0194244014762708, -0.0208485860504521, 0.0179989462502117, -0.0447360956735265, -0.0514206220639084, -0.00573328563370722, 0.00659890521654868, 0.032650584987578, -0.0184693683443398, -0.0467582438739805, 0.0135011023661066, 0.0274114406349387, 0.0619045028948852, 0.0959874822264266, -0.119649857183138, -0.0742507422065495, -0.032287896792509, -0.0774162461920492, 0.0482646197275909, -0.0440951260934114, 0.0711427936113711, 0.0746773852852332, -0.0184700007737676, -0.000831028950324878, 0.0916853511801156, -0.0435483671049158, 0.0629248873618281, -0.0800483402221562, -0.0349542144765449, -0.0438654664058621, 0.0591981968063137, -0.0376173464530714, 0.0212585464789898, -0.015477158117257, -0.0188898523232864, 0.0192066195783841, -0.0642852944103787, -0.0293917806941686, -0.0105121552786185, 0.0639598949882742, -0.0475538949734785, -0.112460388535468 };
				double[] myDArr2 = new double[109] { -0.0498897955559231, -0.00234206914216949, -0.0167772738910426, -0.0497613659412561, 0.0158904165123556, 0.00395587517538804, -0.0680901684548687, -0.000274599297847171, -0.00147307527018621, -0.0606348702344401, -0.0531911295629245, 0.0163835341916852, 0.0145118832896587, -0.0486657258065906, 0.0246243536861883, 0.0372975973755964, -0.0234505015039713, 0.0273130219907178, 0.0136191072248006, -0.0380902922223028, 0.0189433028983461, 0.0330997781438103, 0.0781158780530853, 0.0380706818293876, 0.0367910672593857, -0.0363243663707961, -0.0762387787748962, -0.0256234289600046, -0.0406613913750856, -0.0300177711471189, 0.0601188459257016, -0.0971969536088867, 0.052876940751843, -0.0684562075973472, -0.0835913010884498, -0.0149464481079092, -0.0344078795295534, -0.022025049876961, -0.0252068303825156, -0.00208087051626745, 0.055499162147784, -0.0802796698767543, -0.046763776990744, 0.0163940481071527, 0.0164167131738451, 0.0478392558733552, -0.0127705551795562, -0.0404529135271101, -0.0629607891595825, -0.00959080590426704, -0.106765326296145, -0.0318693058426412, 0.102299742285363, 0.0159982747940409, -0.00497654995221297, -0.0145787674991394, 0.00939971892283339, 0.0734718775171337, 0.110306759021699, -0.0694232967837653, -0.10161574613107, 0.0386852458745032, -0.0293790407731843, -0.00677428898925851, 0.0596648345530649, 0.0023500868345317, 0.020809126909963, -0.0194244014762708, -0.0208485860504521, 0.0179989462502117, -0.0447360956735265, -0.0514206220639084, -0.00573328563370722, 0.00659890521654868, 0.032650584987578, -0.0184693683443398, -0.0467582438739805, 0.0135011023661066, 0.0274114406349387, 0.0619045028948852, 0.0959874822264266, -0.119649857183138, -0.0742507422065495, -0.032287896792509, -0.0774162461920492, 0.0482646197275909, -0.0440951260934114, 0.0711427936113711, 0.0746773852852332, -0.0184700007737676, -0.000831028950324878, 0.0916853511801156, -0.0435483671049158, 0.0629248873618281, -0.0800483402221562, -0.0349542144765449, -0.0438654664058621, 0.0591981968063137, -0.0376173464530714, 0.0212585464789898, -0.015477158117257, -0.0188898523232864, 0.0192066195783841, -0.0642852944103787, -0.0293917806941686, -0.0105121552786185, 0.0639598949882742, -0.0475538949734785, -0.112460388535468 };
				myWeights.Add(myDArr1);
				myWeights.Add(myDArr2);
			}
			else
			{
				//string weights_file = @"/Users/amy/Downloads/fittest_log.csv";
				//string weights_file = "C:\\Users\\Main\\Documents\\StrategySearch\\logs\\fittest_log.csv";
				//string weights_file = "C:\\Users\\Main\\Documents\\GitHub\\Sabber_Work_2019F\thesis-output\\StrategySearch\\Warlock_Net\\logs\\fittest_log.csv";
				string[] lines = System.IO.File.ReadAllLines(weights_file);

				string[] firstLine = lines[0].Trim(new char[] { ' ', '\n' }).Split(",");
				int firstWeightIndex = 0; // this will be 19
				int lastWeightIndex = -1; //this will be 127
				int numWeights = 0; // this will be 109
				int numNonWeights = -1; // this will be 18 -> there are 19 index positions before weights come in

				for(int i = 0; i < firstLine.Length; i++)
				{
					if (!firstLine[i].Contains("Weight"))
					{
						numNonWeights += 1;
					}
					else if (firstLine[i].Contains("Weight:0"))
					{
						firstWeightIndex = i;
					}
					lastWeightIndex += 1;
				}
				numWeights = lastWeightIndex - numNonWeights; 

				for (int i = 1; i < lines.Length; i++)
				{
					if (i < lines.Length -1)
					{
						continue;
					}
					string[] lineSplit = lines[i].Trim(new char[] { ' ', '\n' }).Split(",");
					double[] lineWeights = new double[109]; // creates a default array of 0s
					for (int j = firstWeightIndex; j < lineSplit.Length; j++)
					{
						try
						{
							lineWeights.SetValue(Convert.ToDouble(lineSplit[j]), j - numNonWeights - 1);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
							Console.WriteLine(j - numNonWeights - 1);
							Console.WriteLine(lineWeights[j - numNonWeights - 1]);
							Console.WriteLine(j);
							Console.WriteLine(lineSplit[j]);
							Environment.Exit(0);
						}
						
					}
					myWeights.Add(lineWeights);
				}


				/*
				int firstWeightIndex = 0;
				int lastWeightIndex = 0;

				foreach (string line in lines)
				{ //iterate over each line of fittest_log.csv
					string[] myRowLine = line.Split(","); //this is the line of the csv split
					var weights = new double[lastWeightIndex - firstWeightIndex + 1];

					for (int i = 0; i < myRowLine.Count(); i++)
					{ //iterate over the index values for the line
						if (myRowLine[i].Contains("Weight:0")) // if (in the first line) there is Weight:0
						{
							//assign firstWeightIndex and lastWeightIndex for every line of the csv
							firstWeightIndex = i;
							lastWeightIndex = myRowLine.Count() - 1;
							Console.WriteLine("Index Weight 0: " + firstWeightIndex);
							Console.WriteLine(myRowLine[i]);

						}
						else if (firstWeightIndex != 0 && i >= firstWeightIndex)
						{ // otherwise, if the firstWeightIndex != 0 and the current index i is >= firstWeightIndex
							if (!myRowLine[i].Contains("Weight")) //AND we are on a line after line 0
							{ // we have a weight, assign it into the weights vector
								weights[i - 19] = Double.Parse(myRowLine[i]);
							}

							Console.WriteLine("Weight:" + i + ", " + myRowLine[i]);
						}
					}
					// add the weights from a line in fittest_log.csv to the myWeights vector
					myWeights.Add(weights);
				}
				*/
			}
			return myWeights;
		}

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
			weights_file = "";

            parseArgs(args);
			//20200305 Connor - NeuralNetWeights sample vector
			//double[] myDArr = new double[109] { -0.0498897955559231, -0.00234206914216949, -0.0167772738910426, -0.0497613659412561, 0.0158904165123556, 0.00395587517538804, -0.0680901684548687, -0.000274599297847171, -0.00147307527018621, -0.0606348702344401, -0.0531911295629245, 0.0163835341916852, 0.0145118832896587, -0.0486657258065906, 0.0246243536861883, 0.0372975973755964, -0.0234505015039713, 0.0273130219907178, 0.0136191072248006, -0.0380902922223028, 0.0189433028983461, 0.0330997781438103, 0.0781158780530853, 0.0380706818293876, 0.0367910672593857, -0.0363243663707961, -0.0762387787748962, -0.0256234289600046, -0.0406613913750856, -0.0300177711471189, 0.0601188459257016, -0.0971969536088867, 0.052876940751843, -0.0684562075973472, -0.0835913010884498, -0.0149464481079092, -0.0344078795295534, -0.022025049876961, -0.0252068303825156, -0.00208087051626745, 0.055499162147784, -0.0802796698767543, -0.046763776990744, 0.0163940481071527, 0.0164167131738451, 0.0478392558733552, -0.0127705551795562, -0.0404529135271101, -0.0629607891595825, -0.00959080590426704, -0.106765326296145, -0.0318693058426412, 0.102299742285363, 0.0159982747940409, -0.00497654995221297, -0.0145787674991394, 0.00939971892283339, 0.0734718775171337, 0.110306759021699, -0.0694232967837653, -0.10161574613107, 0.0386852458745032, -0.0293790407731843, -0.00677428898925851, 0.0596648345530649, 0.0023500868345317, 0.020809126909963, -0.0194244014762708, -0.0208485860504521, 0.0179989462502117, -0.0447360956735265, -0.0514206220639084, -0.00573328563370722, 0.00659890521654868, 0.032650584987578, -0.0184693683443398, -0.0467582438739805, 0.0135011023661066, 0.0274114406349387, 0.0619045028948852, 0.0959874822264266, -0.119649857183138, -0.0742507422065495, -0.032287896792509, -0.0774162461920492, 0.0482646197275909, -0.0440951260934114, 0.0711427936113711, 0.0746773852852332, -0.0184700007737676, -0.000831028950324878, 0.0916853511801156, -0.0435483671049158, 0.0629248873618281, -0.0800483402221562, -0.0349542144765449, -0.0438654664058621, 0.0591981968063137, -0.0376173464530714, 0.0212585464789898, -0.015477158117257, -0.0188898523232864, 0.0192066195783841, -0.0642852944103787, -0.0293917806941686, -0.0105121552786185, 0.0639598949882742, -0.0475538949734785, -0.112460388535468 };
			//double[] myDArr2 = new double[109] { -0.0498897955559231, -0.00234206914216949, -0.0167772738910426, -0.0497613659412561, 0.0158904165123556, 0.00395587517538804, -0.0680901684548687, -0.000274599297847171, -0.00147307527018621, -0.0606348702344401, -0.0531911295629245, 0.0163835341916852, 0.0145118832896587, -0.0486657258065906, 0.0246243536861883, 0.0372975973755964, -0.0234505015039713, 0.0273130219907178, 0.0136191072248006, -0.0380902922223028, 0.0189433028983461, 0.0330997781438103, 0.0781158780530853, 0.0380706818293876, 0.0367910672593857, -0.0363243663707961, -0.0762387787748962, -0.0256234289600046, -0.0406613913750856, -0.0300177711471189, 0.0601188459257016, -0.0971969536088867, 0.052876940751843, -0.0684562075973472, -0.0835913010884498, -0.0149464481079092, -0.0344078795295534, -0.022025049876961, -0.0252068303825156, -0.00208087051626745, 0.055499162147784, -0.0802796698767543, -0.046763776990744, 0.0163940481071527, 0.0164167131738451, 0.0478392558733552, -0.0127705551795562, -0.0404529135271101, -0.0629607891595825, -0.00959080590426704, -0.106765326296145, -0.0318693058426412, 0.102299742285363, 0.0159982747940409, -0.00497654995221297, -0.0145787674991394, 0.00939971892283339, 0.0734718775171337, 0.110306759021699, -0.0694232967837653, -0.10161574613107, 0.0386852458745032, -0.0293790407731843, -0.00677428898925851, 0.0596648345530649, 0.0023500868345317, 0.020809126909963, -0.0194244014762708, -0.0208485860504521, 0.0179989462502117, -0.0447360956735265, -0.0514206220639084, -0.00573328563370722, 0.00659890521654868, 0.032650584987578, -0.0184693683443398, -0.0467582438739805, 0.0135011023661066, 0.0274114406349387, 0.0619045028948852, 0.0959874822264266, -0.119649857183138, -0.0742507422065495, -0.032287896792509, -0.0774162461920492, 0.0482646197275909, -0.0440951260934114, 0.0711427936113711, 0.0746773852852332, -0.0184700007737676, -0.000831028950324878, 0.0916853511801156, -0.0435483671049158, 0.0629248873618281, -0.0800483402221562, -0.0349542144765449, -0.0438654664058621, 0.0591981968063137, -0.0376173464530714, 0.0212585464789898, -0.015477158117257, -0.0188898523232864, 0.0192066195783841, -0.0642852944103787, -0.0293917806941686, -0.0105121552786185, 0.0639598949882742, -0.0475538949734785, -0.112460388535468 };

			//WeightsList.Add(myDArr);
			//WeightsList.Add(myDArr2);
			//WeightsList = [-0.0498897955559231,-0.00234206914216949,-0.0167772738910426,-0.0497613659412561,0.0158904165123556,0.00395587517538804,-0.0680901684548687,-0.000274599297847171,-0.00147307527018621,-0.0606348702344401,-0.0531911295629245,0.0163835341916852,0.0145118832896587,-0.0486657258065906,0.0246243536861883,0.0372975973755964,-0.0234505015039713,0.0273130219907178,0.0136191072248006,-0.0380902922223028,0.0189433028983461,0.0330997781438103,0.0781158780530853,0.0380706818293876,0.0367910672593857,-0.0363243663707961,-0.0762387787748962,-0.0256234289600046,-0.0406613913750856,-0.0300177711471189,0.0601188459257016,-0.0971969536088867,0.052876940751843,-0.0684562075973472,-0.0835913010884498,-0.0149464481079092,-0.0344078795295534,-0.022025049876961,-0.0252068303825156,-0.00208087051626745,0.055499162147784,-0.0802796698767543,-0.046763776990744,0.0163940481071527,0.0164167131738451,0.0478392558733552,-0.0127705551795562,-0.0404529135271101,-0.0629607891595825,-0.00959080590426704,-0.106765326296145,-0.0318693058426412,0.102299742285363,0.0159982747940409,-0.00497654995221297,-0.0145787674991394,0.00939971892283339,0.0734718775171337,0.110306759021699,-0.0694232967837653,-0.10161574613107,0.0386852458745032,-0.0293790407731843,-0.00677428898925851,0.0596648345530649,0.0023500868345317,0.020809126909963,-0.0194244014762708,-0.0208485860504521,0.0179989462502117,-0.0447360956735265,-0.0514206220639084,-0.00573328563370722,0.00659890521654868,0.032650584987578,-0.0184693683443398,-0.0467582438739805,0.0135011023661066,0.0274114406349387,0.0619045028948852,0.0959874822264266,-0.119649857183138,-0.0742507422065495,-0.032287896792509,-0.0774162461920492,0.0482646197275909,-0.0440951260934114,0.0711427936113711,0.0746773852852332,-0.0184700007737676,-0.000831028950324878,0.0916853511801156,-0.0435483671049158,0.0629248873618281,-0.0800483402221562,-0.0349542144765449,-0.0438654664058621,0.0591981968063137,-0.0376173464530714,0.0212585464789898,-0.015477158117257,-0.0188898523232864,0.0192066195783841,-0.0642852944103787,-0.0293917806941686,-0.0105121552786185,0.0639598949882742,-0.0475538949734785,-0.112460388535468];



			WeightsList = weightsfromfile();

			Console.WriteLine("maxDepth = " + maxDepth);
            Console.WriteLine("maxWidth = " + maxWidth);


			if (folderName == "")
			{
				string assemblyFolderName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToString();
				string[] parts = assemblyFolderName.Split("SabberTMTestCore");
				string rootFolderName = parts[0];

                string[] path_parts = { rootFolderName, "thesis-output", DateTime.Now.ToString("yyyy-MM-dd.hh.mm.ss")};
                folderName = Path.Combine(path_parts).ToString();
                //Console.WriteLine(folderName);
                //folderName = rootFolderName + "\\thesis-decklists\\" + DateTime.Now.ToString("yyyy-MM-dd.hh.mm.ss");
			}

            if (stepSize == 0) { stepSize = numGames; }
            if (numGames < stepSize || (numGames % stepSize) > 0)
            {
                Console.WriteLine("\'numGames / stepSize\' must result in an integer bigger than 0");
                return;
            }

            numLoops = numGames / stepSize;

			//20190128 Amy - Load the data we need for the list of players and opponents
			//players = getPlayersFromFile(player_decks_file);
			//opponents = getPlayersFromFile(opponent_decks_file);

			//20200203 Connor - Directory to store stuff

			Console.WriteLine("Output directory: " + folderName);

			if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            Thread.Sleep(10000);

			List<Tuple<List<Card>, string, string, string, string>> playersList;
			List<Tuple<List<Card>, string, string, string, string>> opponentsList;
			if ((players_decks_file != "") && (opponents_decks_file != ""))
			{
				playersList = GetPlayersFromFile(players_decks_file);
				opponentsList = GetPlayersFromFile(opponents_decks_file);
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
				opponentsList.Add(player2TempTup);

			}

            Console.WriteLine("Starting test setup.");

			//20200305 Connor - NeuralNetWeights
			Console.WriteLine("Preparing ANN Weights");
			/*foreach (double[] Dlist in myWeights)
			{
				for (int j = 0; j < Dlist.Count(); j++)
				{
					Console.WriteLine(Dlist[j]);
				}
			}*/


			PlayAllGames(playersList, opponentsList);
			Console.WriteLine("Test end!");
            //Console.ReadLine();
        }//End Main

		public static void PlayAllGames(List<Tuple<List<Card>, string, string, string, string>> allPlayers, List<Tuple<List<Card>, string, string, string, string>> allOpponents)
		{
			string allGamesOutput = "";
			int j = 0;
			string previous_matchups = "";

			for (int x = 0; x < allPlayers.Count; x++)
			{
				for (int y = 0; y < allOpponents.Count; y++)
				{
					//DeckOfCards(List), deckName(string), playerName(string), heroCharacter(string), heroStrategy(string)
					Tuple<List<Card>, string, string, string, string> player = allPlayers[x];
					Tuple<List<Card>, string, string, string, string> opponent = allOpponents[y];

					List<Card> PlayerDeckList = player.Item1;
					List<Card> OpponentDeckList = opponent.Item1;

					string playerDeckName = player.Item2;//Zoolock_Control_Test
					string opponentDeckName = opponent.Item2;

					string playerName = player.Item3; //Pizza
					string opponentName = opponent.Item3;

					string playerCharacter = player.Item4; //Warlock
					string opponentCharacter = opponent.Item4;

					string playerScore = player.Item5; //Control
					string opponentScore = opponent.Item5;

					//Console.WriteLine(playerName + " vs " + opponentName);
					//Console.WriteLine(!String.Equals(playerName, opponentName));
					//Console.WriteLine(!previous_matchups.Contains("|" + playerName + "X" + opponentName + "|"));
					//if (!String.Equals(playerName, opponentName) && !previous_matchups.Contains("|" + playerName + "X" + opponentName + "|"))
					if (!previous_matchups.Contains("|" + playerName + "X" + opponentName + "|"))
					{
						j = 0;

						previous_matchups += "|" + playerName + "X" + opponentName + "|";
						//Console.WriteLine("numLoops is " + numLoops.ToString());
						while (j < numLoops)
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
										if (parallelGames)
										{
											//allGamesOutput = PlayParallelGames(PlayerDeckList, OpponentDeckList, playerName, opponentName, playerDeckName, opponentDeckName, playerScore, opponentScore);
											try
											{
												allGamesOutput = PlayParallelGames(player, opponent);
											}
											catch(Exception e)
											{
												allGamesOutput = "!!!!!!!!!!!!!!!!!!Error in the parallel games\n" + e.ToString() + "\n!!!!!!!!!!!!!!!!!!End Error!";
												Console.WriteLine(allGamesOutput);
												
											}
										}
										else
										{
											//allGamesOutput = FullGame(PlayerDeckList, OpponentDeckList, playerName, opponentName, playerDeckName, opponentDeckName, playerScore, opponentScore);
											try
											{
												allGamesOutput = FullGame(player, opponent);
											}
											catch(Exception e)
											{
												allGamesOutput = "!!!!!!!!!!!!!!!!!!Error in the serial games\n" + e.ToString() + "\n!!!!!!!!!!!!!!!!!!End Error!";
												Console.WriteLine(allGamesOutput);
											}
										}
										Console.WriteLine(allGamesOutput);
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
									Console.WriteLine("!!!!!!!!!!!!!!!!!!Error in the PlayAllGames loop");
									Console.WriteLine(e.Message.ToString());
									Console.WriteLine("!!!!!!!!!!!!!!!!!!End Error!");
								}
								if (tries > 3)
								{
									break;
								}
							}// Ends the loop to try and get the games
							
							string overallGameStat = folderName + "/" + playerName + "/" + opponentName;
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
								Console.WriteLine("!!!!!!!!!!!!!!!!!!Error writing allGamesOutput");
								Console.WriteLine(e.Message);
								Console.WriteLine("!!!!!!!!!!!!!!!!!!End of error !");
							}
							j++;
						}
					}// End matchup
				}// End opponents loop
			}// End players loop
		}// End PlayAllGames

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

            eotbuilder += ("______________________________________________________________________|");
            return eotbuilder;
		}// End PrintEndOfTurnOptions

		public static string PrintDeckOfCards(List<Card> deckOfCards)
        {
            string deckbuild = "";
            deckbuild += "***********Deck of cards***********\n";
            foreach (Card card in deckOfCards)
            {
                deckbuild += (card.ToString() + "\n");
                //Console.WriteLine(card.ToString());
            }
            deckbuild += ("The deck size is: \n");
            deckbuild += (deckOfCards.Count.ToString() + "\n");
            deckbuild += ("***********************************\n");
            return deckbuild;
		}// End PrintDeckOfCards

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
			 *
			 * Output: Tuple ( DeckOfCards(List), deckName(string), playerName(string), heroCharacter(string), heroStrategy(string) ) 
			*/

			List<Card> deck = new List<Card>();

			string[] playerInfo = fileLine.Split2(';');
			string[] cards = playerInfo[4].Split2('*'); //Cards separated by *
			//Console.WriteLine(fileLine);
			//Console.WriteLine(playerInfo);
			//Console.WriteLine(cards);

			for (int j = 0; j < 30; j++)
			{
				string card = cards[j];
				//Console.WriteLine("Card: " + card + " " + j.ToString());
				deck.Add(Cards.FromName(card));
			}

			string deckName = playerInfo[0].Trim(); //name of the deck 
			string playerName = playerInfo[1].Trim(); //name of the player who created the deck
			string heroType = playerInfo[2].Trim(); //hero character chosen
			string heroStrat = playerInfo[3].Trim(); //score Function

			return Tuple.Create(deck, deckName, playerName, heroType, heroStrat);
		}// End CreatePlayerFromLine

		public static string[] Split2(string source, char delim)
		{
			/*
			* 20200203 - Obtained from Fernando
			*/
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
		}// End Split2

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
		}//End GetPlayersFromFile

		//public static string PlayParallelGames(List<Card> Player1Cards, List<Card> Player2Cards, string PlayerOneName, string PlayerTwoName, string Player1DeckName, string Player2DeckName, string Player1Score, string Player2Score)
		public static string PlayParallelGames(Tuple<List<Card>, string, string, string, string>Player1, Tuple<List<Card>, string, string, string, string> Player2)
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

            //string res = "";
            Parallel.For(0, stepSize, parallel_options, j =>//parallelThreadsInner * testsInEachThreadInner, parallel_options, j =>
            {
                /*20190130 Connor - Run a Parallel For loop to run FullGame on separate threads.
				 * OUTPUT - the resulting string to write to the output file
				 */
                int i = j;
                //Console.WriteLine(i);

                string game_log = "";
                bool retry = true;

                while (retry)
                {
                    try
                    {
						//Console.WriteLine("Start Game!");
						//game_log = FullGame(Player1Cards, Player2Cards, PlayerOneName, PlayerTwoName, Player1DeckName, Player2DeckName, Player1Score, Player2Score);
						game_log = FullGame(Player1, Player2);
						game_log_list[j] = game_log;
                        //Console.WriteLine("Game End!");
                    }
                    catch (Exception e)
                    {
						string temp_message = "!!!!!!!!!!!!!!!!!!Error in the PlayParallelGames";
						Console.WriteLine(temp_message);
						temp_message += e.Message.ToString() + "\n";
						Console.WriteLine(e.Message);
						temp_message += "!!!!!!!!!!!!!!!!!!End of error!\n";
						game_log = e.Message.ToString();
                    }

                    if (game_log.ToLower().Contains("present") || game_log.ToLower().Contains("instance") || game_log.ToLower().Contains("zone"))
                    {
                        Console.WriteLine("this was s=" + game_log + "retrying right here");
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
		}// End PlayParallelGames

		public static object GetNNPlayerScore()
		{
			Console.WriteLine("WeightsList" + WeightsList.Count());
			CustomStratWeights customStrat = CustomStratWeights.CreateFromVector(WeightsList[0]);
			Console.WriteLine("CUSTOM-STRAT");
			int count = 0;
			foreach (double d in customStrat.Weights)
			{
				Console.WriteLine("Index = " + count + ", Value = " + d);
				count++;
			}
			//customStrat.Weights = SabberStoneCoreAi.src.DeckEvaluator.Score.CustomStratWeights.CreateFromVector(WeightsList[0]);
			return new NeuralNetScore(layerSizes, customStrat);//new AggroScore();
		}


		public static Tuple<object, object> GetAIPlayerFromScore(string P1Scoring, string P2Scoring)
		{
            Console.WriteLine("DEBUG} Creating AI Score");
            object aiP1 = new AggroScore();
			object aiP2 = new AggroScore();

			switch (P1Scoring.ToLower())
			{
				case "control":
					aiP1 = new ControlScore();
					break;
				case "fatigue":
					aiP1 = new FatigueScore();
					break;
				case "midrange":
					aiP1 = new MidRangeScore();
					break;
				case "ramp":
					aiP1 = new RampScore();
					break;
				case "network":
					aiP1 = GetNNPlayerScore();
					break;
			}
			switch (P2Scoring.ToLower())
			{
				case "control":
					aiP2 = new ControlScore();
					break;
				case "fatigue":
					aiP2 = new FatigueScore();
					break;
				case "midrange":
					aiP2 = new MidRangeScore();
					break;
				case "ramp":
					aiP2 = new RampScore();
					break;
				case "network":
					aiP2 = GetNNPlayerScore();
					break;
			}
            Console.WriteLine($"DEBUG: Returning AI Scores {P1Scoring} and {P2Scoring}");
            return Tuple.Create(aiP1, aiP2);

		}

		public static object GetAIHeroCharacter(string heroType)
		{
            Console.WriteLine($"DEBUG: Getting AI Hero Character {heroType}");
			switch (heroType.ToLower())
			{
				case "druid":
					return CardClass.DRUID;
				case "hunter":
					return CardClass.HUNTER;
				case "mage":
					return CardClass.MAGE;
				case "paladin":
					return CardClass.PALADIN;
				case "priest":
					return CardClass.PRIEST;
				case "rogue":
					return CardClass.ROGUE;
				case "shaman":
					return CardClass.SHAMAN;
				case "warrior":
					return CardClass.WARRIOR;
				default:
					return CardClass.WARLOCK;
			}
		}


		//public static string FullGame(List<Card> P1Cards, List<Card> P2Cards, string P1Name, string P2Name, string P1DeckName, string P2DeckName, string P1Score, string P2Score)
		public static string FullGame(Tuple<List<Card>, string, string, string, string> P1, Tuple<List<Card>, string, string, string, string> P2)
		{
			Console.WriteLine("DEBUG} Setting items");
			List<Card> P1Cards = P1.Item1;
			List<Card> P2Cards = P2.Item1;

            Console.WriteLine("DEBUG} Getting P1 Deck of Cards");
            string s = PrintDeckOfCards(P1Cards);
            Console.WriteLine("DEBUG} Getting P2 Deck of Cards");
            s = PrintDeckOfCards(P2Cards);
            

            string P1DeckName = P1.Item2;
			string P2DeckName = P2.Item2;

			string P1Name = P1.Item3;
			string P2Name = P2.Item3;

			string P1Character = P1.Item4;
			string P2Character = P2.Item4;

			string P1Score = P1.Item5;
			string P2Score = P2.Item5;

			Console.WriteLine("DEBUG} Creating game");
			var game = new Game(
                new GameConfig()
                {
                    StartPlayer = 1,
                    Player1Name = P1Name,
                    Player1HeroClass = (CardClass)GetAIHeroCharacter(P1Character),
                    Player1Deck = P1Cards,
                    Player2Name = P2Name,
                    Player2HeroClass = (CardClass)GetAIHeroCharacter(P2Character),
                    Player2Deck = P2Cards,
                    FillDecks = false,
                    Shuffle = true,
                    SkipMulligan = false
                });

            //20200130 Connor - Changing Console.WriteLine() calls to agree with logsbuild
            string logbuild = "";
			string tempstring = "";
            tempstring = "+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+";
            Console.WriteLine("DEBUG}" + tempstring);
            logbuild += (tempstring + "\n");

            tempstring = $"Player1: {game.Player1.Name}={P1Score} vs Player2: {game.Player2.Name}={P2Score}";
            //Console.WriteLine("DEBUG}" + tempstring);
            logbuild += (tempstring + "\n");

            tempstring = $"Player1Deck: {P1DeckName} vs Player2Deck: {P2DeckName}";
            //Console.WriteLine("DEBUG}" + tempstring);
            logbuild += (tempstring + "\n");

            /*Console.WriteLine("Getting P1 Deck of Cards");
			tempstring = PrintDeckOfCards(P1Cards);
			Console.WriteLine(tempstring);
			logbuild += tempstring;

            Console.WriteLine("Getting P2 Deck of Cards");
			tempstring = PrintDeckOfCards(P2Cards);
			Console.WriteLine(tempstring);
			logbuild += tempstring;*/

            tempstring = "Starting game";
            Console.WriteLine("DEBUG}" + tempstring);
            logbuild += (tempstring + "\n");
            game.StartGame();

			Tuple<object, object> aiPlayers = GetAIPlayerFromScore(P1Score, P2Score);

			object aiPlayer1 = aiPlayers.Item1;
			object aiPlayer2 = aiPlayers.Item2;

			List<int> mulligan1 = ((Score.Score)aiPlayer1).MulliganRule().Invoke(game.Player1.Choice.Choices.Select(p => game.IdEntityDic[p]).ToList());
            List<int> mulligan2 = ((Score.Score)aiPlayer2).MulliganRule().Invoke(game.Player2.Choice.Choices.Select(p => game.IdEntityDic[p]).ToList());

            tempstring = ($"Player1: Mulligan {String.Join(",", mulligan1)}");
			Console.WriteLine("DEBUG}" + tempstring);
			logbuild += (tempstring + "\n");
            //.Trim( new Char[] { ' ', '\n' } )

            tempstring = ($"Player2: Mulligan {String.Join(",", mulligan2)}");
			Console.WriteLine("DEBUG}" + tempstring);
            logbuild += (tempstring + "\n");

            game.Process(ChooseTask.Mulligan(game.Player1, mulligan1));
            game.Process(ChooseTask.Mulligan(game.Player2, mulligan2));

            game.MainReady();

            while (game.State != State.COMPLETE)
            {

                
                tempstring = ($"Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState} - " + $"ROUND {(game.Turn + 1) / 2} - {game.CurrentPlayer.Name}");
				Console.WriteLine("DEBUG}" + tempstring);
                logbuild += (tempstring + "\n");

                tempstring = ($"Hero[P1]: {game.Player1.Hero.Health} / Hero[P2]: {game.Player2.Hero.Health}");
                //Console.WriteLine("DEBUG}" + tempstring);
                logbuild += (tempstring + "\n");


                while (game.State == State.RUNNING && game.CurrentPlayer == game.Player1)
                {
                    tempstring = $"* Calculating solutions *** Player 1 ***";
					Console.WriteLine("DEBUG}" + tempstring);
                    logbuild += (tempstring + "\n");

                    List<OptionNode> solutions = OptionNode.GetSolutions(game, game.Player1.Id, ((Score.Score)aiPlayer1), 10, 500);
                    var solution = new List<PlayerTask>();
                    solutions.OrderByDescending(p => p.Score).First().PlayerTasks(ref solution);
                    tempstring = $"- Player 1 - <{game.CurrentPlayer.Name}> ---------------------------";
					Console.WriteLine("DEBUG}" + tempstring);
                    logbuild += (tempstring + "\n");

                    string allTasks = "ALL TURN TASKS:\n";
                    foreach (PlayerTask task in solution)
                    {
                        string printedTask = task.FullPrint() + "\n";
                        allTasks += printedTask;
                        tempstring = printedTask;
						Console.WriteLine("DEBUG}" + tempstring.Trim(new char[] { ' ', '\n' }));
                        logbuild += (tempstring);

                        if (task.PlayerTaskType.Equals(PlayerTaskType.END_TURN))
                        {
                            allTasks += "COMPLETE ALL TURN TASKS";
                            tempstring = PrintEndOfTurnOptions(game, task, allTasks);
							Console.WriteLine("DEBUG}" + "COMPLETE ALL TURN TASKS");
                            logbuild += (tempstring + "\n");
                        }
                        game.Process(task);
                        if (game.CurrentPlayer.Choice != null)
                        {
                            tempstring = ($"* Recaclulating due to a final solution ...");
							Console.WriteLine("DEBUG}" + tempstring);
                            logbuild += (tempstring + "\n");
                            break;
                        }
                        if (game.Player1.Hero.Health <= 0 || game.Player2.Hero.Health <= 0)
                        {
                            allTasks += "COMPLETE ALL TURN TASKS";
                            tempstring = PrintEndOfTurnOptions(game, task, allTasks);
							Console.WriteLine("DEBUG}" + "COMPLETE ALL TURN TASKS");
                            logbuild += (tempstring + "\n");
                        }
                    }
                    //Console.WriteLine("Testing>>>>>>>>>>>>>>>>>>>>>");
                    //Console.WriteLine(game.CurrentPlayer.NumCardsPlayedThisTurn);
                }

                // Random mode for Player 2
                tempstring = ($"- Player 2 - <{game.CurrentPlayer.Name}> ---------------------------");
				Console.WriteLine("DEBUG}" + tempstring);
                logbuild += (tempstring + "\n");
                while (game.State == State.RUNNING && game.CurrentPlayer == game.Player2)
                {
                    //var options = game.Options(game.CurrentPlayer);
                    //var option = options[Rnd.Next(options.Count)];
                    //Log.Info($"[{option.FullPrint()}]");
                    //game.Process(option);
                    tempstring = ($"* Calculating solutions *** Player 2 ***");
					Console.WriteLine("DEBUG}" + tempstring);
                    logbuild += (tempstring + "\n");

                    List<OptionNode> solutions = OptionNode.GetSolutions(game, game.Player2.Id, ((Score.Score)aiPlayer2), 10, 500);
                    var solution = new List<PlayerTask>();
                    solutions.OrderByDescending(p => p.Score).First().PlayerTasks(ref solution);
                    tempstring = ($"- Player 2 - <{game.CurrentPlayer.Name}> ---------------------------");
					Console.WriteLine("DEBUG}" + tempstring);
                    logbuild += (tempstring + "\n");

                    string allTasks = "ALL TURN TASKS:\n";
                    foreach (PlayerTask task in solution)
                    {
                        string printedTask = task.FullPrint() + "\n";
                        allTasks += printedTask;
                        tempstring = (printedTask);
						Console.WriteLine("DEBUG}" + tempstring.Trim(new char[] { ' ', '\n' }));
                        logbuild += (tempstring);

                        if (task.PlayerTaskType.Equals(PlayerTaskType.END_TURN))
                        {
                            allTasks += "COMPLETE ALL TURN TASKS";
                            tempstring = PrintEndOfTurnOptions(game, task, allTasks);
							Console.WriteLine("DEBUG}" + "COMPLETE ALL TURN TASKS");
                            logbuild += (tempstring + "\n");
                        }
                        game.Process(task);
                        if (game.CurrentPlayer.Choice != null)
                        {
                            tempstring = ($"* Recaclulating due to a final solution ...");
							Console.WriteLine("DEBUG}" + tempstring);
                            logbuild += (tempstring + "\n");
                            break;
                        }
                        if (game.Player1.Hero.Health <= 0 || game.Player2.Hero.Health <= 0)
                        {
                            allTasks = allTasks + "COMPLETE ALL TURN TASKS";
                            tempstring = PrintEndOfTurnOptions(game, task, allTasks);
							Console.WriteLine("DEBUG}" + "COMPLETE ALL TURN TASKS");
                            logbuild += (tempstring + "\n");
                        }
                    }
                }
            }
            tempstring= ($"Game: {game.State}, Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState}");
			Console.WriteLine("DEBUG}" + tempstring);
            logbuild += (tempstring + "\n");
            tempstring = "+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+";
			Console.WriteLine("DEBUG}" + tempstring);
            logbuild += (tempstring + "\n");
            return logbuild;
		}// End FullGame
	}// End internal class Program
}//End namespace SabberStoneCoreAi
