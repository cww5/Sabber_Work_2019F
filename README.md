# Sabber_Work_2019F
For my Master's Thesis, I will be conducting research using Hearthone to dedect teachable moments. Because Hearthstone isn't open source, we are using an open source software developed by HearthSim called SabberStone (link below). This repository has an AI built in which will play a full game of Hearthstone against another AI, and output very detailed logs about the final decisions it makes. We can utilize these logs in order to investigate the high-level decisions made by each player in a study to determine which of these turns (moments) are in fact teachable (or not). 

## TODO

- [x] Retrieve relevant turn information per player from Sabber
- [ ] Format the tasks and cards into the data set
- [ ] Create a train / test data set of games
- [ ] Find an encoding for decisions, and/or extract which features go into the model
- [ ] Build models to predict which turns are "teachable"
- [ ] Create some plots / tables
- [ ] Type up Thesis document (ongoing)
 
# Getting Started
## Prerequisites
.NetCore 2.0

## Sample CMD Runs

>dotnet build SabberTMTestCore.csproj
>dotnet publish -r win-x64 --self-contained false -p:UseAppHost=true
>dotnet bin\Debug\netcoreapp2.2\win-x64\publish\SabberTMTestCore.dll folder=resultsTemp playerdecks=C:\Users\Main\Documents\GitHub\Sabber_Work_2019F\thesis-decklists\zoolock_decks.csv opponent_decks=opponent_decks.csv

## Helpful Links
https://stackoverflow.com/questions/52860789/how-to-execute-framework-dependent-deployment-using-dotnet-publish-with-remote-h
https://stackoverflow.com/questions/50048591/couldnt-find-a-project-to-run-ensure-a-project-exists-in-d-home-site-wwwroot
https://www.ace-net.ca/wiki/FAQ#Eqw:_Job_waiting_in_error_state
https://support.microsoft.com/en-us/help/890015/you-receive-a-the-process-cannot-access-the-file-because-it-is-being-u

## Benchmarks
| Title | Time |
| ----- | ---- |
| Local | 4 mins | 

## Noteworthy Observations


## Built With
* [Sabberstone] https://github.com/HearthSim/SabberStone - Original Repository as of August 10, 2019
* [Python 3] https://www.python.org/ - Used for analysing logs
* [Hearthstone Meta] https://www.hearthstonetopdecks.com/top-standard-legend-decks-from-rise-of-shadows-week-14-july-2019/ - July 2019
* [Hearthstone Meta] https://www.hearthstonetopdecks.com/top-standard-legend-decks-from-rise-of-shadows-week-12-june-2019/ - June 2019
* [Rise of Shadows Budget Zoolock] https://www.hearthstonetopdecks.com/decks/budget-zoo-warlock-deck-list-guide-rise-of-shadows/
