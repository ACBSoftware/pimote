PiMote
------

This is a personal project that displays security camera images and controls music playback.

Disclaimer:
* This code comes with no guarantees or warranties. I accept no responsibility for any use.
* Use at your own risk, for personal curiosity only. I assume no liability for anything. 
* I don't plan on doing much maintenance to this version, so if you want to make a bunch of changes you probably should fork a copy. 

Technologies Used:
1. Windows 10 IoT v.10.0.15063.540 running on a Raspberry Pi 3 with the Raspberry Pi Official 7 inch touch screen display.
2. Blue Iris security software Web server (currently I'm at v4.5.9.8 x64).
3. KODI (My app works with V17.3 on a Raspberry Pi (LibreElec) OR a Windows 10 PC.)
4. Visual Studio 2017, C#, Xaml

Notes:

1. Portrait mode is currently a hack, until a Windows 10 IoT version comes out that correctly supports portrait mode on this display.
2. This is a rough proof of concept version. Some hardcoded stuff too. It's not pretty, but it gets the job done for me. You won't find any "MVVM" or other acronym soup that people use to suck all the fun out of development these days... 
You click a button to tell the PC what to do and code behind the button is there to do it - what could be simpler than that?
(Actually there are some helper classes so it's not as bad as you're thinking.)

	