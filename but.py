
from binascii import hexlify,unhexlify
from sys import argv,exit
from subprocess import PIPE,run,CalledProcessError,call
from os import path,remove,mkdir,stat
from shutil import copy

wwise = False
def littleEndian(size:int):
    size = list(hex(size))
    del size[0:2]
    size = size[::-1]
    if len(size)%2==1:
        size.append("0")
    endianNum = ""
    for i in range(0, len(size), 2):
        endianNum+=(str(size[i + 1]) + str(size[i]))
    endian32 = endianNum
    endian64 = endianNum
    currlen = len(endianNum)
    for i in range(currlen,8):
        endian32+="0"
    for i in range(currlen,16):
        endian64+="0"
    return (endian32,endian64)

##Checking for a file
if len(argv) < 2:
    print(f"Drag an .mp3 or a .wav file with the same name of your ubulk file on the exe.\nExample:\n\t324756096.ubulk 324756096.mp3|.wav")
    input()
    exit(1)


file_path = argv[1]
name_without_extension, file_extension = path.splitext(file_path)

if not path.exists(file_path):
    print(f"Please provide an existing file.")
    input()
    exit(1)

##Checking for the appropriate extension
if file_extension not in ['.wav','.mp3']:
    print(f"You must provide a file with either the .wav or .mp3 extension.")
    input()
    exit(1)

###Checking if the {id}.ubulk file is in the running folder
if not path.exists(f'{name_without_extension}.ubulk') or not path.exists(f'{name_without_extension}.uexp'):
    print("You must have the {id}.ubulk and {id}.uexp file in the same folder as the {id}.wav|.mp3 file")
    input()
    exit(1)

###Checking if ffmpeg is installed
try:
    run(['ffmpeg.exe', '-version'], stdout=PIPE, stderr=PIPE, check=True)
    print("Ffmpeg is installed and available.")
except CalledProcessError as e:
    print("Ffmpeg is not available. Error:", e)
    input()
    exit(1)
except FileNotFoundError:
    print("Ffmpeg is not installed. Please download and install it from https://ffmpeg.org/download.html",)
    input()
    exit(1)

###Checking if wwise is installed

try:
    run(['wwise_pd3.exe' ], stdout=PIPE, stderr=PIPE, check=True)
    print("Wwise_pd3 is installed and available. Your files will be encoded automatically")
    wwise = True
except CalledProcessError as e:
    print("Wwise_pd3 is not available. Error:", e)
    input()
except FileNotFoundError:
    print("Wwise_pd3 is not installed. To encode your files instantly, download it from https://github.com/MoolahModding/wwise_pd3", )
    input()



print("Calling ffmpeg\n")
##Ffmpeg call
call(['ffmpeg.exe','-hide_banner','-loglevel','error','-y','-i',f'{file_path}','-c:a','pcm_s16le',f'{name_without_extension}.wav'])
print("\nFfmpeg done\n")
with open(f'{name_without_extension}.uexp', 'rb') as f:
    hexdata = hexlify(f.read()).decode('utf-8')

size = stat(f"{name_without_extension}.ubulk").st_size
newsize = stat(f"{name_without_extension}.wav").st_size


oldsizes = littleEndian(size)
newsizes = littleEndian(newsize)

hexdata = hexdata.replace(oldsizes[0],newsizes[0])
hexdata = hexdata.replace(oldsizes[1],newsizes[1])


hexdata = bytes(hexdata,'utf-8')
with open(f"{name_without_extension}.uexp", "wb") as binary_file:
    binary_file.write(unhexlify(hexdata))

print(f"Wrote changes to {name_without_extension}.uexp")

if not wwise:
    input()
    exit(1)

print("Encoding the .wav file\n")
remove(f"{name_without_extension}.ubulk")

call(['wwise_pd3.exe','-encode',f'{name_without_extension}.wav',f'{name_without_extension}.ubulk'])

print("\nDeleting temporary files")

output = name_without_extension


if not path.exists("old"):
    mkdir("old")

if path.exists(f"{name_without_extension}.mp3"):
    copy(f"{name_without_extension}.mp3","old")
    remove(f"{name_without_extension}.mp3")

if not path.exists(output):
    mkdir(output)
remove(f"{name_without_extension}.wav")
copy(f"{name_without_extension}.ubulk",output)
remove(f"{name_without_extension}.ubulk")
copy(f"{name_without_extension}.uexp",output)
remove(f"{name_without_extension}.uexp")

try:
    copy(f"{name_without_extension}.uasset",output)
    remove(f"{name_without_extension}.uasset")
except:
    print("Couldn't move {id}.uasset")
print("Done!")
input()

