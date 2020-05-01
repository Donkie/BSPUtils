# BSPUtils
A set of command line .NET Core programs for interacting with the Binary Space Partition .bsp file format in the Source Engine created by Valve.

### BSPLumpExtract
Lets you clear and extract specific lumps of a .bsp into .lmp files.

### BSPPak
Lets you pack map content into the .bsp using an intelligent .gitignore-style file filter to only pack the files you want.

### BSPLumpCompare
Compares the offset, size and contents of BSP lumps.

### LibBSP
The library that the above programs use. Available as a NuGet package at https://www.nuget.org/packages/LibBSP/.

Supports reading and editing raw lump data for any lump. Additionally contains various helping methods and classes for easier data manipulation.
