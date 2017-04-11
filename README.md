# sync-windows-app
[![Build status](https://ci.appveyor.com/api/projects/status/76fekkii1oo5whsx?svg=true)](https://ci.appveyor.com/project/edewit/sync-windows-app)

Author: Erik Jan de Wit   
Level: Intermediate  
Technologies: C#, Windows, RHMAP
Summary: A demonstration of how to synchronize a single collection with RHMAP. 
Community Project : [Feed Henry](http://feedhenry.org)
Target Product: RHMAP  
Product Versions: RHMAP 3.7.0+   
Source: https://github.com/feedhenry-templates/sync-ios-app  
Prerequisites: fh-dotnet-sdk : 3.+, Visual Studio 2015 / 2013

## What is it?

This application manages items in a collection that is synchronized with a remote RHMAP cloud application.  The user can create, update, and delete collection items.  Refer to `sync-windows-app/sync-windows-app.Shared/fhconfig.json` for the relevant pieces of code and configuration.

If you do not have access to a RHMAP instance, you can sign up for a free instance at [https://openshift.feedhenry.com/](https://openshift.feedhenry.com/).

## How do I run it?  

### RHMAP Studio

This application and its cloud services are available as a project template in RHMAP as part of the "Sync Framework Project" template.

### Local Clone (ideal for Open Source Development)
If you wish to contribute to this template, the following information may be helpful; otherwise, RHMAP and its build facilities are the preferred solution.

## Build instructions

1. Clone this project

2. Populate ```sync-windows-app.Shared/fhconfig.json``` with your values as explained [here](https://access.redhat.com/documentation/en-us/red_hat_mobile_application_platform/4.3/html/client_sdk/native-windows#native-windows-set-up-configuration).

3. Open sync-windows-app.sln

4. Run the project
 
## How does it work?

### Start synchronization

In ```sync-windows-app.Shared/MainPage.xaml.cs``` the synchronization loop is started.
```
  var client = FHSyncClient.GetInstance();
  var config = new FHSyncConfig();
  client.Initialise(config);   // [1]
  client.SyncCompleted += async (sender, args) =>
  {
      await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
      {
          ShoppingItems.Clear();
          foreach (var item in client.List<ShoppingItem>(DatasetId).OrderByDescending(i => i.Created)) // [2]
          {
              ShoppingItems.Add(item);
          }
          FirePropertyChanged("ShoppingItems");
      });
  };
```
[1] Initialize with sync configuration.

[2] Initialize a sync client for a given dataset.
