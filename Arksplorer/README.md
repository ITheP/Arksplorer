Arksplorer - The Ark Explorer App!
© 2021 MisterB@ITheP

==============================================================================================================
Arksplorer uses .net 6 framework. If you don't have this installed and aren't running an all in one version of
Arksplorer, please install this first!

https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime

Recommend you install the x64 version under `Run desktop apps`
==============================================================================================================

Simple front end that takes bits of Ark data including data exposed by an ark server to help you find things in Ark.

Loads data - as required - we don't load all data available as often as we can to make sure server load is minimised.

Made avaiable under the Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International (CC BY-NC-SA 4.0) License.

See https://creativecommons.org/licenses/by-nc-sa/4.0/ for more details.

Artwork (c) respective artists - supplied and used in good faith, various published under Creative Commons CC BY-NC-SA 3.0 License - see ark.gamepedia.com for further details.

Dino images are loaded from Dododex - many thanks to Danlev and Dodoex for giving permission to use their images.

If you believe any content infringes on anything, or would like a credit adding, please contact and we will happily update accordingly!

This version is considered a preview version. Information, supporting files, etc. may currently be incomplete.

The `ServerSetup` folder contains details on what needs to be set up on an Ark server for it to work with Arksplorer.

Release change history...

v0.5 Preview - 02/03/2021
- First public version

v0.6 Preview - 04/03/2021
- Config now read from Ark server rather than local hardcoded
- Other UI changes

v0.7 Preview - 15/03/2021
- Map availability now comes from Ark server config
- Various UI changes
- Code improvements
- Initial code for auto-refresh of cache
- Ark.Gamepedia embedding
- Dododex embedding
- Taming countdown timer

v0.8 Preview - 24/03/2021
- Various UI improvements
- Sorted out the cache so it refreshes properly
- Sorted out the UI so it shows updated cache data properly
- Bulk display of creatures on the map (including level visualisation)

v0.81 Preview
- Outlining around mass dino visualisation blocks
- Extra cases of filter applying on chances

v0.9 Preview - 28/03/2021
- Merged any Info on selected dino/hover over info into one
- Pop up info inprovements
- Icons (for sex, cryo state)
- Hover over dino in list info pop up
- Hover over mass marker info pop up
- Colouration changes to mass marker
- Optimisations
- Other UI changes

v1.0 The First Bite - 06/04/2021
- First `more public` release!
- Huge data translation lookup table for Ark <-> Arksplorer <-> Ark wiki <-> Dododex
- Icons in DataGrid results view
- Corrected spelling of Dodex to Dododex :)
- Direct link to Ark wiki creature page and Dododex taming page for selected creature (including passing of server and dino data)
- Funky new logo
- Extra server details (such as taming rates etc.)
- Extra browser tab to server website
- Visualisation of server configuration data
- Added extra data in info pop ups
- Show `main info` from selected mass marker
- Added option to turn on/off extra details in pop up info
- Added .ico icon file

v1.1 The Bigger Bite - 10/04/2021
- Added a splash screen
- Added error/crash log
- Enabled async startup of app (for future expansion)
- Reworked browser tab layout to work better with resizing
- Included Map/Tribe/Dino quick filter buttons
- Minor reworking of pop up info layout
- Minor reworking of other parts of interface
- Major refactoring of code into more classes etc.
- Major restructuring of files
- Misc. fixes and improvements
- Multiple server.json files can be specified in /Servers/ (so server owners can just drop in their own)
- Ordering by colour now possible! (Color order manually set in lookup data)
- Search by colour (including fuzzy searching of nearby colours)

v1.2 The Snaggled Tooth - 16/04/2021
- Enable/Disable filter controls depending on data type
- Hide messages that shouldn't stick around
- Make buttons flash under certain conditions to help with usage between controls
- Neatened up main window code
- Moved web browser components into reusable user control
- Made flash message more colorfull!

v1.2.3 The Snagglier Tooth - 19/04/2021
- Can turn on/off the mouse over pop up information windows
- Right click on colors to set that as the Filter Color
- Custom marker (click on map)
- Zoom into map (centered on selected dino or custom marker)

v1.3 - The Flappy Wing - 04/05/2021
- Level filter includes selected level rather than excluding
- Saving a few extra settings to user settings
- YouTube browser (as people end up watching loads of ark related ones)
- Mouse wheel zooms in and out
- Last 3 dino's selected appear in Ark Wikipedia and Dododex webbrowser tabs quick link buttons
- Alarm times changed a bit (removed rarely used ones)
- Alarm has +/- 1 minute buttons now to increase/decrease current timer
- Alarm will now flash window as well
- Changed button highlighting to make it look a bit nicer
- Handles web browser crashes (attempts to recreate a new instance)
- Clicking on a mass marker will (if possible) selected the selected creature in the results list/data grid
- Misc. optimisations

