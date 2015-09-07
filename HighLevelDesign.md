# Project Goal #
This page describes the proposed design and implementation of a stand alone subtitles plugin that interacts with Moving Pictures. The purpose of this stand alone plugin is to download subtitles for a movie via a UI in the MediaPortal GUI.

While the focus of this document focuses on the interaction between Moving Pictures and this new Subtitles plugin, it is important to note that the subtitles plugin can and eventually will interact with other plugins as well, such as !MP-TVSeries.

# Use Case #
  1. The user browses through their movie collection and selects a movie to watch.
  1. The user notices via the user interface that this movie is in a foreign language and they do not have subtitles for this movie.
  1. The user then clicks a "Get Subtitles" button.
  1. A new window opens up with a user interface for the user to select and download a subtitle file for the selected movie.
  1. Once the subtitle downloading has completed, the user clicks back to return to the original movie page.
  1. The user should now be able to visually confirm that the movie does in fact have a subtitle now.
  1. The user clicks play and the films starts with subtitles enabled.

# Technical Overview #
As explained above the subtitles module will be a stand alone plug-in. It will have it's own configuration panel in the MediaPortal configuration screen and it will implement it's own !GUIWindow class for the GUI.

In the Moving Pictures GUI skin a new button will need to be added that links to the subtitles plugin. This will not need to be added to the !MovingPicturesGUI.cs and it should only be visible when the subtitles plugin is installed. This button would be fully implemented by the skinner.

When this button is clicked it will launch the !GUIWindow for the Subtitles plugin.  The subtitles plugin will then access Moving Pictures code to determine the currently selected movie. This should be done via the property from the static MovingPicturesCore class:
```
   MovingPicturesCore.MovieBrowser.SelectedMovie 
```
It is important to note that the MovieBrowser property of MovingPicturesCore does not yet exist and will be added in the next release of Moving Pictures. The SelectedMovie property will return a !DBMovieInfo object.

Once the Subtitles Plugin downloads an appropriate subtitle it should then call UpdateMediaInfo() and Commit() on each !DBLocalMedia object contained by the !DBMovieInfo object. These calls will cause the subtitle flag to be updated in the DBLocalMedia object and will in turn trigger an update in the Moving Pictures GUI. ''It is important to note that this update currently is not implemented and will need to be added in the next release of Moving Pictures.''

At this point the user should exit the subtitles plugin by clicking the back button and the Moving Pictures GUI will be updates to indicate there is now a subtitle available.