"""
TO DO:
1) Parse through the new logs

2) Make a function that takes the data and forms a dataframe per game:
turn number		P1_health	P2_health	health_difference   AMOUNTHEALEDTHISTURN .... etc etc

3) Find a way to run the C# script N number of times and write to output files

4) python script should read all N files



End Of Turn Logs look like:

______________________________________________________________________
Player1: PLAYING / Player2: PLAYING - turn no 1
Hero[P1]: 30 / Hero[P2]: 30
#>>>>>>>>>TASK TYPE CHECK (is EOT?): True
#>>>>>>>>>TASK TYPE: END_TURN
CURRENT PLAYER: P1 FitzVonGerald
AMOUNTHEALEDTHISTURN 0
HEROPOWERACTIVATIONSTHIS TURN 0
NUMATTACKSTHISTURN 0
NUMCARDSDRAWNTHISTURN 1
NUMCARDSPLAYEDTHISTURN 1
NUMCARDSTODRAW 0
NUMELEMENTALSPLAYEDLASTTURN 0
NUMELEMENTALSPLAYEDTHISTURN 0
NUMFRIENDLYMINIONSTHATATTACKEDTHISTURN 0
NUMFRIENDLYMINIONSTHATDIEDTHISTURN 0
NUMMINIONSPLAYEDTHISTURN 1
NUMMINIONSPLAYERKILLEDTHISTURN 0
NUMOPTIONSPLAYEDTHISTURN 2
NUMSECRETSPLAYEDTHISGAME 0
NUMSPELLSPLAYEDTHISGAME 0
NUMTIMESHEROPOWERUSEDTHISGAME 0
REMAININGMANA 0
TOTALMANASPENTTHISGAME 1
USEDMANATHISTURN 1
______________________________________________________________________|

this contains 25 lines from index of ______ to index of _____|
shuold be range(i, i+26)
The two lines above with the >>>>>> don't begin with '#', added that there because PyCharm threw errors on those lines.

"""
import numpy as np
import pandas as pd
import matplotlib
import matplotlib.pyplot as plt

import re
import argparse
import logging


def parse_options():
	parser = argparse.ArgumentParser(description="Class for parsing game data")
	parser.add_argument('data_file', help='path to the data file')
	parser.add_argument('--sum', action='store_true', help='flag to print summary')
	parser.add_argument('--oth', action='store_true', help='flag to print other lines')
	parser.add_argument('--ron', action='store_true', help='flag to print health_difs')
	parser.add_argument('--end', action='store_true', help='flag to print end_turn health')
	parser.add_argument('--blk', action='store_true', help='flag to print blocktypes')
	parser.add_argument('--log_file', help='file to store logs')
	parser.add_argument('-vb', '--verbose', action='store_true', help='turn on verbose logs for debugging')
	ret_args = parser.parse_args()

	log_fmt = '%(asctime)s LINE:%(lineno)d LEVEL:%(levelname)s %(message)s'
	if ret_args.verbose:
		log_lvl = logging.DEBUG
	else:
		log_lvl = logging.INFO

	if ret_args.log_file is not None:
		log_file = ret_args.log_file
		logging.basicConfig(level=log_lvl, filename=log_file, format=log_fmt)
	else:
		logging.basicConfig(level=log_lvl, format=log_fmt)
	return ret_args


class GameFilter:
	"""class to parse through the game.txt file"""

	def __init__(self, infile):
		self.lines = []
		with open(infile) as f:
			for line in f:
				self.lines.append(line.strip('\n'))
		self.df = pd.DataFrame()

	def parse_file(self):
		for i in range(len(self.lines)):
			line = self.lines[i]
			if len(line) > 0:
				if line[0] == '_' and line[-1] == '_':
					self.parse_turn(self.lines[i:i+26])
					i += 26
		return

	def parse_turn(self, turn_text_list):
		for line in turn_text_list:
			logging.info(line)
		return


