# zPing
TCP probe tool with added ICMP functionality

## Requirements
### For compiling:
- Visual Studio 2019 Community or higher
- .NET 5.0.3 SDK
### For running:
- .NET 5.0.3 Desktop Runtime

## Compiling
Clone this repository
```
git clone https://github.com/KhrysusDev/zPing.git
```

Open the .sln file with Visual Studio and rebuild.

## Usage
This is a command-line tool, that means you have to run it from a command prompt. I would recommend copying it to C:\Windows\System32 so it's added to PATH
automatically or you can manually add it to PATH if you want.

Get the latest version from the Releases.
```
zping -i 1.1.1.1
```
Will ping 1.1.1.1

```
zping 1.1.1.1 80
```
Will TCP probe 1.1.1.1 on port 80

## License
Licensed under the Common Development and Distribution license (CDDL), check LICENSE.md
