# HyperSearch

Development requirements:
 
* Visual Studio 2015
* C#
* .NET Framework 4.0

## Getting started

Refer to the [wiki]( https://github.com/pietie/HyperSearch/wiki "HyperSearch Wiki") for instructions on initial setup and usage.


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

>**NOTE:** When specifying paths with backslashes be sure to double-up on each slash, e.g. "C:\\\Foo\\\Bar"

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
    // The following controls how long (in milliseconds) a certain trigger key needs to be held down before it fires. Set to 0 to fire immediately.
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
    "Misc": {
        // Controls how long to wait after game selection before showing the game video
        "GameVideoPopupTimeoutInMilliseconds": 1600,

        // Controls the location of System wheel images. If not specified, uses Hyperspin\Media\Main Menu\Images\Wheel by default
        "AlternativeSystemWheelImagePath": "c:\\SystemImagesPath",

        // If not specified uses the default Hyperspin\Media\[System]\Images\Wheel
        // If specified uses Hyperspin\Media\[SystemName]\Images\[AlternativeGameWheelSourceFolder]
        "AlternativeGameWheelSourceFolder": "AltWheelImages",
    
        // Specify one or more locations to source the Genre wheel images from. Paths may be relative
        "GenreWheelImageLocations": ["c:\\Path1", "c:\\Path2" ]
    }
}
```