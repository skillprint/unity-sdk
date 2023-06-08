# Setting up Skillprint Unity SDK
## Download the unity SDK
This section needs to be updated.

## Import Newtonsoft Json package
This pacakage is required for the Skillprint SDK to function correctly.
> Go to Window > Package Manager

<img width="464" alt="Pic 1" src="https://github.com/skillprint/unity-sdk/assets/91208213/42ba93a9-bf3f-442f-8ec5-16edd4a15dd2">

> Click on the `+` sign and select `Add Package from git url`
<img width="370" alt="Pic 2" src="https://github.com/skillprint/unity-sdk/assets/91208213/b533dda0-2fad-4994-bb89-c6fc1be54240">

> Enter `com.unity.nuget.newtonsoft-json` and press `Add`.
<img width="362" alt="Pic 3" src="https://github.com/skillprint/unity-sdk/assets/91208213/2b07a6d2-d5c8-4fe0-8773-e496fbe0695c">

> You should be able to see the added package
<img width="366" alt="Pic 4" src="https://github.com/skillprint/unity-sdk/assets/91208213/4dba371a-740a-4287-acb6-20ee50a0933d">

## Importing Skillprint SDK

> Go to Assets > Import Package > Custom Package
<img width="605" alt="Pic 5" src="https://github.com/skillprint/unity-sdk/assets/91208213/329e2fa3-8b2f-4f08-b48a-337534e621c2">

> Select the unity package from the downloaded location
<img width="798" alt="Pic 6" src="https://github.com/skillprint/unity-sdk/assets/91208213/7152cebf-3b77-491a-ac33-cdd5c9ba0ca3">

> A window should pop up with all the assets being imported. Click `Import`
<img width="504" alt="Pic 7" src="https://github.com/skillprint/unity-sdk/assets/91208213/793e31a8-2340-465c-add9-43376c89e332">


At this point the Skillprint SDK setup is complete. The next section will describe some basic usage of the script.

## Firing Events

### Initialize
In the GameManager or where the game is initialized, declare Skillprint and initialize it:
```
private Skillprint _skillprint;

    private void Awake()
    {

        _skillprint = Skillprint.Instance;
        _skillprint.Init("gameName");
    }
```

The `gameName` is the `gameId` provided to you.

### Start event log

Call `_skillprint.GameSessionStart();` to initialize the logging process.

### End event log and display results
Call `_skillprint.GameSessionEnd();` to end the event logging. Optionally you can display the result as a modal over the game by calling:
`_skillprint.ShowWebViewContent();`


