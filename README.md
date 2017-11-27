# bandi-crop
Crop status analysis module for BandiPurri Smart Farm

# Description
Crop status analysis module for BandiPurri Smart Farm packaged as MATLAB assembly for C# .NET project integration.

# Installation and Usage
1. Run installer file in \for_redistribution folder. This will install the required MATLAB runtime if not detected and put application DLL in C:\Program Files\NareTrends\ folder

2. Create references in Visual Studio Project for the installed library and MATLAB's MWArray library:

C:\Program Files\NareTrends\BandiCropAnalyzer\application\BandiCropAnalyzer.dll <br />
C:\Program Files\MATLAB\MATLAB Runtime\v##\toolbox\dotnetbuilder\bin\win64\version\MWArray.dll 

3. Define import headers in C# code:

using MathWorks.MATLAB.NET.Arrays; <br />
using BandiCropAnalyzer;

# Sample code
Sample C# console application provided in BandiCropAnalyzer.cs
