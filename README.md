# This is a Pre-Release Project
The Skillprint Unity SDK is still in early development and the Developer 
Dashboard is not yet publicly available. We look forward to launching 
to the public soon! Check [skillprint.co](https://skillprint.co) for the 
latest information or to contact us for early access.
# Get Started with the Skillprint Unity SDK
## Add the Package to your Unity Project
You can add this package to Unity as a Git dependency. (Note that you must have
git installed and available on your path.)
1. In the Unity editor, open the package manager (Window > Package Manager)
2. Press the + button and select "Add package from git URL..."
3. Input the URL for this repository and press Add: 
```
https://github.com/skillprint/unity-sdk.git
```

The Skillprint SDK package includes an assembly definition file which you can 
reference from other assemblies as necessary. See the [Unity docs](
https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html
) for more information on using assembly references.

## Initialize the SDK
In the GameManager (or some other central location convenient to you), 
declare Skillprint and initialize it using the Game ID provided on the 
Developer Dashboard.
```c#
private Skillprint _skillprint;

private void Awake()
{
    _skillprint = Skillprint.Instance;
    _skillprint.Init("example-game-id");
}
```

## Game Session Management
When the game begins, you must call 
```c#
_skillprint.GameSessionStart()
```
to begin writing events into a new session. This creates a new Session and 
logs a `GAME_START` event in that Session. 

To close the session, call 
```c#
_skillprint.GameSessionEnd()
```
See the Skillprint developer documentation for more information on Sessions.

## Sending Standard Events
The Skillprint Unity API provides methods to log our Standard Events. Currently we have 6 standard events defined. They are:
* LEVEL_START
* LEVEL_COMPLETE
* LEVEL_FAILED
* LEVEL_QUIT
* LEVEL_RESTART
* HINT

We have created some helper functions to fire the standard events. They are defined below:

* `public void LevelStart(int level)`
* `public void LevelFailed(int level)`
* `public void LevelComplete(int level)`
* `public void LevelQuit(int level)`
* `public void LevelRestart(int level)`
* `public void Hint(int level)`
* `public void GenericPositive(int level)`
* `public void GenericNegative(int level)`

Example Usage:
```c#
Skillprint.LevelStart(
    2 // Level Number
);
```

## Sending Custom Events
To send a custom event, use the `SendEvent` method, which takes two arguments: 
the name of the event type you want to send, and a dynamically-valued 
dictionary of parameters to attach to the event. See the Developer 
Documentation for more information on using Custom Events.
```c#
_skillprint.SendEvent(
    "BALL_MOVEMENT", // Event type
    new Dictionary<string, dynamic> // Parameters
    {
        ["speed"] = BallRigid.velocity.magnitude,
        ["position_x"] = BallRigid.position.x,
        ["position_y"] = BallRigid.position.y,
        ["level_over"] = false,
    }
);
```

### Showing the Result Panel
If your game has defined Custom Session Metrics in the Developer Dashboard, 
you can display the Skillprint Result Panel as a modal over the game:
```c#
_skillprint.ShowWebViewContent()
```
Note that you must [close the Session](#game-session-management) before you 
can show Session results to the user.

