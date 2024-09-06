# [EnumaLimunada](https://github.com/Tiniifan/EnumaLimunada/releases/latest) (CS/Galaxy to GO Converter)

Some essential CS/Galaxy files are different in GO, so you need to convert them.  
EnumaLimunada is the perfect tool for converting files from CS/Galaxy to IEGO,  
allowing you to convert models, maps, techniques and much more!

**Supported Files**
- ATR
- MTR
- PRM
- Animation (mtn2, imm2, mtm2)
- Camera (cmr2)
- Res.bin
- XPCK (pck, xc, xv)

**Supported Options**
- flags: to add iego flags
- player: to add player material

The tool comes in two versions: 
- a Graphical User Interface (GUI)
- a Command-Line Interface (CMD)
  
## GUI Version

The GUI version provides a user-friendly interface, you just have to click on "Add" (or drag n drop) and select your files, 
then select the export folder and whether or not to add options, then click on run to start conversion

![image](https://github.com/user-attachments/assets/54090ba2-f55c-4a5a-aa2a-86ead1146843)

## CMD Version
The CMD version is designed for command-line use and requires invocation with the following syntax:

```bash
EnumaLimunada.exe <FilePath> [options]
````

### Available option
-flags : Force adding IEGO properties to archives or RES files."
-player : Add player-specific properties to models (only applicable to .atr files or XPCK archives)

Example

  `EnumaLimunada.exe cp2024a.xc -player`  
  `EnumaLimunada.exe myCustomPlayer.xc -flags - player` 

Your converted file will be in the converted folder
