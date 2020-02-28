# TestPond
A set of CLIs, SDKs, and packages built with .NET Core for creating a private test farm for Xamarin UI Tests. 

**TestPond** is composed of three layers: the UI Tests, Runner CLI, and Client.

### UI Tests
This layer is your own UI Tests built against your app using Xamarin UI Tests. To get started building UI Tests you can read the [Microsoft documentation](https://docs.microsoft.com/en-us/appcenter/test-cloud/uitest/).
Due to limitations within the Xamarin UI Framework, you might have to perform some "manual" cleanup after the execution of each test.

### Runner
The Runner layer is a CLI tool that interfaces with NUnit to execute the UI Test without the dependency of an IDE. This allows you to automate the execution of UI Tests.

### Client
This is an optional layer that you can create based on your current configuration and requirements. You can create a GUI App or a CLI to interface with your chosen CI tool/service to automate the downloads of your builds and execute the UI Test in multiple connected phones at the same time through the Runner CLI.
At FishAngler we created an interactive Command Line tool that prompts the user which build to download from Azure Dev Ops, which phones to run it on, and which tests to execute. Furhermore, it parses all the results into a sumarry HTML report to quickly look at each run. We call it the *Creator* and we will be publishing the code as an example.

# Dependencies
If you are running this in your development machine configured to run Xamarin, then most likely you already have most of the dependencies.

### General
* NUnit 3 Console 

### For iOS Testing
* xcrun (part of XCode, but you [can install without xcode](https://mac-how-to.gadgethacks.com/how-to/install-command-line-developer-tools-without-xcode-0168115/). **Warning, this was not tested**)
* [ios-deploy](https://www.npmjs.com/package/ios-deploy)

### For Android Testing
* Android SDK
* Java SDK
* adb must be in the path
* JAVA_HOME Environmental Variable pointing to the JDK home directory
* ANDROID_HOME Environment Variable pointing to the Android SDK directory

# How to execute my tests?
`TestPond.Runner -lp path/to/packagedapp -dp android -di 9A221FFBA005EZ -apn your.app.name.apk -dlln UI.Tests.dll`

### Arguments
```
-lp|--local-path: [Required] Local path to where the application package and ui test dll live.
-dp|--device-platform: [Required] The device platform to run: 'a' for Android or 'i' for iOS.
-apn|--app-package-name: [Required] The file name of the APK or the APP to run.
-dlln|--dll-name: [Required] The file name of the UI Test dll to run.
-nuw|--nunit-where: The Nunit where clause for test selection.
-di|--device-id: The Android Device ID to run the tests against. Required when running for Android.
-dn|--device-name: The iOS Device Name to run the tests against. Required when running for iOS.
-dix|--device-index: An arbitrary index that gets passed in to the UI Test project.
-dip|--device-ip-address: The iOS Device IP Address. If not passed in, it is retrieved using the Device Name.
-rdp|--result-dir-path: Specify the Result Directory Path for the for Test Artifacts like the TestResult.xml, screenshots, etc.
```

# Using Nuget
Coming soon

# Build from source
1. Use your favorite terminal/command tool
2. Navigate to the root of TestPond.Runner using
3. Execute `dotnet publish` based on your Operating System
4. Copy `appsettings.json` to the same directory as the final TestPond.Runner executable and modify as needed

### Mac
`dotnet publish -r osx-x64 -c Release-OSx /p:PublishSingleFile=true /p:PublishTrimmer=true --self-contained true`

### Windows
`dotnet publish -r win-x64 -c Release-Windows /p:PublishSingleFile=true /p:PublishTrimmer=true --self-contained true`

