# HyperSearch

Development requirements:
 
* Visual Studio 2015
* C#
* .NET Framework 4.0


## Config

All config is stored in **Settings.json**.

#### Non-UI supported settings 

Most of the settings can be configured using the built-in UI but some require manually changing the settings in the file.

```json
{
    <!-- The following controls how long (in milliseconds) a certain trigger key needs to be held down before it triggers. Set to 0 to trigger immediately. -->
    "Input": {
        "Triggers": {
            "SearchTriggerDelayInMilliseconds": 0,
            "FavouritesTriggerDelayInMilliseconds": 0,
            "GenreTriggerDelayInMilliseconds": 0,
            "SettingsTriggerDelayInMilliseconds": 0,
    }
}

```

##### Misc

???

#### Input configuration 

Each input config key takes a comma-separated list of one or more **Key code** values ([see reference](https://msdn.microsoft.com/en-us/library/system.windows.input.key%28v=vs.110%29.aspx) for allowed values).

For example, the following entry configures the search window to trigger when either F1 or F3 is pressed.

```xml
<add key="Keys.Trigger.Search" value="F1,F3"/>
```

##### Cab Mode
Just a note about Cab Mode. If disabled the user is allowed to type keys on the keyboard as per normal. However, any key configured as an input key will take precedence. 

For example if your input keys are configured as follows:

```xml
<add key="Keys.Up" value="W"/>
<add key="Keys.Right" value="D"/>
<add key="Keys.Down" value="S"/>
<add key="Keys.Left" value="A"/>
```
Hitting W,D,S or A will navigate the onscreen keyboard so those keys will not be *typed*.


#### Default config

The default configuration is setup with:


Config Key | Binding | Description
------------- | ------------- | -------------
Keys.Trigger.Search | F3 | Opens up the default search window
Keys.Trigger.Favourites | F4 | Searches the favourites database
Keys.Trigger.Genre | F5 | Searches the genres database
Keys.Trigger.Settings | F10 | Settings
&nbsp; | | |
Keys.Action | Enter | Confirm/Launch
Keys.Back|Backspace| Navigate back to previous screen
Keys.Exit|Escape| Closes the search window
Keys.Minimize|Tilde| Minimizes the search window. Reactivating should bring up the search window in its previous state.
&nbsp; | | |
Keys.Up|Up Arrow|Moves current selection upwards
Keys.Right|Right Arrow|Moves current selection rightwards
Keys.Down|Down Arrow|Moves current selection downwards
Keys.Left|Left Arrow|Moves current selection leftwards