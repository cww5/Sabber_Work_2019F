"""
TO DO:
1) Find a way to run the C# script N number of times and write to output files

2) python script should read all N files



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

python game_filter.py ..\thesis-output\output_last_turn_health_diff.txt

"""
import numpy as np
import pandas as pd
import matplotlib
import matplotlib.pyplot as plt

import re
import argparse
import logging

#import os
#import sys
#import glob
from pathlib import Path


def parse_options():
	parser = argparse.ArgumentParser(description="Class for parsing game data")
	parser.add_argument('root', help='path to the master folder')
	parser.add_argument('--sum', action='store_true', help='flag to print summary')
	parser.add_argument('--oth', action='store_true', help='flag to print other lines')
	parser.add_argument('--ron', action='store_true', help='flag to print health_difs')
	parser.add_argument('--end', action='store_true', help='flag to print end_turn health')
	parser.add_argument('--blk', action='store_true', help='flag to print blocktypes')
	parser.add_argument('--log_file', help='file to store logs')
	parser.add_argument('-vb', '--verbose', action='store_true', default=False, help='turn on verbose logs for debugging')
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
		self.end_of_turn_data = {}
		self.df = pd.DataFrame()

	def parse_file(self):
		for i in range(len(self.lines)):
			line = self.lines[i]
			if len(line) > 0:
				if line[0] == '_' and line[-1] == '_':
					self.parse_turn(self.lines[i:i+26])
					i += 26
		self.df = pd.DataFrame.from_dict(self.end_of_turn_data, orient='index')
		self.df.to_csv('..\\thesis-output\\test.csv')

	def parse_turn(self, turn_text_list):
		turn_dict = {}
		turn_num = -1
		for line in turn_text_list:
			logging.info(line)
			if 'turn no' in line:
				logging.debug('>>>>>>>>>>>>>>>>>>>>>>TURN NUMBER QUERY')
				parts = line.split()
				logging.info(parts)
				turn_num = int(parts[-1])
				turn_dict[turn_num] = {'TURN_NO': turn_num}
				continue

			health_query = re.match('Hero\[P[12]\]: \-?[0-9]+ / Hero\[P[12]\]: \-?[0-9]+', line)
			if health_query is not None:
				logging.debug('>>>>>>>>>>>>>>>>>>>>>>HEALTH DIF QUERY')
				health_parts = line.split(' / ')
				hero_1_parts = health_parts[0].split()
				hero_2_parts = health_parts[1].split()
				turn_dict[turn_num]['P1_HEALTH'] = int(hero_1_parts[-1])
				turn_dict[turn_num]['P2_HEALTH'] = int(hero_2_parts[-1])
				continue

			player_query = re.match('CURRENT PLAYER: P[0-9] [a-zA-Z]+', line)
			if player_query is not None:
				logging.debug('>>>>>>>>>>>>>>>>>>>>>>CURRENT PLAYER QUERY')
				player_parts = line.split(': ')
				turn_dict[turn_num]['CURRENT_PLAYER'] = player_parts[-1]
				continue

			param_query = re.match('[A-Z]+ [0-9]+', line)
			if param_query is not None:
				logging.debug('>>>>>>>>>>>>>>>>>>>>>>PARAM QUERY')
				parts = line.split()
				turn_dict[turn_num][parts[0]] = int(parts[-1])
				continue

		logging.debug(turn_dict)
		self.end_of_turn_data.update(turn_dict)

	def plot_data(self):
		plt.figure()
		x = self.df.TURN_NO
		y1 = self.df['P1_HEALTH']
		y2 = self.df['P2_HEALTH']
		y = y1 - y2
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

	root_dir = args.root
	logging.info(root_dir)
	directory = "C:\\Users\\watson\\Documents\\GitHub\\SabberStone-master\\Sabber_Work_2019F\\thesis-output\\Z1vsZ2\\"

	pathlist = Path(directory).glob('**\\*.txt')
	for path in pathlist:
		# because path is object not string
		game_name = str(path)
		logging.info(game_name)

		game_obj = GameFilter(game_name)
		game_obj.parse_file()
		logging.info('Number of turns: {}'.format(len(game_obj.end_of_turn_data)))
		game_obj.plot_data()


if __name__ == "__main__":
	main()
