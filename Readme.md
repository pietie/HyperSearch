# HyperSearch

Development requirements:
 
* Visual Studio 2015
* C#
* .NET Framework 4.0


### Config

Refer to **App.config** for more detail on certain keys.

#### Input configuration 

Each input config key takes a comma-separated list of one or more **System.Windows.Input.Key** values ([see reference](https://msdn.microsoft.com/en-us/library/system.windows.input.key%28v=vs.110%29.aspx) for allowed values).

For example, the following entry configures the search window to trigger when either F1 or F3 is pressed.

```xml
<add key="Keys.Trigger.Search" value="F1,F3"/>
```

##### Cab Mode
Just a note about Cab Mode. If disabled the user is allowed to type keys on the keyboard as per normal. However, any key configured as an input key will take precedence. 

For example if your input keys are configured as follows.


```xml
<add key="Keys.Up" value="W"/>
<add key="Keys.Right" value="D"/>
<add key="Keys.Down" value="S"/>
<add key="Keys.Left" value="A"/>
```