"""This is a simple script to conduct some simple data mining. There are two possible configurations:

Option 1: Create the set of English cards:
C:|...\thesis-extensions\python test.py  ..\thesis-output\CardDefs.xml ..\thesis-output\CardsEng.txt

Option 2: Search the set of English cards using a decklist:
C:\...\thesis-extensions\python test.py ..\thesis-output\<decklist>/txt ..\thesis-output\CardsEng.txt

Both runs REQUIRE that the second argument is the path to the parsed list of English cards (which also
contains any lines of xml with english text, since the search parameter in Option 1 is <enUS>"""

import sys


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
		if card in line.lower():
			found = True
			print(card + ' found')
			break
	if not found:
		print(card + ' NOT FOUND')


def parse_decklist(fname, cards_eng_path):
	try:
		f = open(cards_eng_path)
		deck = open(fname)
	except FileNotFoundError as e:
		print(e)
		return

	lines_ = f.readlines()
	f.close()

	for card_ in deck:
		card_ = card_.lower().strip('\n')
		find_card(card_, lines_)
	deck.close()


assert len(sys.argv) == 3, '''Please input 2 additional arguments: inFile, and secFile. \n
inFile is path to either [CardDefs.xml, CardsEng.txt] and \n
secFile is path to either [<decklist>.txt, CardsEng.txt]'''

inFile = sys.argv[1]
option = inFile[-3:]
eng_cards_path = sys.argv[2]

if option == 'xml':
	parse_cards_xml(inFile, eng_cards_path)
elif option == 'txt':
	parse_decklist(inFile, eng_cards_path)
else:
	print('Invalid option. argv[2] xml to parse CardDefs.xml, or eng to parse the created file with eng cards')
