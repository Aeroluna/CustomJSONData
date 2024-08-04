# CustomJSONData
CustomJSONData is a library that allows the loading of arbitrary data from specific locations in beatmap JSON files. Custom data can be attached to levels (songs), individual difficulties of songs, and individual notes, obstacles, and lighting events within difficulties. In addition, entirely new event types can be added. CustomJSONData completely overwrites how custom maps are deserialized from JSON.

This README is for modders, end users just need to install the mod!

# V4 format
Custom data in v4 is not currently supported and not planned, see [#15](https://github.com/Aeroluna/CustomJSONData/issues/15). V4 maps will not have their types replaced by their custom variants. This means it is not currently possible to read custom data in any form on v4 maps.

# Custom data in info.dat
Custom data can also be placed on entire levels (songs) and on individual difficulties. For example:
```json
// ...
"_songFilename": "song.ogg",
"_coverImageFilename": "cover.png",
"_environmentName": "DefaultEnvironment",
"_customData": {
  "_contributors": [
    {
      "_role": "Furry",
      "_name": "Reaxt",
      "_iconPath": "furry.png"
    },
    {
      "_role": "Lighter",
      "_name": "Kyle 1413 The Second",
      "_iconPath": "test.png"
    }
  ],
  "_customEnvironment": "CoolCustomEnv",
  "_customEnvironmentHash": ""
},
"_difficultyBeatmapSets": [
  {
    "_beatmapCharacteristicName": "Standard",
    "_difficultyBeatmaps": [
      {
        "_difficulty": "Easy",
        "_difficultyRank": 1,
        "_beatmapFilename": "Easy.dat",
        "_noteJumpMovementSpeed": 0.0,
        "_noteJumpStartBeatOffset": 0,
        "_customData": {
          "_difficultyLabel": "Nightmare"
        }
// ...
```
This data can be accessed by casting a `BeatmapData` to `CustomJSONData.CustomBeatmap.CustomBeatmapData`. The first "_customData" section (the one for the whole level) is provided as `levelCustomData`, and the second (the one for the Easy difficulty) is provided as `beatmapCustomData`.

Example (For more about reading custom data, see [Reading custom data](#Reading-custom-data)):
```csharp
if (beatmapData is CustomBeatmapData customBeatmapData)
{
    string customEnvironment = customBeatmapData.levelCustomData.Get<string>("_customEnvironment"); // "CoolCustomEnv"
    string label = customBeatmapData.beatmapCustomData.Get<string>("_difficultyLabel"); // "Nightmare"
}
```

Although not as recommended, the custom data section is also available from a `StandardLevelInfoSaveData` by casting to `CustomJSONData.CustomLevelInfo.CustomLevelInfoSaveData` and accessing the customData property. `beatmapCustomData` is a little more complicated. In order to do so, cast a `StandardLevelInfoSaveData.DifficultyBeatmap` to a `CustomLevelInfoSaveData.DifficultyBeatmap`.

Example:
```csharp
if (standardLevelInfoSaveData is CustomLevelInfoSaveData customLevelInfoSaveData)
{
    string customEnvironment = customLevelInfoSaveData.customData.Get<string>("_customEnvironment"); // "CoolCustomEnv"

    CustomLevelInfoSaveData.DifficultyBeatmap difficultyBeatmap = (CustomLevelInfoSaveData.DifficultyBeatmap)customBeatmapData.difficultyBeatmapSets.First().difficultyBeatmaps.First();
    string label = difficultyBeatmap.customData.Get<string>("_difficultyLabel"); // "Nightmare"
}
```

In pre-1.35 builds, this custom data is additionally stored in the `BeatmapSaveData` and can be read from a `IDifficultyBeatmap` using provided extensions methods (see [CustomData extensions](#CustomData-extensions))

In post-1.35 builds, the custom data is available from casting a `FileBeatmapLevelData`/`FileDifficultyBeatmap` to a `CustomFileBeatmapLevelData`/`CustomFileDifficultyBeatmap` or a `BeatmapBasicData` to a `CustomBeatmapBasicData` (`CustomBeatmapLevel` is not implemented due to a compatibility issue with PlaylistManager). These conversions can be done easily using the same extension methods.

# Custom data on notes, obstacles, waypoints, and lighting events
Custom data can be attached to notes (including bombs), obstacles, waypoints, and lighting events simply by adding a `customData` (or `_customData` if v2) property to the event/note/obstacle/waypoint object in the difficulty JSON file. For example, adding some custom fields to a note:
```json
"colorNotes": [
  {
    "b": 8.0,
    "x": 2,
    "y": 0,
    "c": 1,
    "d": 1,
    "customData": {
      "foo": 3,
      "bar": "Hello, BSMG!"
    }
  }
]
```
To get this data from a `NoteData`, `ObstacleData`, `WaypointData`, `SliderData`, or `BasicBeatmapEventData` object in your plugin code, cast it to the appropriate type from the `CustomJSONData.CustomBeatmap` namespace (`CustomNoteData`, `CustomObstacleData`, `CustomWaypointData`, `SliderData`, or `CustomBasicBeatmapEventData`) and access the resulting object's customData property. Alternatively, all of these classes inherit the `ICustomData` interface, which requires the `customData` property.

Example (For more about reading custom data, see [Reading custom data](#Reading-custom-data)):
```csharp
if (noteData is CustomNoteData customNoteData)
{
    int foo = customNoteData.customData.Get<int>("foo"); // 3
}

if (noteData is ICustomData customDataInterface)
{
    int foo = customDataInterface.customData.Get<int>("foo"); // 3
}
```

*Note: The recommended way to create custom events to trigger plugin functionality is with CustomJSONData's [custom events](#Custom-events) feature. Custom data on lighting events should be used when your plugin does something related to the Beat Saber lighting event the data is placed on (e.g. changing the color of a group of lights or the direction of a ring spin), not to create new event types.*

# Burst Sliders
Unlike other objects, burst sliders (or chains) are handled by creating many `NoteData`s from one `SliderData`. All of these `NoteData`s will inherit the same `CustomData` object from their original `SliderData`.
```json
"burstSliders": [
  {
    "b": 15,
    "c": 0,
    "x": 0,
    "y": 0,
    "d": 2,
    "tb": 16,
    "tx": 0,
    "ty": 0,
    "sc": 10, // slice count, this will create 10 notes
    "s": 1,
    "customData": {
      "color": [0, 1, 0] // all 10 notes will point to the same CustomData reference
    }
  }
]
```

# Reading custom data
In CustomJSONData, all JSON objects are converted to the `CustomData` class, which inherits from `ConcurrentDictionary<string, object?>`, and all JSON arrays are converted to a `List<object?>`.

After getting your customData (see above), accessing your data is as simple as accessing from a dictionary.
```csharp
if (beatmapData is CustomBeatmapData customBeatmapData)
{
    string label = customBeatmapData.beatmapCustomData["_difficultyLabel"]; // possible KeyNotfoundException, dont actually do this!
}
```

To help with this, the `CustomData` class provide the method `Get<T>(string key)`. This will return the value as `T` if it exists, else it will return `default(T)`. `Get` will convert numeric types between each other but will not convert `string`s or `boolean`s to numeric types.

Full example of getting a color from an array:
```csharp

/* json:
  "colorNotes": [{
      "b": 10,
      "x": 1,
      "y": 0,
      "c": 0,
      "d": 1,
      "customData": {
        "color": [0.5, 1, 0]
      }
    }
  ]
*/

if (noteData is CustomNoteData customNoteData)
{
    // get the customData
    Dictionary<string, object> dictionary = customNoteData.customData;

    // get the _color array
    // remember that arrays are always a List<object> and must be casted
    List<object>? colorArray = dictionary.Get<List<object>>("color");

    if (colorArray == null || colorArray.Count < 3)
    {
      return;
    }

    // Convert to floats and save color
    IEnumerable<float> colorFloats = colorArray.Select(n => Convert.ToSingle(n));
    Color color = new Color(colorFloats.ElementAt(0), colorFloats.ElementAt(1), colorFloats.ElementAt(2)); // (0.5, 1, 0)
}
```

As a reminder, `customData` IS mutable, i.e. you can modify it at any time. Want to store a variable inside a note?
```csharp
customNoteData.customData["colorz"] = new Color(0, 0, 1);
// insert code here
Color noteColor = customNotedata.Get<Color>("colorz"); // (0, 0, 1);
```

If your mod reads v2 and v3 maps, it may be desirable to have different property names depending on what kind of map you are reading. Which version you are working with can be found with the `version` property of all types that inherit the `IVersionable` interface, which includes most common types like `CustomBeatmapData`, `NoteData`, `ObstacleData`, `WaypointData`, `SliderData`, or `BasicBeatmapEventData`. These can be parsed by comparing with the static versions available in `VersionExtensions` class which include `version2`, `version3`, and `version4`, or alternatively use the `IsVersion2` extension method (see [CustomData extensions](#CustomData-extensions)).
```csharp
bool v2 = customBeatmapData.version.IsVersion2();
bool? foo = dictionary.Get<bool>(v2 ? "_foo" : "f");
```

# CustomData extensions
To aid with reading CustomData, there are many extensions for frequently used methods.

`CustomData` methods:
* `Get<T>(string key)` - As described before.
* `GetVector3(string key)` - Runs `Get<List<object>>` and if found, converts result to a `Vector3`, otherwise returns null.
* `GetQuaternion(string key)` - As above but with a `Quaternion`.
* `GetStringToEnum<T>(string key)` - Runs `Get<string>` and converts result to `T` or `default(T)`. `T` must be an enum. Must be nullable to return null or will return default.
* `GetRequired<T>(string key)` - Runs `Get<T>` but throws a `JsonNotDefinedException` if it returns null. `T` should not be nullable.
* `GetStringToEnumRequired<T>(string key)` - See above. `T` should not be nullable.
* `ToString()` - ToString is overriden to help print object contents in a human readable format.

`IDifficultyBeatmap` extensions:
* Pre-1.35:
  * `GetBeatmapSaveData(this IDifficultyBeatmapData difficultyBeatmap)` - Returns the map's `CustomBeatmapSaveData` or null if not a Custom Level.
  * `GetBeatmapCustomData(this IDifficultyBeatmapData difficultyBeatmap)` - Returns the map's `beatmapCustomData` or a new `CustomData`.
  * `GetLevelCustomData(this IDifficultyBeatmapData difficultyBeatmap)` - Returns the map's `levelCustomData ` or a new `CustomData`.
* Post-1.35:
  * `GetBeatmapCustomData(this IBeatmapLevelData beatmapLevelData, in BeatmapKey beatmapKey)` - Same as above.
  * `GetLevelCustomData(this IBeatmapLevelData beatmapLevelData)` - Same as above.
  * `GetBeatmapCustomData(this BeatmapLevel beatmapLevel, in BeatmapKey beatmapKey)` - Same as above.
  * `GetLevelCustomData(this BeatmapLevel beatmapLevel)` - Same as above.

`Version` extensions:
* `IsVersion2(this Version version)` - Returns whether version is less than `version3`.

# Custom events
In addition to providing access to the custom data found in info.dat, `CustomJSONData.CustomBeatmap.CustomBeatmapData` provides a new list of `CustomEventData` objects. Not to be confused with `CustomBasicBeatmapEventData`, which are vanilla Beat Saber lighting events with custom data added, this is a place for entirely new events added to the game by plugins. These events are stored in difficulty `.dat` files. Here's an example of what a custom event might look like inside a difficulty:
```json
"version": "3.0.0",
"customData": {
  "customEvents": [{
      "b": 0.0,
      "t": "HelloWorld",
      "d": {
        "foo": 1.0,
        "message": "Hello from a custom event!"
      }
    }
  ]
},
"basicBeatmapEvents": [
// ...
```
A CustomEventData object has three fields.
* `b` (or `_time` in v2) functions identically to the `b` field on notes/obstacles/lighting events.
* `t` (or `_type`) Unlike in lighting events, this is a string; use it to specify what sort of event this is. Event types are de-facto defined/standardized by the first plugin to make use of them.
* `d` (or `_data`) is custom data. To see how to access it, see [Reading custom data](#Reading-custom-data))

To subscribe to these events, you must register a callback through vanilla class the `BeatmapCallbacksController`. You can get this through any means, but Zenject will be used for this example.

From there you can invoke `AddBeatmapCallback<CustomEventData>(BeatmapDataCallback<T> callback)`. You can also call `AddBeatmapCallback<CustomEventData>(float aheadTime, BeatmapDataCallback<CustomEventData> callback)` if you want an `aheadTime`, which is how long in seconds before the event you want the callback to trigger.

Example:
```csharp
[Inject]
private BeatmapCallbacksController _beatmapCallbacksController;

private BeatmapDataCallbackWrapper? _callbackWrapper;

public void Initialize()
{
    _callbackWrapper = _beatmapCallbacksController.AddBeatmapCallback<CustomEventData>(HandleCallback);
}

public void Dispose()
{
    _beatmapCallbacksController?.RemoveBeatmapCallback(_callbackWrapper);
}

private void Callback(CustomEventData customEventData)
{
    if (customEventData.type == "HelloWorld")
    {
        string message = customEventData.customData.Get<string>("message"); // "Hello from a custom event!"
    }
}
```

# CustomJSONDataDeserializer
## For advanced use only!
CustomJSONDataDeserializers allow you to modify how data is read within the `customData` field of a beatmap. These only work on v3 maps. Currently one is used to read fake notes from a separate array in Noodle Extensions and will be used as an example (it has been slightly simplified). First, one must be registered to a class with the `[JSONDeserializer]` attribute on a method. This created CustomJSONDataDeserializer can be enabled and disabled with the `Enabled` property.
```csharp
public static CustomJSONDataDeserializer JSONDeserializer { get; } = CustomJSONDataDeserializer.Register<FakeNotesJSON>();
```

`FakeNotesJSON` looks like this. When CustomJSONData comes across the string parameter defined in `[JSONDeserializer]`, it will redirect to this method. Returning false indicates do not add to the beatmap custom data while returning true mean do add.
```csharp
[CustomJSONDataDeserializer.JSONDeserializer("fakeColorNotes")]
private static bool HandleFakeNotes(CustomBeatmapSaveData.SaveDataCustomDatas customDatas, List<BeatmapSaveData.ColorNoteData> colorNotes, JsonTextReader reader)
{
    // Basic check to make sure this only runs for NE maps
    // WARNING: CustomBeatmapSaveData.SaveDataCustomDatas is only injected for pre-1.35 builds!
    if (!(customDatas.beatmapCustomData.Get<List<object>>("_requirements")?.Contains("Noodle Extensions") ?? false))
    {
        return true;
    }

    // More info: https://www.newtonsoft.com/json/help/html/readjsonwithjsontextreader.htm
    // See CustomBeatmapSaveData and JsonExtensions to see how CJD uses helper methods
    reader.ReadObjectArray(() =>
    {
        CustomBeatmapSaveData.ColorNoteData data = CustomBeatmapSaveData.DeserializeColorNote(reader); // Read from reader head to create a new note save data
        data.customData["NE_fake"] = true; // tags as fake to be read later
        colorNotes.Add(data); // Add to the injected colorNotes list, which will be fed back to CJD
    });
    return false;
}
```

A `[JSONDeserializer]` method may request the following types to be injected: `JsonTextReader`, `List<BeatmapSaveData.BpmChangeEventData>`, `List<BeatmapSaveData.RotationEventData>`, `List<BeatmapSaveData.ColorNoteData>`, `List<BeatmapSaveData.BombNoteData>`, `List<BeatmapSaveData.ObstacleData>`, `List<BeatmapSaveData.SliderData>`, `List<BeatmapSaveData.BurstSliderData>`, `List<BeatmapSaveData.WaypointData>`, `List<BeatmapSaveData.BasicEventData>`, `List<BeatmapSaveData.ColorBoostEventData>`, `List<BeatmapSaveData.LightColorEventBoxGroup>`, `List<BeatmapSaveData.LightRotationEventBoxGroup>`, `List<BasicEventTypesWithKeywords.BasicEventTypesForKeyword>`, `bool`(useNormalEventsAsCompatibleEvents), `List<CustomEventData>`, `CustomBeatmapSaveData.SaveDataCustomDatas`
