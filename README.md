# Color Name Assistant © 2020 Stefano Gottardo

A simple software for Windows to identify the colours

![alt text](https://github.com/CastagnaIT/ColorNameAssistant/blob/master/Screenshot.jpg?raw=true)

The main purpose of this software is to help people suffering of a color vision deficiency (sometimes called color blindness),
the reduced ability to see colors causes constant discomfort in daily life, making it necessary to ask other people for support to fulfill their tasks.

There are many similar tools on the web but often each one has some gaps, usually for lack pick up colour from the screen, lack of the hue name, non-updatable colour lists, not open-source, or limited to web use.
Here i have tried to fill these gaps trying to provide a simple interface.

This software is not intended for professional use, the search for colour names is done by calculating the metrics of [colour spaces](https://en.wikipedia.org/wiki/Color_difference) (like CIEDE2000),
and is dependent on internal colour lists.

## How works

1) Press the button "Pick from screen" to choose a color from the screen, then move the mouse over the color to be chosen and click it.
2) Will be show the chosen color info

**How the color name is searched**

To find the name of the colour the software performs a search in the internal colour lists to find the best match with the color you have chosen.
For "best match" in most cases means an approximate colour which does not have the same Hex/RGB code but it is very close.

**The details panel**

In this panel you can view the best matches found with the chosen color,
you can compare the color preview of each result by enabling "Compare color preview" from "Preferences" menu.

The "distance value" indicates the distance to the chosen color,
so when the value is smaller it means that the color is more like the chosen color,
if the value is high it means that the colour is more different,
in cases where the value is 0 it means that the color is identical to the chosen color.

**The internal color lists**

The software works off-line so it does not need an internet connection,
therefore color lists from different sources are included which will be used to search for the color name and hue name.

These lists are contained in standard JSON file contained in the "ColorLists" folder and are easily upgradable
with any editor or through easy scripts like Python.

For more info read json_file_info.txt file in the "ColorLists" folder.

## Download

The download link and the executable could be flagged as malware from the browser and/or antivirus,
this because the executable has not a certified signature, signing the executable has a cost and therefore will not be applied.
You are free to ignore these warnings or compile the software yourself from the sources.

[Download ColorNameAssistant](https://www.dropbox.com/sh/ckbjlzmazooqy0t/AACQDJ-lxODD7htT-sWyRZ3Ba?dl=0)

**How to install**

The software is provided in a portable way, you just have to extract the content of zip file where you like and run the executable.

## Disclaimer

The Software is provided "as is" without warranty of any kind, either express or implied. Use at your own risk.
The use of the software is done at your own discretion and risk with the agreement that you will be solely responsible for any damage resulting from such activities.

## License

Licensed under The MIT License.
