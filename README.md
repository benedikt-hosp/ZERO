# How To
## 1. EyeTracking Controller
![If you choose to use the scripts by attaching them to a game object, you need to specify which VR HEadset you use and which EyeTracker.](HowToFiles/ChooseProvider.png "Provider")

If you choose to use the scripts by attaching them to a game object, you need to specify which VR HEadset you use and which EyeTracker.You can also
define where the GazeWriter Script will save the gazeFile.

<!-- blank line -->
Click on the image to see the video about how to add the script:

[![Watch the video](https://img.youtube.com/vi/k842mTuHbdM/hqdefault.jpg)](https://youtu.be/k842mTuHbdM)

<!-- blank line -->


<!-- blank line -->
### 1.2 How to use the code

If you choose to simply use the classes you can do that by calling the following functions.

#### Public functions

How add an object of ETController 
```
ETController etController;
etController = new ETController(Providers eyeTrackingProvider);
```

Get the set eye tracker back from the controller
```
public EyeTrackingProviderInterface getSetEyeTracker { get { return this.etpc.eyeTrackingProviderInterface; } }
```
    
Returns wether the eye tracker is calibrated or not
```
public bool isCalibrated();
```

According to the EyeTrackingProvider, the correct game objects and prefabs are loaded into the scene
```
public GameObject loadGameobject(string path, string name);
```

Some eye trackers need to subscribe to the gaze signal or enable it. So call it once, before you access data to enable it.
```
public void startET();
```

Starts the coroutine that is collecting the latest gaze samples of the eye tracker  
```
public void startSampleHarvester();
```

Stops the coroutine that is collecting the latest gaze samples of the eye tracker  
```
public void stopSampleHarvester();
```
Destructs all objects.
```
public void close();
```
<!-- blank line -->

## 2. GazeWriter


Create object of event tracker, wherever you want
```
    GazeWriter gazeWriter;
    public GazeWriter(string userFolder, EyeTrackingProviderInterface eyeTrackerObject)
```

Starts a coroutine that is periodically writing gaze samples to specific file
```
public void startGazeWriting()`
```

Stops the coroutine that writes gaze samples to file
```
public void stopGazeWriting();
```

Function returns the current system time in milliseconds
```
public long getCurrentSystemTimestamp()
```



#### Private functions

```
IEnumerator writeGazePeriodically();
WriteGazeToFile(List<SampleData> currentSamples); 
WriteGazeData(SampleData gazeData)
```
<!-- blank line -->

## 3. Add new Events and Listeners

How to Add Event listeners:

Let's assume we want to raise an event (let's call it "MyEvent") in our ExperimentController Gameobject when the key "C" on the keyboard is clicked and listen to it in our GazeWriter.

e.g.
```
public class ExperimentController
{
	.
	.
	.
	.
	
	void Update()
	{
		  if (Input.GetKeyUp(KeyCode.C))
		  {
				MyEvent();
		  }
	}
}
```


1. We have to define a delegate (which defines a callback or event listener ) somewhere public. To have a tidy code, we add it to our "Delegates.cs"
Let's call our delegate MyEvent_delegate, but you can name whatever you want.

```
public delegate void MyEvent_delegate();
```

2. We can only call events that are connected to a delegate. To create an event, 
we add it as a class member to a class in which the event will be raised.
Our event has to have the type of the delegate we just created.

e.g. 
```
public class ExperimentController
{

	public event MyEvent_delegate MyEvent;
	
	public ExperimentController(...){}
	.
	.
	.
	
}
```

Now we created an event we can raise when something happens and we created a delegate that allows us to add listeners to that event.





3. To add a listener to that event: Let's say we want to listen to that new event in our GazeWriter.

This time we have to add our listener inside the constructor of the class we want to handle the event.

```
public class GazeWriter
{
	public GazeWriter(...)
	{
		// Register a method "MyEvent_handlerMethod" to listen to the event "MyEvent"
		// raised by the Class "ExperimentController".
		ExperimentController.MyEvent += MyEvent_handlerMethod;
	}
}
```





4. Create the handler method

```
public void MyEvent_handlerMethod()
{
	// Do what you want to do when the letter c is pressed.
	e.g. write down a message with a timestamp into our events.csv file.
	
}
```


Note: It is also possible to add parameters to delegates.

1. Raise event with parameter: MyEvent(cCode);
2. Tell delegate that we want to access a parameter: public delegate void MyEvent_delegate(string cCode);
3. Add the parameter to your handler method: public void MyEvent_handlerMethod(string cCode)


And to listen to events raised by objects. Then you have to change your registration to that object:

```
ExperimentController.MyEvent += MyEvent_handlerMethod;
```
to
```
myObject.MyEvent += MyEvent_handlerMethod;
```



