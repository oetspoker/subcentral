@echo off

md tmp
ilmerge /out:tmp\SubCentral.dll SubCentral.dll NLog.dll


IF EXIST SubCentral_UNMERGED.dll del SubCentral_UNMERGED.dll
ren SubCentral.dll SubCentral_UNMERGED.dll
IF EXIST SubCentral_UNMERGED.pdb del SubCentral_UNMERGED.pdb
ren SubCentral.pdb SubCentral_UNMERGED.pdb

move tmp\*.* .
rd tmp

