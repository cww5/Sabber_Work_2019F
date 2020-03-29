import numpy as np
import pandas as pd
import matplotlib

import argparse
import logging

import os
import sys
from pathlib import Path
from gamework import GameFilter

"""
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
	new_sub_dir = sub_dir.split('\\')[-1] + '_Compiled'
	if sub_dir[:2] == 'C:':
		full_directory = sub_dir
	else:
		full_directory = '{}{}\\'.format(directory, sub_dir)
	full_output_directory = '{}{}\\'.format(directory, new_sub_dir)
	#print(full_directory)
	#print(full_output_directory)
	#sys.exit(0)
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
			game_counter = 0
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

				game_obj = GameFilter(game_name, game_counter)
				game_obj.parse_file()
				# logging.info(len(game_obj.games_data))
				matchup_data.update(game_obj.games_data)
				game_counter += game_obj.num_games
			# logging.info('Number of turns: {}'.format(len(game_obj.end_of_turn_data)))
			# game_obj.plot_data(game_plot_name)
			try:
				ending_df = pd.concat(list(matchup_data.values()))
				ending_df.reset_index(inplace=True, drop=True)
				with open(csv_output_path, 'w') as f:
					ending_df.to_csv(csv_output_path)
				logging.info('Check {}'.format(csv_output_path))
			except Exception as e:
				logging.error('THERE WAS A PROBLEM CONCATTING DFs')
				logging.error('NUMBER OF PAIRS {}'.format(len(matchup_data)))
				sys.exit(0)


if __name__ == "__main__":
	main()
