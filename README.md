# Sabber_Work_2019F
For my Master's Thesis, I will be conducting research using Hearthone to compare common strategies. Because Hearthstone isn't open source, we are using an open source software developed by HearthSim called SabberStone (link below). SabberStone has an AI built in which will play a full game of Hearthstone against another AI, and output very detailed logs about the final decisions it makes. We can utilize these logs in order to investigate the high-level decisions made by each player in a study to quantify the strategies, and in fact compare if evolved strategies via Neural Networks perform better than these baseline agents. 

## Final Presentation (PPT Form)
The PDF is available in this repo. 

The publicly available PPT is available in this [Drive Link](https://docs.google.com/presentation/d/1CmDJkEJGiCRlZxZFhQ7sxtBU6EGc7qcT9f041sCTgeM/edit?usp=sharing).

## TODO

- [x] Retrieve relevant turn information per player from Sabber
- [ ] Format the tasks and cards into the data set (Optional?)
- [x] Confirm that the NeuralNetScore works
- [x] Run games to evolve Warlock Aggro and Warlock Control
- [x] Compare the baseline strategy matchups against each other
- [x] Compare the baseline strategy matchups against the evolved ones
- [x] Type up Thesis document (ongoing)
 
# Getting Started
## Prerequisites
.NetCore 2.0

## How to Run
Please note that the full instruction set lies in the file in [/SabberTMTestCore/README.md](https://github.com/cww5/Sabber_Work_2019F/tree/master/SabberTMTestCore)

## Benchmarks
| Title | Time |
| ----- | ---- |
| Local | 4 mins | 

## Noteworthy Observations
For now, the notion of Teachable Moments is put aside. It doesn't seem feasible given the time frame to define a brand new concept.

## Built With
* [Sabberstone] https://github.com/HearthSim/SabberStone - Original Repository as of August 10, 2019
* [Python 3 (Anaconda)] https://www.anaconda.com/distribution/ - Used for analysing logs and data processing
* [Hearthstone Meta] https://www.hearthstonetopdecks.com/top-standard-legend-decks-from-rise-of-shadows-week-14-july-2019/ - July 2019
* [Hearthstone Meta] https://www.hearthstonetopdecks.com/top-standard-legend-decks-from-rise-of-shadows-week-12-june-2019/ - June 2019
* [Rise of Shadows Budget Zoolock] https://www.hearthstonetopdecks.com/decks/budget-zoo-warlock-deck-list-guide-rise-of-shadows/
* [Map Elites for Hearthstone] https://github.com/fernandomsilva/Hearthstone-map-elites-helpers/tree/master/10k%20Code%20Revamp/GamePlayer/GamePlayer - Used to help distribute the games in HPC cluster environment
* [EvoStone] https://github.com/tehqin/EvoStone - Used to evolve strategies to compare against the baseline ones.




## Teachable Moments?
- [ ] Create a train / test data set of games
- [ ] Find an encoding for decisions, and/or extract which features go into the model
- [ ] Build models to predict which turns are "teachable"
- [ ] Create some plots / tables
