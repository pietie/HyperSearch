# HyperSearch

Development requirements:
 
* Visual Studio 2015
* C#
* .NET Framework 4.0


## CLI options

You can run HyperSearch with the following command line options. If an instance of HyperSearch is already running the command will be send to that instance.

Option | Description
---- | ----
-search|Open full search
-genre|Open genre search
-fav|Open favourites search
-settings|Open settings 

## Config

All config is stored in **Settings.json**.

#### Non-UI supported settings 

Most of the settings can be configured using the built-in UI but some require manually changing the settings in the file.

```javascript
{
    // The following controls how long (in milliseconds) a certain trigger key needs to be held down before it triggers. Set to 0 to trigger immediately.
    "Input": {
        "Triggers": {
            "SearchTriggerDelayInMilliseconds": 0,
            "FavouritesTriggerDelayInMilliseconds": 0,
            "GenreTriggerDelayInMilliseconds": 0,
            "SettingsTriggerDelayInMilliseconds": 0,
    }
}

```

```javascript
{
    "General": {
        // Controls the width and height of the main HyperSearch windows if Standalone mode is enabled.
        "StandaloneWidth": 1600,
        "StandaloneHeight": 900,

        // Controls how long the balloon tooltip is shown for. Set to 0 to disable completely.
        "BalloonToolTipTimeOutInMilliseconds": 0
    }
}
```


```javascript
{
    "Misc": {
        // Controls how long to wait after game selection before showing the game video
        "GameVideoPopupTimeoutInMilliseconds": 1600
    }
}
```

<!-- 
#### Input configuration 

Each input configuration can be setup with one or more keys.


##### Cab Mode
Just a note about Cab Mode. If disabled the user is allowed to type keys on the keyboard as per normal. However, any key configured as an input key will take precedence. 

For example if your input keys are configured as follows:

Config | Binding
---- | ----
Up|W
Right|D
Down|S
Left|A

Hitting W,D,S or A will navigate the onscreen keyboard so those keys will not be *typed*.

    -->

<!-- 
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

    -->