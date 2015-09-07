## Overview ##

**SubCentral** is a standalone subtitles plugin for the MediaPortal HTPC application. The purpose of the plugin is to allow user to search and download subtitles for movies or TV shows managed by popular Moving Pictures and MP-TVSeries plugins. It also has the interfaces to allow easy implementation and integration in any other plugin.

Best way to use SubCentral is through one of the supported plugins. If you wish, there is a possibility to modify search data SubCentral grabs from the plugins for more customized search. If plugin is opened from home screen, only manual search is possible, where you can create your own search queries for movies or TV shows.

Before using SubCentral for the first time, use MediaPortal configuration to configure the plugin thoroughly (selecting, reordering and renaming groups/providers, selecting and ordering languages and managing of the download folders).

From the GUI you can enable/disable languages (but not language priority), search, download or delete existing subtitles and sort the search results using different criteria, including unique **tag ranking** system.

If you find the plugin useful, you can support the development by donating via PayPal. Thank you!

[![](https://www.paypalobjects.com/WEBSCR-640-20110429-1/en_US/i/btn/btn_donate_SM.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=3TXC5D2X4ZPM6&lc=FI&item_name=SubCentral&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted)


## Requirements ##

  * MediaPortal v1.1.0 or higher
  * Supported skin


## Supported subtitle providers / languages ##

SubCentral uses providers (scrapers) and languages from [SubtitleDownloader API](http://www.assembla.com/spaces/subtitledownloader/). SubCentral provides a nice GUI for all the cool features SubtitleDownloader offers.

There are many providers already included in SubtitleDownloader by default. As SubtitleDownloader is very flexible and allows adding new providers, there are already few of them created by other users. All of them are included in SubCentral v1.0 and higher.

  * SubtitleDownloader official providers - seco
    * OpenSubtitles
    * Sublight
    * TVSubtitles
    * Subscene
    * Bierdopje
    * Podnapisi
    * MovieSubtitles
    * s4u.se

  * User created additional providers
    * [ItalianSubs.net](http://forum.team-mediaportal.com/mediaportal-plugins-47/subtitledownloader-2-4-a-71651/index23.html#post708045) – samo\_yea
    * [SeriesSub.com](http://forum.team-mediaportal.com/mediaportal-plugins-47/subtitledownloader-2-3-a-71651/index22.html#post705997) – samo\_yea
    * [Sous-titres.eu](http://forum.team-mediaportal.com/subcentral-544/subtitledownloader-2-9-a-71651/index26.html#post752283) – MrJul
    * [SubsCenter.org](http://forum.team-mediaportal.com/subcentral-544/subtitledownloader-2-9-a-71651/index26.html#post750495) – yoavain
    * [Subsfactory.it](http://forum.team-mediaportal.com/mediaportal-plugins-47/subtitledownloader-2-3-a-71651/index21.html#post693203) – samo\_yea
    * [Titlovi.com](http://forum.team-mediaportal.com/mediaportal-plugins-47/subtitledownloader-2-4-a-71651/index23.html#post708115) – SilentException
    * [Titulky.com](http://forum.team-mediaportal.com/mediaportal-plugins-47/subcentral-v0-9-1-download-manage-subtitles-moving-pictures-mediaportal-tvseries-others-85545/index13.html#post739052) – katulus

## Integration – supported plugins ##

  * [MP-TVSeries](http://code.google.com/p/mptvseries/)
    * SubCentral has inbuilt support (plugin handler) for MP-TVSeries v2.6.0 and higher.
  * [Moving Pictures](http://code.google.com/p/moving-pictures/)
    * SubCentral has inbuilt support (plugin handler) for Moving Pictures v1.0.3 and higher.
  * [MyFilms](http://code.google.com/p/my-films/)
    * SubCentral has inbuilt support (plugin handler) for MyFilms v5.0.1 or higher.
  * [MyVideos](http://www.team-mediaportal.com/mediaportal-features/video-dvd)
    * SubCentral has inbuilt support (plugin handler) for MyVideos for MediaPortal v1.1.0 or higher.
  * Other plugins
    * If the data from the plugin can be accessed from the "outside", there is a possibility of writing new plugin handler. Consult manual and source code for more info.
    * There is also a possibility of integration without modifications in SubCentral. SubCentral supports data exchange through MediaPortal's GUI window messages. Consult manual for more info.
    * Another option is just to do manual search for subtitles you're after. SubCentral will allow you to rename and save subtitles to defined folders.


## Developing / translating / help ##

You're a talented developer wanting to help us? Wrote a new plugin handler? Modified SubCentral so it works better/faster/stronger? Translated the plugin? Write to us here, on IRC (freenode, usually on [#MovingPictures](irc://irc.freenode.net/MovingPictures) and/or [#MP-TVSeries](irc://irc.freenode.net/MP-TVSeries)) or use issue tracker to submit your patches.


