# SabberTMTestCore

## Summary
This repository is the newest updated code which is configured to run standalone in an HPC environment, and it is also configured to use the EvoStone NeuralNetScore for players. 

## Heads Up
This is the directory for running N games against two simulated agents. To configure CMA-ME experiments for evolving strategies, visit [this repo: EvoStone-master/TestBed/StrategySearch] (https://github.com/cww5/Sabber_Work_2019F/tree/master/EvoStone-master/TestBed/StrategySearch). 

## Dependencies:
SabberStoneCoreAi.dll and SabberStoneCore.dll (compiled from the main repo)

## Sample Runs from Laptop:

dotnet run Program.cs

dotnet run Program.cs playerdecks=C:\Users\watson\Documents\GitHub\SabberStone-master\Sabber_Work_2019F\thesis-decklists\player_deck.csv opponentdecks=C:\Users\watson\Documents\GitHub\SabberStone-master\Sabber_Work_2019F\thesis-decklists\opponent_deck.csv


## Sample Runs from MIXR:

dotnet run Program.cs playerdecks=C:\Users\Main\Documents\GitHub\Sabber_Work_2019F\thesis-decklists\player_deck.csv opponentdecks=C:\Users\Main\Documents\GitHub\Sabber_Work_2019F\thesis-decklists\opponent_deck.csv
dotnet run Program.cs playerdecks=C:\Users\Main\Documents\GitHub\Sabber_Work_2019F\thesis-decklists\player_deck.csv opponentdecks=C:\Users\Main\Documents\GitHub\Sabber_Work_2019F\thesis-decklists\NNDecks\nnagg.csv

## Sample Runs from Kong:

dotnet /home/c/cww5/SabberNewDir/bin/Debug/netcoreapp2.2/win-x64/publish/SabberTMTestCore.dll gpuid=$SGE_TASK_ID numGames=10 stepsize=2 log=true folder=ZLvsCL_1k0 playerdecks=/home/c/cww5/SabberNewDir/warlock_player.csv opponentdecks=/home/c/cww5/SabberNewDir/warlock_opponent.csv
