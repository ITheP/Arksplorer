Arksplorer - The Ark Explorer App!
© 2021 MisterB@ITheP

Simple front end that takes bits of Ark data including data exposed by an ark server to help you find things in Ark.

Loads data - as required - we don't load all data available as often as we can to make sure server load is minimised.

Made avaiable under the Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International (CC BY-NC-SA 4.0) License.

See https://creativecommons.org/licenses/by-nc-sa/4.0/ for more details.

Artwork (c) respective artists - supplied and used in good faith, various published under Creative Commons CC BY-NC-SA 3.0 License - see ark.gamepedia.com for further details.

If you believe any content infringes on anything, or would like a credit adding, please contact and we will happily update accordingly!

This version is considered a preview version. Information, supporting files, etc. may currently be incomplete.

The `ServerSetup` folder contains details on what needs to be set up on an Ark server for it to work with Arksplorer.

Change History...

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
- Major refactoring of code into proper classes etc.
- Major restructuring of files
- Misc. fixes and improvements
- Multiple server.json files can be specified in /Servers/ (so server owners can just drop in their own)
- Ordering by colour now possible! (Color order manually set in lookup data)
- Search by colour (including fuzzy searching of nearby colours)

ToDo:
- Click on a colour on a dino and it sets the search color
- Need to disable color searches for entities that don't have colors (e.g. survivors)