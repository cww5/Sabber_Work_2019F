import sys
import argparse
from pathlib import Path

desc = '''
This script is meant to take a decklist and add it to a .tml file. The input is as follows:

python deck_to_tml.py path_to_decklist path_to_decks.tml'''


def get_arguments():
	parser = argparse.ArgumentParser(description=desc)
	parser.add_argument('deck', help='Path\\to\\thesis-decklists\\deck.txt ')
	parser.add_argument('tmlname', help='Name for the output tml file (which exists already)')
	args = parser.parse_args()
	return args


def main():
	args = get_arguments()
	deck_file = args.deck
	tml_file = args.tmlname
	tml_builder = '\n[[Decks]]\n'
	num_cards = 0
	try:
		with open(deck_file) as f:
			lines = f.readlines()
			for line in lines:
				line = line.strip('\n')
				if '>>Name' in line:
					tml_builder += ('DeckName = \"{}\"\n'.format(line.split(': ')[-1]))
				elif '>>Hero' in line:
					tml_builder += ('ClassName = \"{}\"\n'.format(line.split(': ')[-1]))
					tml_builder += ('CardList = [')
				elif '><' in line:
					cards = [line.split('><')[0] for i in range(int(line.split('><')[-1]))]
					for card in cards:
						tml_builder += '\"{}\", '.format(card)
						num_cards += 1
			tml_builder = tml_builder.strip(', ')
			tml_builder += ']\n'
	except FileNotFoundError as fe:
		print('Cannot find file: {}'.format(deck_file))
		sys.exit(0)
	except Exception as e:
		print('Other error...')
		sys.exit(0)
	print('There are {} cards in the deck'.format(num_cards))
	try:
		print(tml_builder)
		with open(tml_file, 'a') as tf:
			tf.write(tml_builder)
		print('Done! Please check: {}'.format(tml_file))
	except FileNotFoundError as fe:
		print('Cannot find file: {}'.format(tml_file))
		sys.exit(0)
	except Exception as e:
		print('Other error...')
		sys.exit(0)


if __name__ == '__main__':
	main()
