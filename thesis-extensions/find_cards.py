import argparse


desc = """This is a simple script to conduct some simple data mining. There are two possible configurations:

Option 1: Create the set of English cards:
C:|...\thesis-extensions\python find_cards.py  ..\thesis-output\CardDefs.xml ..\thesis-output\CardsEng.txt

Option 2: Search the set of English cards using a decklist:
C:\...\thesis-extensions\python find_cards.py ..\thesis-decklists\<decklist>.txt ..\thesis-output\CardsEng.txt

Both runs REQUIRE that the second argument is the path to the parsed list of English cards (which also
contains any lines of xml with english text, since the search parameter in Option 1 is <enUS>"""


def get_arguments():
	parser = argparse.ArgumentParser(description=desc)
	parser.add_argument('infile', help='Either [CardDefs.xml, <decklist>.txt]')
	parser.add_argument('cardseng', help='Must be path to: CardsEng.txt')
	args = parser.parse_args()
	return args


def parse_cards_xml(fname, cards_eng_path):
	enc = 'utf-8'
	try:
		f = open(fname, encoding=enc)
		f2 = open(cards_eng_path, 'w')
	except FileNotFoundError as e:
		print(e)
		return

	cards = []
	lines = f.readlines()
	for line in lines:
		try:
			if '<enUS>' in line:
				cards.append(line)
		except Exception as e:
			continue

	f.close()
	print('num lines: {}'.format(len(lines)))
	print('num cards: {}'.format(len(cards)))

	for card in cards:
		f2.write(card + '\n')
	f2.close()


def find_card(card, lines):
	found = False
	for line in lines:
		if card in line:
			found = True
			#print(card + ' found')
			#print(line)
			#print()
			break
	if not found:
		print(card + ' NOT FOUND')
	return found


def parse_decklist(fname, cards_eng_path):
	try:
		f = open(cards_eng_path)
		deck = open(fname, 'r')
	except FileNotFoundError as e:
		print(e)
		return

	lines_ = f.readlines()
	deck_file_lines = deck.readlines()
	f.close()
	deck.close()
	all_cards_found = True
	deck_name, player_name,  hero_type, hero_score, deck_list = '', '', '', '', ''
	deck_size = 0
	for line_ in deck_file_lines:
		if '><' in line_:
			card_parts = line_.strip('\n').split('><')
			card_ = card_parts[0]
			deck_size += int(card_parts[1])
			# amount = card_parts[1]
			card_found = find_card(card_, lines_)
			if not card_found:
				all_cards_found = False
			else:
				for i in range(int(card_parts[1])):
					deck_list += '{}*'.format(card_)
		elif 'Name' in line_:
			deck_name = line_.strip('\n').split(': ')[-1]
		elif 'Author' in line_:
			player_name = line_.strip('\n').split(': ')[-1]
		elif 'Hero' in line_:
			hero_type = line_.strip('\n').split(': ')[-1]
		elif 'Score' in line_:
			hero_score = line_.strip('\n').split(': ')[-1]
	if all_cards_found and deck_size == 30:
		print('All cards were found!')
		with open(fname, 'a') as deck:
			deck.write('\n>>CSV:{}; {}; {}; {};{};'.format(deck_name, player_name, hero_type, hero_score, deck_list.strip('*')))
	elif not deck_size==30:
		print('Double check the deck size!')
	deck.close()


def main():
	cmd_args = get_arguments()

	infile = cmd_args.infile
	option = infile[-3:]
	eng_cards_path = cmd_args.cardseng

	if option == 'xml':
		parse_cards_xml(infile, eng_cards_path)
	elif option == 'txt':
		parse_decklist(infile, eng_cards_path)
	else:
		print('Invalid option. argv[2] xml to parse CardDefs.xml, or eng to parse the created file with eng cards')


if __name__ == '__main__':
	main()
