README
------

* This code comes with no guarantees or warranties. I accept no responsibility for any use.
* Use it at your own risk, for personal use only. I assume no liability for anything. 
* I don't plan on doing much maintenance to this version, so...
* If you want to make a bunch of changes you probably should fork a copy. 

This is a personal project that uses the following:
1. Windows 10 IoT v.10.0.15063.540 running on a Raspberry Pi 3 with the Raspberry Pi Official 7 inch touch screen display.
2. Blue Iris security software Web server (currently I'm at v4.5.9.8 x64).
3. KODI (My app works with V17.3 on a Raspberry Pi (LibreElec) OR a Windows 10 PC.)
	
My remaining to-do list:

1. High Priority
	A-Z buttons or page down for lists > n entries
	Simulate non-network response on all http calls/Handle all exceptions
	Sync playlist from pre-existing after 1st contact with Kodi and allow start w/o add 1st song.
	Play/Pause button: Is something playing? If yes then fine issue Play/Pause, if not open playlist 0 or see above line.
	Get more stuff on the screen / Formatting.
	Handle translating m3u playlists within the folder structure.
	Are you sure on clear everything button.

2. Low
	Use templates to shrink the list view size
	Show currently playing song on playlist / poll
	Don't reset data source if nothing has changed.
	Hidden window with rolling logs
	Yamaha Receiver Remote
	Grace Digital Remote http://forum.universal-devices.com/topic/8346-upnp-help-grace-digital-media-player/
	Complete all TODOS in Visual Studio task list
	Allow Kodi playlists other than 0 (Show list from Kodi on settings screen?)
	Persistent Configuration values on Pi local storage
	Get active players from Kodi for correct-ness instead of just using 0/
	Observable collection not working to show playing songs in list

NOTES/REFERENCE LINKS:
----------------------
Icon sources: 
1. https://preloaders.net/en/free#
2. https://www.iconfinder.com/free_icons
3. https://openclipart.org
4. http://findicons.com/

Controlling brightness:
https://www.raspberrypi.org/forums/viewtopic.php?f=105&t=152495
https://www.raspberrypi.org/forums/viewtopic.php?f=108&t=177574
https://social.msdn.microsoft.com/Forums/en-US/e7392ab6-bb32-4583-bd0a-8eb015c2745f/raspberrypi-7-zoll-display-and-backlightbrightness?forum=WindowsIoT

Portrait mode:
https://social.msdn.microsoft.com/Forums/en-US/1d989376-98a7-4165-82c9-2eaf8dfc7454/how-to-rotate-touch-panel-on-touchscreen-on-rpi?forum=WindowsIoT
https://buildazure.com/2016/08/06/fix-windows-iot-core-raspberry-pi-touchscreen-display-upside-down/
https://social.msdn.microsoft.com/Forums/Windows/en-US/1d989376-98a7-4165-82c9-2eaf8dfc7454/how-to-rotate-touch-panel-on-touchscreen-on-rpi?forum=WindowsIoT&prof=required
https://superuser.com/questions/406502/how-can-i-reverse-mouse-movement-x-y-axis-system-wide-win-7-x64
https://developer.microsoft.com/en-us/windows/iot/Samples/DriverLab3

Random Links:
http://forum.kodi.tv/showthread.php?tid=210549
http://kodi.wiki/view/JSON-RPC_API/v6#Player.PlayPause

Commands for changing config.txt on my old Pi:

PiMusic:/flash # mount -o remount,rw /flash
PiMusic:/flash # nano /flash/config.txt
PiMusic:/flash # mount -o remount,ro /flash
PiMusic:/flash # reboot
