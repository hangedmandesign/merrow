# MERROW
Merrow is a Quest 64 randomizer: a tool for randomizing the NA/U dotZ64 version of Quest 64, offering many randomization, difficulty modification, and quality-of-life options. For the full feature list, follow the itch link in the Downloads section below.

Merrow can also output randomizer options as IPS patches. You'll need an IPS-capable patching tool (ex. Lunar or Ninja) to use the patches it creates.

Along with the randomizer, Merrow has three advanced data tools that may be handy to coders and romhackers and aren't necessarily specific to Quest 64. For more detailed tool explanations, follow the itch link in the Downloads section below.
* A universal IPS patch generator, for those who know what they're doing and just want to test some specific hex address patching. 
* A binary file reader, that allows you to specify a series of addresses and lengths and easily export hexadecimal data from any binary file.
* A Z64 file checksum repair tool, because some randomizer features will modify the checksum.

Developed by Hangedman (Jonah Davidson), with the aid and support of many people, especially the Eltale Monsters Discord. Credits are available in the program itself.
Merrow was originally developed in Unity, this repository is the C# Winforms port, to make it more widely available to edit and use.

# Disclaimer
Merrow is experimental and therefore offered without warranty. Merrow patches are confirmed to work on the NA/U dotZ64 version, but may still cause unknown errors and crashes. Expect potential crashes. Patches are not made to work on the NA/U dotN64 version or any other region. Always back up files before applying.
Merrow contains no copyrighted game data or files.

# Downloads
Latest build and details available at https://hangedman.itch.io/merrow.

# Repo Overview
The project is a C# Visual Studio WPF application. When compiled, it will create **Merrow.exe**, the standalone application. That entire application is contained in the "Merrow" folder, which contains the Merrow project solution and the Merrow project code:
* *MerrowStandard.cs*, which contains the randomization and winforms application code, further detailed below in the **Code Structure** section.
* *QuestPatchBuild.cs*, which contains the Quest randomizer patch builder and related functions, detailed below in the **Code Structure** section.
* *HackFunctions.cs*, which contains the generic patch generator and binary file reader functionality, further detailed below in the **Code Structure** section.
* *VarFunctions.cs*, which contains various variable translation functions for easy converting of bytes/hex/strings/colors.

The above are all partial classes in the MerrowStandard namespace. The following are kept deliberately separate:
* *DataStore.cs*, which contains arrays and tables of reference data used for randomizer calculation and generation.
* *crc64.dll*, a DLL implementation of some old community code for repairing N64 CRCs, modified to work standalone from a filename rather than requiring a bitstream.

The rest of the files (especially *MerrowStandard.Designer.cs*) are auto-generated as part of WPF and should never be manually edited, to avoid future build issues. To edit the visual layout of Merrow, right-click on the *MerrowStandard.cs* file in the Solution Explorer, and select *View Designer*.

The other folder on the root level "crc64" is a C++ Visual Studio DLL project, which compiles into the **crc64.dll** file, which must be included alongside the compiled Merrow.exe for the CRC (checksum) Repair Tool to work. It's unlikely to change, so an up-to-date build of it is provided in the Merrow folder.

# MerrowStandard.cs Code Structure
This is a short overview of the code structure, contained in the class *MerrowStandard:Form*. Comments in the code explain each section in more detail.
* Variable declarations. New winforms objects should ideally only be created through the Winforms Properties interface in the Designer, so that variable names will auto-update throughout. Large arrays should be stored in DataStore.cs, not here.
  - "library" is the imported DataStore.cs
  - "fix_crc" is the connected crc64.dll
  - Prefixes "rnd","exp" are winforms objects in the *Quest 64 Randomizer* tab
  - Prefix "adv" are winforms objects in the *Generic Patch Generator* tab
  - Prefix "bin" are winforms objects in the *Binary File Reader* tab
  - Prefix "crc" are winforms objects in the *CRC Repair Tool* tab
* *MerrowStandard*: initialization functions.
* *Shuffling*: Updating of seed, creation of new Random() and shuffling of all randomized elements performed in a row, to guarantee seed consistency.
* General functions not created by Winforms, including *RepairCRC*, which calls the fix_crc dll connection.
* UI interactions, for modifying the interface on interaction and for calling other functions. Roughly ordered by tab. These should ideally only be created through the Winforms Properties interface in the Designer, so that variable names will auto-update throughout. None should be left empty.

# QuestPatchBuild.cs Code Structure
* UI-object list building functions.
* *BuildPatch*: Assemble Quest 64 randomizer content into *patchcontent* hexadecimal string, convert to bytestream and export as IPS, export spoiler log. This section should not contain any randomization.
* A few minor data handling functions for item randomization purposes.

# HackFunctions.cs Code Structure
* *BuildCustomPatch*: Assemble custom IPS patch content into *patchcontent* hexadecimal string, convert to bytestream and export as IPS.
* *BinRead*: Binary file reader functionality, reading from selected file at specified addresses and exported as comma-separated strings.

# License
Merrow is copyright (c) 2021 Jonah Davidson (Hangedman).
Merrow is made freely available under the MIT License:

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