class GameFilterOriginal:
	"""class to filter key info from fullgame.txt"""

	def __init__(self, infile):
		self.lines = []
		with open(infile) as f:
			for line in f:
				self.lines.append(line.strip("\n"))
		self.blocktypes = {}
		self.healths = []
		self.cur_round = []
		self.end_turns = []
		self.others = []
		self.df = pd.DataFrame(columns=['turn_no', 'p1_end_health', 'p2_end_health', 'health_dif'])

	def line_parser(self):
		for i in range(len(self.lines)):
			blockF, healthF, endturnF = False, False, False
			line = self.lines[i]
			prev = self.lines[i - 1]
			block_query = re.search("Blocktype: [A-Z]+", line)
			if block_query is not None:
				blockF = True
				# self.blocktypes[i] = block_query.group()
				self.blocktypes[i] = line

			health_query = re.search("Hero\[P1\]: [0-9]+ / Hero\[P2\]: [0-9]+", line)
			if health_query is not None:
				healthF = True
				cur = health_query.group().strip('\n')
				health_list = [prev.strip('\n'), cur]
				self.healths.append(health_list)

			endturn_query = re.search(">>>Player [0-9] HEALTH", line)
			if endturn_query is not None:
				endturnF = True
				end = line.strip("\n")
				self.end_turns.append(end)

			if not blockF and not healthF and not endturnF:
				self.others.append(line)

	def print_header(self, s):
		for i in range(45):
			logging.info("_", end="")
		logging.info(s, end="")
		for i in range(45):
			logging.info("_", end="")
		logging.info()

	def print_summary(self):
		self.print_header("SUMMARY")
		logging.info("Number of lines: {}".format(len(self.lines)))
		logging.info("Number of blocktypes: {}".format(len(self.blocktypes)))
		logging.info("Number of health declarations: {}".format(len(self.healths)))
		logging.info("Number of others: {}".format(len(self.others)))

	def print_rounds(self):
		self.print_header("ROUNDS")
		for l in self.healths:
			# logging.info(l[0])
			logging.info(l[1])
		logging.info()

	# Hero[P1]: 30 / Hero[P2]: 30

	def print_others(self):
		self.print_header("OTHERS")
		for line in self.others:
			logging.info(line)
		logging.info()

	def print_blocktypes(self):
		self.print_header("BLOCKTYPES")
		for key in self.blocktypes:
			logging.info(key, self.blocktypes[key])

	def process_endturn_health(self, p_flag):
		"""
		MAJOR PROBLEM HERE - turn number in DF is calculated manually. There
		is an error in the text files with how the turn number is calculated.
		Please check .sln file


		:return:
		"""
		if p_flag: self.print_header("ENDTURN_HEALTH")
		p1_health = 30
		p2_health = 30
		for turn_no in range(len(self.end_turns)):
			turn = self.end_turns[turn_no]
			parts = turn.split()
			if turn_no % 2 == 0:
				p1_health = int(parts[-1])
			else:
				p2_health = int(parts[-1])
			dif = p1_health - p2_health

			row = {'turn_no': turn_no + 1, 'p1_end_health': p1_health, 'p2_end_health': p2_health, 'health_dif': dif}
			self.df = self.df.append(row, ignore_index=True)
			if p_flag: logging.info(turn_no, turn)

	def print_options(self, arg_list):
		if arg_list.sum:
			self.print_summary()
		self.process_endturn_health(arg_list.end)
		if arg_list.ron:
			self.print_rounds()
		if arg_list.oth:
			self.print_others()
		if arg_list.blk:
			self.print_blocktypes()

	def plot_data(self):
		plt.figure()
		x = self.df.turn_no
		y = self.df.health_dif
		y1 = self.df.p1_end_health
		y2 = self.df.p2_end_health
		plt.plot(x, y, label='P1-P2', color='grey')
		plt.plot(x, y1, label='P1', color='red')
		plt.plot(x, y2, label='P2', color='blue')
		plt.title('Health Difference Player1-Player2')
		plt.legend(loc='upper right')
		plt.xlabel('Turn Num')
		plt.ylabel('Health Dif')
		plt.show()


def main():
	args = parse_options()
	# C:\Users\watson\Desktop\fullgame01_060219.txt
	logging.debug(f'Matplotlib: {matplotlib.__version__}')
	logging.debug(f'Numpy: {np.__version__}')
	logging.debug(f'Pandas: {pd.__version__}')
	game_name = args.data_file
	#game_obj = GameFilterOriginal(game_name)
	#game_obj.line_parser()
	#game_obj.print_options(args)
	#logging.info('\n{}'.format(game_obj.df))
	#game_obj.plot_data()

	game_obj = GameFilter(game_name)
	game_obj.parse_file()


if __name__ == "__main__":
	main()
