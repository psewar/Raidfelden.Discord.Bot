﻿#!/bin/bash
until dotnet Raidfelden.Discord.Bot.dll; 
do 
    # Restart the Bot when the exit code was not 0
    sleep 100
done