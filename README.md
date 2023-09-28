
# Bence's Uexp Utility


![BUU](https://i.imgur.com/vOH2W6I.png)



## Features

- Convert .mp3 or .wav to the correct format
- Automatically modifies Uexp
- If [wwise_pd3](https://github.com/MoolahModding/wwise_pd3) is present, it encodes the Wav into an Ubulk file
- Works with drag and drop
## Usage

![Usage](https://i.imgur.com/1uUAvcA.gif)

 1. Extract the asset you want to change using [Fmodel](https://moolah.dev/docs/modding-basics/using-fmodel/)
 2. Have ffmpeg and wwise_pd3 installed, or next to the exe
 3. Rename the music file to the id of the extracted files
 4. Drag the music file on top of the tool
 5. Enjoy!
 
 The tool can also be used in cli, by providing the path to the music file
 
 *Example:*

    but.exe 324756076.mp3

