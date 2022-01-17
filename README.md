# ToBot
Generic Discord bot with dynamically loaded plugins. The purpose of this bot is to create single bot that can support various ad-hock added plugins. Plugins may have various purposes, for example monitoring various webpages (through scrapping or RSS channels). See [Plugins](https://github.com/tariel36/ToBot#Plugins) for examples.

## Build, configuration, installation

### Build
Build the solution, it should restore required packages and build successfully.

### Prepare bot directory
Create directory where executable, dlls and configs will be stored.

### Prepare configuration
Create `appsettings.json` file with input parameters. File contains single `JSON` object with `arguments` (array) property that stores all input parameters.

```json
{
    "arguments": [
        "-sp",
        "<absolute path to current directory>",
        "-c",
        "<absolute path to config.json file (see below)>",
        "-ns",
        "<absolute path to netstandard.dll file>",
        "-scsn",
        "<service name>",
        "-scdn",
        "<service display name>"
    ]
}
```

There are more parameters, see `-help` for details.

Example path to `netstandard.dll` file - `C:\\Program Files\\dotnet\\packs\\Microsoft.NETCore.App.Ref\\3.1.0\\ref\\netcoreapp3.1\\netstandard.dll`

If you have any plugins you want to run, add following lines for each plugin:

```json
"-m",
"<plugin file name>",
```

Create `config.json` with config parameters.

```json
{
  "Token": "<your bot token>",
  "TokenType": 1,
  "UseInternalLogHandler": false,
  "LogLevel": 8,
  "CommandPrefix": "<global command prefix>",
  "DatabaseFilePath": "<absolute file path to db file>",
  "LogsDirectory": "<absolute directory path to logs directory>",
  "BackupDirectory": "<absolute directory path to backups directory ",
  "NotificationTimeout": 36000000000,
  "StatisticsNotificationTimeout": 864000000000,
  "AutoReconnect": false
}
```

### Install
* Copy all binaries, `*.deps.json`, `ToBot.runtimeconfig.dev.json` and `ToBot.runtimeconfig.json` files to ealier prepared directory for your bot;
* Copy `appsettings.json` and `conifg.json` files to the same directory;
* Copy binaries and `*.deps.json` files of all plugins you want to use;

With properly prepared directory you can now install the service. Use following command to do so:
`dotnet ToBot.dll install`

Depending on where you start your command window and your environment variables configuration, you may have to use absolute paths for both `dotnet` and `ToBot.dll`. Path to the first one depends on your installation, the path to the latter one is absolute file path to `ToBot.dll` - it should be within director you have created ealier. For example:
`"C:\Program Files\dotnet\dotnet.exe" "<my absolute path>\ToBot.dll" install`

If installation succeeds, you should see something like that:
```
Configuration Result:
[Success] Name ToBot - Notifier
[Success] ServiceName ToBot - Notifier
Topshelf v4.3.1.0, .NET Core 4.6.26614.01 (4.0.30319.42000)

Running a transacted installation.

Beginning the Install phase of the installation.
Installing ToBot - Notifier service
Installing service ToBot - Notifier...
Service ToBot - Notifier has been successfully installed.

The Install phase completed successfully, and the Commit phase is beginning.

The Commit phase completed successfully.

The transacted install has completed.
```
The result may differ a bit depending on content of your `appsettings.json` file.

### Run
If installation succeeded, you should see new service in `Services` (`services.msc`). The name of your service depends on content of your `appsettings.json` file.

When you find the proper service, you can right click on it and select `Start`. Starting may take a while depending on your plugins and amount of data loaded from local database.

### Discord integration
When you get your bot running, add it to your discord server (you should already know how) and then you can use the common `help` command to see what are your possibilities.

## Create plugins
If you want to create your own plugin then you can use this repository solution as base or create new solution with following projects:
* ToBot.Plugin;
* ToBot;
* ToBot.Common;
* ToBot.Communication;
* ToBot.Data;
* ToBot.Rss;

If you want to write a scrapper or RSS based plugin, you can also reference one (or both) projects:
* ToBot.Plugin.GenericRssPlugin;
* ToBot.Plugin.GenericScrapperPlugin;

When you're done with solution, then you can create project for your plugin. It should use `Class Library (.NET Standard)` template. Your project's name have to follow the `ToBot.Plugin.<name>` pattern. When you're ready with your project, create `class` that will be an entry point of your plugin. The name is irrelevant. Your class, at some point has to inherit from `BasePlugin` `class` from `ToBot.Plugin` project.

Depending on your choices, different set of constructor parameters will be needed. See examples [Plugins](https://github.com/tariel36/ToBot#Plugins). Then you probably have to override `Name` and `Version` properties, but it depends on your previous choices.

When you're ready with your base class then you can add command handlers to it. Each command has to be `async`, return Task` and have single parameter of type `CommandExecutionContext` and has to be decorated with `IsCommand` attribute. For example:

```cs
[IsCommand]
public async Task Test(CommandExecutionContext ctx)
{
    Logger.LogMessage(LogLevel.Debug, Name, $"Plugin `{Name}` invoked `Test` command.");
    await Task.CompletedTask;
}
```

With all that, you're done. You can now compile your plugin and put its `.dll` file and all used dependencies in the same directory as `ToBot`.

## Plugins
* HumbleBundle - https://github.com/tariel36/ToBot_Plugin_HumbleBundle
* BlackScreenRecords - https://github.com/tariel36/ToBot_Plugin_BlackScreenRecords
* Kolekcjonerki.com https://github.com/tariel36/ToBot_Plugin_KolekcjonerkiCom
* ŁowcyGier - https://github.com/tariel36/ToBot_Plugin_LowcyGierPl
* PPE - https://github.com/tariel36/ToBot_Plugin_PpePl

## This project uses following libraries:
* [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus);
* [TopShelf](http://topshelf-project.com/);
* [Json.NET](https://www.newtonsoft.com/json);
* [CodeHollow.FeedReader](https://github.com/arminreiter/FeedReader/);
