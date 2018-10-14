# EU4ToVic2
A WIP C# console program that converts EU4 save games to Vic2 HoD mods.

Designed to compliment my [Ck2ToEu4](https://github.com/Dinglydell/CK2ToEu4) converter for the sake of grand campaigns. Notable features include:
* Dynamic start date
* Countries are civilised if all instutions are embraced, uncivillised if one is unembraced and are deleted (only exist as natives like most of africa in vanilla vic2) if more than 1 institution is unembraced
* A lot of potential for customisation - most things the converter decides about your vic2 country are based on text files that determine what state of your EU4 country creates what effect in your VIC2 country
* Dynamically creates formable/releasable nations for cultures that don't have a nation
* Dynamic political parties
