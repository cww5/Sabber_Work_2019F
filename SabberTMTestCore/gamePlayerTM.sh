#!/bin/sh
#
# Usage: gputest.sh
# Change job name and email address as needed 
#        
 
# -- our name ---
#$ -N TMTest
#$ -S /bin/sh
# Make sure that the .e and .o file arrive in the
#working directory
#$ -cwd
#Merge the standard out and standard error to one file
#$ -j y
# Send mail at submission and completion of script
# Specify GPU queue
#$ -q medium
#$ -t 1-400
#$ -l mem_free=14.0G
/bin/echo Running on host: `hostname`.
/bin/echo In directory: `pwd`
/bin/echo Starting on: `date`
 
# Load CUDA module
## module load mono
module load dotnet
#Full path to executable
##numGames / stepsize must be integer > 1
dotnet run /home/c/cww5/Sabber_Work_2019F/SabberTMTestCore/SabberTMTestCore/Program.cs gpuid=$SGE_TASK_ID numGames=10 stepsize=2 log=true folder=results playerdecks=/home/c/cww5/Sabber_Work_2019F/thesis-decklists/zoolock_decks.csv opponentdecks=/home/c/cww5/Sabber_Work_2019F/thesis-decklists/controlock_decks.csv