v1.4 - The Fearsome Roar
- Major update to server setup information (to help people set up their servers)
- Inclusion of example batch files used on Wiredcat server
- Right click on filter buttons to append rather than replace filter value on Colors and other relevant filter buttons
- Filter also has a Drop Down List that remembers history of last searches you ran. Removes duplicates. Always puts latest search at top.
- Improved some UI elements
- Included a lot more tool tips
- Alarm now includes a drop down of more durations, and a user specified `last selected` button to quick repeat access the last drop down duration selected
- Can add and remove alarms
- Dynamically picks up alarm audio types from /audio/ folder
- Shrunk UI on left hand side for alarm space and to help with people with lower vertical resolutions
- Move and update of instructions/help and about information into a Help tab
- Added non-crash exception logging to crash file
- Various refactoring
- Improved error handling, reporting and logging with more detail
- Extra exception handling

v1.4.1
- UI shrink test for user with screen space problems

v1.4.2 - The Even Bigger Roar
- Side bar tweaks to help with spacing, scrollable alarm area and count
- Zoom in to map now fills all of map area with image, rather than region map originally appears in

v1.4.3 - The Distant Roar
- Autochecks with a server for new versions + links through if found
- Minor refactoring

v1.5.0 - The Shiny New Scale
- Updated data handling for genesis 2 problem with sex
- Put in new Genesis 2 map
- Put in new Gensis 2 Dino lookup data (first version, info out there on these is not all complete yet it seems, Arkpedia + Dododex links included)
- Put in some other Dino lookup data that was also missing
- Put in new icon for non male/female sex (N/A) - though this data isn't feeding through yet from the server
- Note: Appears that there are some bugs in the app that creates the data Arksplorer uses, until these are worked out, we can't add much other new stuff in

v1.6.0 - The Large Feather
- Remembers last window position and size, and Map area size
- Zoom, popups and detail prefs now saving
- Alarms now saving if you change them
- Loading status shown next to map selection list
- Secondary data result area, copying data from main result area (allows for comparisons)
- Second marker for secondary result area selected dinos (assuming same map visible)

v1.6.1 - The Fluffy Feather
- Something up with the Genesis 2 map image (looks like it was corrupt) - replaced
- Better handling of corrupt images for maps
... Saving/loading of timers includes audio
- Show image of selected dino in pop up (from Dododex) - first version, some what fragile!
... Searches will ignore unticked maps
- Selection of Mass Marker mode colouration - sex/level or creature colours
- Added extra filter quick buttons and changed their order
- Creature type dataset labeling for results view
- Popups display from different creature types in main/secondary results areas
- Filter allows for OR statements using | separator, e.g. Rag | Ext, Argent | Dodo ...Argentavis or Dodo's on Ragnarock or Extinction

v1.6.2 - The Fallen Feather
- Updated some of the Help information

v1.7.0 - The Pointy Talon
- Updated framework to use .net 6.0
- Inclusion of Lost Island map
- Few other minor adjustments/tweaks

v1.7.1 - The Blunted Talon
- Adding back in ability to generate text to speech
- Type text into Alarm for speech instead of pre-defined audio sample

ToDo:
- Loading status not using WPF bound updates, cheaty refresh method used. Nice to do this properly.
- if (CurrentRectanglePopUpInfo != null) <-- need a different way of checking this, so we can still click on mass markers when popups are hidden
- Nursing Effectiveness
- Holder needed while downloading cached images from servers, and better handling if it fails
- Stop showing data/searching in maps that are de-selected
- Make a demo video and link through to home page(s)
- Contact other ark server admin and see if we can get them to join in!
- Include exclusions in filter
- Replace data storage internally with little db engine, speed up searching/better queries etc.
- Extra audio warnings (e.g. breeding timer)
- Rather than shipping maps with Arksplorer, can we download (on first run) from Wiki?
- Not entirly sure everything updating in a timely manner, to check
- Improve UI to make it clearer you are looking at wild/tame/etc.
- Suvivors often have a coordinate of 0,0 (old accounts no longer used?) - be good to have some UIX around that
- Need to add in new Lost Island dinos!
- Settings tab
- Set marker transparency

Known fixes needed:
- Details button should toggle details in selected dino
- Show all not always working (possibly data not being requiried at all)
- Click to center map without orange marker
