# TicTacToe
This repository contains a Discord Bot which allows you to play TicTacToe against other players or an AI using an straightforward interface ([TicTacToe.Discord](/TicTacToe.Discord)).

It also contains a simple TicTacToe library ([TicTacToe.Game](/TicTacToe.Game)) and a CLI which allows you to play TicTacToe against a very bad AI ([TicTacToe.Cli](/TicTacToe.Cli)).

## Setup
### Add the official bot
You can add the bot to your server via this link: https://discord.com/api/oauth2/authorize?client_id=899279391346016286&permissions=137439341632&scope=bot%20applications.commands

### Self host
Alternatively you can download the latest release from the [Releases tab](https://github.com/TheDusty01/TicTacToe/releases) and host it yourself.\
Make sure to provide the settings via environment variables or through the [appsettings.json](/TicTacToe.Discord/appsettings.json) file.

You can also run this app in a docker container, a [Dockerfile](/TicTacToe.Discord/Dockerfile) is already ready to be used.

## Usage
1. Adjust the settings
2. Invite the bot to your server
3. Use the ``/tictactoe`` command to start a game of Tic Tac Toe against the AI or ``/tictactoe @opponent`` to start a game of Tic Tac Toe against another player.
4. Example: ``/tictactoe @Dusty``

## Build
### Visual Studio
1. Open the solution with Visual Studio 2022
2. Build the solution
3. (Optional) Publish the solution

### .NET CLI
1. ``dotnet restore "TicTacToe.Discord/TicTacToe.Discord.csproj"``
2. ``dotnet build "TicTacToe.Discord/TicTacToe.Discord.csproj" -c Release``
3. (Optional) ``dotnet publish "TicTacToe.Discord/TicTacToe.Discord.csproj" -c Release``

Output directory: ``TicTacToe.Discord\TicTacToe.Discord\bin\Release\net5.0`` \
Publish directory: ``TicTacToe.Discord\TicTacToe.Discord\bin\Release\net5.0\publish``

## Credits
This project uses the following open-source projects:
- https://github.com/DSharpPlus/DSharpPlus
- https://github.com/feelingnothing/DSharpPlus.Menus

## License
TicTacToe is licensed under the MIT License, see [LICENSE.txt](/LICENSE.txt) for more information.