[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT) ![Built With C#](https://img.shields.io/badge/Built_with-C%23-green.svg)

# ChilliSource.Mobile.Media #

This project is part of the ChilliSource framework developed by [BlueChilli](https://github.com/BlueChilli).

## Summary ##

```ChilliSource.Mobile.Media``` provides services and controls to easily work with the media library and camera, and perform audio and video playback and recording.

## Usage ##

Please note that most of the features below are currently only supported on iOS.

### Media Library ###

**Existing Media Items**

To pick an existing image or video from the device's media library, call the ```PickImage``` or ```PickVideo``` methods of the ```MediaLibraryService``` and specify the path to which the media item should be saved once the native OS media library modal screen is dismissed. E.g.

```csharp
var mediaLibraryService = DependencyService.Get<IMediaLibraryService>();
mediaLibraryService.PickVideo(outputFilePath);
```

**New Media Items**

To capture a new photo or video using the native OS screens, call the ```CapturePhoto``` or ```CaptureVideo``` methods of the ```MediaLibraryService``` and specify the path to which the media item should be saved. E.g.

```csharp
var mediaLibraryService = DependencyService.Get<IMediaLibraryService>();
mediaLibraryService.CaptureVideo(outputFilePath);
```

### Audio Playback ###

To play an audio file, first initialize the ```AudioPlaybackService``` with the desired file path, and then use the ```Play, Pause, Resume, Rewind, Stop``` methods. E.g.

```csharp
var audioService = DependencyService.Get<IAudioPlaybackService>();
audioService.Initialize(audioFilePath);
audioService.Play();
```

### Audio Recording ###

To record audio to a file, first initialize the ```AudioRecordingService``` with path of the new audio file to be created, and then use the ```Record, Pause, Stop, Clear``` methods. E.g.

```csharp
var audioService = DependencyService.Get<IAudioRecordingService>();
audioService.Initialize(newAudioFilePath);
audioService.Record();
audioService.Stop();
```

### Video Playback ###

To play a video using the native OS video player, use the ```PlayVideo``` method of the ```VideoService```, which will open a native video player screen and play the video at the path specified. E.g.

```csharp
var videoService = DependencyService.Get<IVideoService>();
videoService.PlayVideo(videoPath);
```

To play a video directly on your page, use the ```VideoPlayerNativeView``` control:

```csharp
var videoPlayer = new VideoPlayerNativeView();
videoPlayer.LoadFromPath(videoFilePath);
videoPlayer.OnPlaybackCompleted += () =>
    {
        _videoPlayer.Pause();
        _videoPlayer.Reset();
    };

//add video player to xamarin forms stack layout
var layout = new StackLayout
{
    Children = {_videoPlayer.ToView()}
};
```

### Video Recording ###

To record a video using the native OS video recording screens, call the ```CaptureVideo``` method of the ```MediaLibraryService``` and specify the destination file path. E.g.

```csharp
var mediaLibraryService = DependencyService.Get<IMediaLibraryService>();
mediaLibraryService.CaptureVideo(outputFilePath);
```

To record a video using a camera view in your app, see the Camera section below.

### Camera ###

To show the camera input on a page, add an instance of ```CameraNativeView```, and call the ```StartCapture``` method, E.g.

```csharp
var cameraView = new CameraNativeView();
await cameraView.Setup(enableAudioRecording: true, enableStillImageCapture: true, orientation: preferredOrientation);
cameraView.StartCapture();
```

To record a video using the camera view, call the ```StartRecording``` method, E.g.

```csharp
cameraView.OnRecordingFinished += (outputFileUrl, completionCallback) =>
    {
        completionCallback();
    };

await cameraView.StartRecording(outputFilePath);
cameraView.StopRecording();
```

To capture a photo, call the ```SnapStillImage``` method, E.g.

```csharp
cameraView.OnImageSnapped += (NSData imageData) => 
    {
        //save image here
    };

cameraView.SnapStillImage();
```

## Installation ##

All ChilliSource libraries are available via NuGet [here](https://www.nuget.org/packages/ChilliSource.Mobile.Media).

## Releases ##

See the [releases](https://github.com/BlueChilli/ChilliSource.Mobile.Media/releases).

## Contribution ##

Please see the [Contribution Guide](.github/CONTRIBUTING.md).

## License ##

ChilliSource.Mobile is licensed under the [MIT license](LICENSE).

## Feedback and Contact ##

For questions or feedback, please contact [chillisource@bluechilli.com](mailto:chillisource@bluechilli.com).


