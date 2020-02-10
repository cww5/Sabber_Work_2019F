import argparse
import sys
from pathlib import Path

desc = """This is a simple script to compile the individual decklists into one csv file.:

C:|...\thesis-extensions\python find_cards.py  ..\thesis-output\CardDefs.xml ..\thesis-output\CardsEng.txt """


def get_arguments():
	parser = argparse.ArgumentParser(description=desc)
	parser.add_argument('decks', help='Path\\to\\thesis-decklists ')
	parser.add_argument('fname', help='Name for the output file')
	args = parser.parse_args()
	return args


def main():
	args = get_arguments()
	root_dir = args.decks
	out_name = root_dir + "\\{}".format(args.fname)
	path_list = Path(root_dir).glob('**\\*.txt')
	csv_builder = ''
	for file_name in path_list:
		with open(file_name) as f:
			lines = f.readlines()
			for line in lines:
				if '>>CSV' in line:
					csv_builder += (line.split(':')[-1]+'\n')
	f = open(out_name, 'w')
	f.write(csv_builder)
	f.close()
	print('Done! Please check {}'.format(out_name))




if __name__ == '__main__':
	main()
