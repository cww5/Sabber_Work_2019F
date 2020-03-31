# SabberTMTestCore

## Summary
This repository is the newest updated code which is configured to run standalone in an HPC environment, and it is also configured to use the EvoStone NeuralNetScore for players. 

## Before Starting
This is the directory for running N games against two simulated agents, either using the default Scoring functions, or the evolved Scoring Functions. To configure CMA-ME experiments for evolving Scoring Functions, visit the original template at [this repo: EvoStone-master/TestBed/StrategySearch](https://github.com/cww5/Sabber_Work_2019F/tree/master/EvoStone-master/TestBed/StrategySearch). 

NOTE: The current running examples of evolving Scoring Functions using are in [this repo: /thesis-output/StrategySearchResults](https://github.com/cww5/Sabber_Work_2019F/tree/master/thesis-output/StrategySearchResults).

## Dependencies:

SabberStoneCoreAi.dll and SabberStoneCore.dll (compiled from the main repo)

## How to Build for Local and HPC Environment

>dotnet build SabberTMTestCore.csproj

>dotnet publish -r win-x64 --self-contained false -p:UseAppHost=true

>dotnet bin\Debug\netcoreapp2.2\win-x64\publish\SabberTMTestCore.dll folder=resultsTemp playerdecks=C:\Users\Main\Documents\GitHub\Sabber_Work_2019F\thesis-decklists\zoolock_decks.csv opponent_decks=opponent_decks.csv

## Helpful Links
https://stackoverflow.com/questions/52860789/how-to-execute-framework-dependent-deployment-using-dotnet-publish-with-remote-h

https://stackoverflow.com/questions/50048591/couldnt-find-a-project-to-run-ensure-a-project-exists-in-d-home-site-wwwroot

https://www.ace-net.ca/wiki/FAQ#Eqw:_Job_waiting_in_error_state

https://support.microsoft.com/en-us/help/890015/you-receive-a-the-process-cannot-access-the-file-because-it-is-being-u

## Sample Runs from Different Machines: 

### Sample Runs from Laptop:

>dotnet run Program.cs

>dotnet run Program.cs playerdecks=C:\Users\watson\Documents\GitHub\SabberStone-master\Sabber_Work_2019F\thesis-decklists\player_deck.csv opponentdecks=C:\Users\watson\Documents\GitHub\SabberStone-master\Sabber_Work_2019F\thesis-decklists\opponent_deck.csv

>dotnet run Program.cs playerdecks=C:\Users\watson\Documents\GitHub\SabberStone-master\Sabber_Work_2019F\thesis-decklists\NNDecks\nnagg.csv opponentdecks=C:\Users\watson\Documents\GitHub\SabberStone-master\Sabber_Work_2019F\thesis-decklists\opponent_deck.csv weights=C:\Users\watson\Documents\GitHub\SabberStone-master\Sabber_Work_2019F\thesis-output\StrategySearchResults\Warlock_Net_AA_sm\logs\fittest_log.csv

>dotnet run Program.cs playerdecks=C:\Users\watson\Documents\GitHub\SabberStone-master\Sabber_Work_2019F\thesis-decklists\player_deck.csv opponentdecks=C:\Users\watson\Documents\GitHub\SabberStone-master\Sabber_Work_2019F\thesis-decklists\opponent_deck.csv weights=C:\Users\watson\Documents\GitHub\SabberStone-master\Sabber_Work_2019F\thesis-output\StrategySearchResults\Warlock_Net_AA_sm\logs\fittest_log.csv


### Sample Runs from MIXR:

>dotnet run Program.cs playerdecks=C:\Users\Main\Documents\GitHub\Sabber_Work_2019F\thesis-decklists\player_deck.csv opponentdecks=C:\Users\Main\Documents\GitHub\Sabber_Work_2019F\thesis-decklists\opponent_deck.csv

>dotnet run Program.cs playerdecks=C:\Users\Main\Documents\GitHub\Sabber_Work_2019F\thesis-decklists\player_deck.csv opponentdecks=C:\Users\Main\Documents\GitHub\Sabber_Work_2019F\thesis-decklists\NNDecks\nnagg.csv

### Sample Runs from Kong:

>dotnet /home/c/cww5/SabberNewDir/bin/Debug/netcoreapp2.2/win-x64/publish/SabberTMTestCore.dll gpuid=$SGE_TASK_ID numGames=10 stepsize=2 log=true folder=ZLvsNNA_1k02 playerdecks=/home/c/cww5/SabberNewDir/zoolock_decks.csv opponentdecks=/home/c/cww5/SabberNewDir/nncontrol.csv weights=/home/c/cww5/SabberNewDir/fittest_log_AA_sm.csv

