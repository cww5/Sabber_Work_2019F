import pandas as pd
import matplotlib.pyplot as plt

import re
import logging
import argparse
import os
import sys
from pathlib import Path

main_dir = ''

class GameFilter:
	"""class to parse through the Output#-#.txt file which contains N games"""

	def __init__(self, infile, gc):
		self.lines = []
		with open(infile) as f:
			for line in f:
				self.lines.append(line.strip('\n'))
		self.game_counter = gc
		self.num_games = 0
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
						self.num_games += 1
						self.end_of_turn_data = {}
					else:
						# logging.info('End the game')
						new_game = False
						# Here is where I need the stuff to create the DF for the game
						df = pd.DataFrame.from_dict(self.end_of_turn_data, orient='index')
						new_col_data = [(self.game_counter + self.num_games) for i in range(df.shape[0])]
						df['GAME_COUNTER'] = new_col_data
						# logging.info(df.shape)
						self.games_data[(self.game_counter + self.num_games)] = df
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


class CompileGames:

	"""This class is for taking a csv file and making all the games into one DataFrame"""

	def __init__(self, compiled_path):
		self.main_path = compiled_path
		logging.debug(compiled_path)
		if not os.path.isdir(compiled_path):
			logging.debug(compiled_path)
			logging.warning('NOT a directory')

	def iterate_files(self):
		csv_names = [os.path.join(self.main_path, f) for f in os.listdir(self.main_path)]
		for f1 in csv_names:
			logging.debug(f1)
		logging.info('{} matchup csv files found'.format(len(csv_names)))

	# def create_data_frame(self):


class MatchupData:

	"""This class is for string parsing the matchups"""

	warlock = {
		'aggro':['Roffle','Viper','Pizza','Solegit','Wabeka'],
		'control':['Orasha','Thijs','Stonekeep','Slage','Krebs1996']
	}

	def __init__(self, p1_strat, p2_strat):
		self.p1s = p1_strat
		self.p2s = p2_strat
		self.matchups = []

	def set_matchups(self):
		matchups = []
		for h1 in self.warlock[self.p1s]:
			for h2 in self.warlock[self.p2s]:
				matchups.append('{}-{}.csv'.format(h1, h2))
		self.matchups = matchups

	def get_matchups(self):
		for m in self.matchups:
			logging.info(m)
		logging.info('Total {} matchups'.format(len(self.matchups)))

def parse_options():
	global main_dir
	parser = argparse.ArgumentParser(description='Class for parsing game data')

	# parser.add_argument('base', help='folder of output to analyze ex: 2020-02-10.12.48.53')
	# parser.add_argument('--sum', action='store_true', help='flag to print summary')
	parser.add_argument('--machine', default='mixr', help='configure to desktop, laptop, or mixr')
	parser.add_argument('-vb', '--verbose', action='store_true', default=False, help='turn on verbose logs for debugging')
	ret_args = parser.parse_args()

	log_fmt = '%(asctime)s LINE:%(lineno)d LEVEL:%(levelname)s %(message)s'
	if ret_args.verbose:
		logging.basicConfig(level=logging.DEBUG, format=log_fmt)
	else:
		logging.basicConfig(level=logging.INFO, format=log_fmt)

	if ret_args.machine == 'mixr':
		main_dir = 'C:\\Users\\Main\\Documents\\GitHub\\Sabber_Work_2019F\\thesis-output\\ZLvsCL_1k0_Compiled\\'
	return ret_args


if __name__ == '__main__':
	parse_options()
	# cg = CompileGames(main_dir)
	# cg.iterate_files()
	md1 = MatchupData('aggro', 'control')
	md1.set_matchups()
	md1.get_matchups()



