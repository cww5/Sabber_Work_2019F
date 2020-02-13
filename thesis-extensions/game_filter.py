import numpy as np
import pandas as pd
import matplotlib
import matplotlib.pyplot as plt

import re
import argparse
import logging

# import glob
import os
import sys
from pathlib import Path

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

python game_filter.py laptop folder_name_with_all_output

"""


def parse_options():
	parser = argparse.ArgumentParser(description='Class for parsing game data')
	parser.add_argument('machine', help='configure to desktop, laptop, or mixr')
	parser.add_argument('base', help='folder of output to analyze ex: 2020-02-10.12.48.53')
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
	"""class to parse through the Output#-#.txt file which contains N games"""

	def __init__(self, infile):
		self.lines = []
		with open(infile) as f:
			for line in f:
				self.lines.append(line.strip('\n'))
		self.game_counter = 0
		self.games_data = {}
		self.end_of_turn_data = {}
		self.file_name = infile

	def parse_file(self):
		turn_info = []
		end_of_turn, new_game = False, False
		for i in range(len(self.lines)):
			line = self.lines[i]
			if len(line) > 0:
				# logging.info(line[:2], line[-2:])
				if line[:2] == '+-' and line[-2:] == '-+':
					if not new_game:
						# logging.info('Start new game')
						new_game = True
						self.game_counter += 1
						self.end_of_turn_data = {}
					else:
						# logging.info('End the game')
						new_game = False
						# Here is where I need the stuff to create the DF for the game
						df = pd.DataFrame.from_dict(self.end_of_turn_data, orient='index')
						new_col_data = [self.game_counter for i in range(df.shape[0])]
						df['GAME_COUNTER'] = new_col_data
						# logging.info(df.shape)
						self.games_data[self.game_counter] = df
					# logging.info(self.games_data.keys())
				elif line[0] == '_' and line[-1] == '_':
					end_of_turn = True
					continue
				elif line[0] == '_' and line[-1] == '|':
					self.parse_turn(turn_info)
					end_of_turn = False
					turn_info = []
				elif '!!!!!!' in line:
					logging.warning('There is an error in the file {}'.format(self.file_name))
				if end_of_turn:  # only add the line to turn_info in between _____ and _____|
					turn_info.append(line)

	def parse_turn(self, turn_text_list):
		turn_dict = {}
		turn_num = -1
		for line in turn_text_list:
			# logging.info(line)
			if 'turn no' in line:
				logging.debug('>>>>>>>>>>>>>>>>>>>>>>TURN NUMBER QUERY')
				parts = line.split()
				# logging.info(parts)
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

			param_query = re.match('[A-Z]+ -?[0-9]+', line)
			if param_query is not None:
				logging.debug('>>>>>>>>>>>>>>>>>>>>>>PARAM QUERY')
				parts = line.split()
				turn_dict[turn_num][parts[0]] = int(parts[-1])
				continue

		logging.debug(turn_dict)
		self.end_of_turn_data.update(turn_dict)

	def plot_data(self, fig_name):
		fig, axs = plt.subplots(2)
		fig.suptitle('Player 1 vs Player 2')
		x = self.df.TURN_NO
		y1 = self.df['P1_HEALTH']
		y2 = self.df['P2_HEALTH']
		y = abs(y1 - y2)
		axs[0].plot(x, y1, label='P1', color='red')
		axs[0].plot(x, y2, label='P2', color='blue')
		axs[1].plot(x, y, label='|HP_DIF|', color='grey')
		axs[0].set(xlabel='Turn Num', ylabel='Health Points')
		axs[1].set(xlabel='Turn Num', ylabel='|HP Difference|')
		plt.tight_layout(rect=[0, 0.03, 1, 0.95])
		plt.savefig(fig_name)


def main():
	args = parse_options()
	# C:\Users\watson\Desktop\fullgame01_060219.txt

	logging.debug(f'Matplotlib: {matplotlib.__version__}')
	logging.debug(f'Numpy: {np.__version__}')
	logging.debug(f'Pandas: {pd.__version__}')

	machine = args.machine.lower()
	sub_dir = args.base
	if machine == 'laptop':
		# laptop directory
		directory = 'C:\\Users\\watson\\Documents\\GitHub\\SabberStone-master\\Sabber_Work_2019F\\thesis-output\\'
	elif machine == 'desktop':
		# desktop directory
		# directory = 'C:\\Users\\watson\\Documents\\SabberStone 2019\\Sabber_Work_2019F\\thesis-output\\'
		directory = 'C:\\Users\\watson\\Documents\\SabberStone 2019\\Sabber_Work_2019F\\thesis-output\\'
	elif machine == 'mixr':
		directory = 'C:\\Users\\Main\\Documents\\GitHub\\Sabber_Work_2019F\\thesis-output\\'
	else:
		logging.warning('UNEXPECTED OPTION IN CMD, CONFIG PROPERLY')
		sys.exit(0)
	new_sub_dir = sub_dir + '_Compiled'
	full_directory = '{}{}\\'.format(directory, sub_dir)
	full_output_directory = '{}{}\\'.format(directory, new_sub_dir)
	try:
		os.mkdir(full_output_directory)
	except OSError:
		print("Creation of the directory %s failed" % full_output_directory)
	else:
		print("Successfully created the directory %s " % full_output_directory)
	list_subfolders_with_paths = [f.path for f in os.scandir(full_directory) if f.is_dir()]
	for f1 in list_subfolders_with_paths:
		logging.debug(f1)
		list_matchup_folders = [f.path for f in os.scandir(f1) if f.is_dir()]
		for f2 in list_matchup_folders:
			logging.info('Current folder: {}'.format(f2))
			matchup_data = {}
			logging.debug(f2)
			csv_file_name = '-'.join(f2.split('\\')[-2:]) + '.csv'
			csv_output_path = full_output_directory + csv_file_name
			logging.debug(csv_output_path)
			pathlist = Path(f2).glob('**\\*.txt')
			for game_name in pathlist:
				logging.debug(game_name)
				# game_csv_name = game_name.replace('.txt', '.csv')
				# game_plot_name = game_name.replace('.txt', '.png')
				# logging.info(game_name)
				# logging.info(game_csv_name)
				# logging.info(game_plot_name)

				game_obj = GameFilter(game_name)
				game_obj.parse_file()
				# logging.info(len(game_obj.games_data))
				matchup_data.update(game_obj.games_data)
			# logging.info('Number of turns: {}'.format(len(game_obj.end_of_turn_data)))
			# game_obj.plot_data(game_plot_name)
			try:
				ending_df = pd.concat(list(matchup_data.values()))
				with open(csv_output_path, 'w') as f:
					ending_df.to_csv(csv_output_path)
				logging.info('Check {}'.format(csv_output_path))
			except Exception as e:
				logging.error('THERE WAS A PROBLEM CONCATTING DFs')
				logging.error('NUMBER OF PAIRS {}'.format(len(matchup_data)))
				sys.exit(0)


if __name__ == "__main__":
	main()
