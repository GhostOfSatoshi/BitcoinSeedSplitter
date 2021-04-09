# BitcoinSeedSplitter
Splitting your 12 or 24 words mnemonic to multiple fault tolerant split-mnemonics (Shares) using shamir secret sharing.
Different from Slip39 as here we are safeguarding the seed words, not the derived master key.

How it works:
You type in (be careful!!!) your 12 or 24 words long BIP39 seed/mnemonic.
Select how many splits you want (2-15) and how many will be needed to restore the original seed (1-14).
Select OPTIONAL password (this in NOT your BIP passpharse, this encrypts the seed itself).

Usage example:
You have a 12 words seed which you want to store safely in 5 places with fault tolerancy. 3 of the 5 shares will be enough to rebuild the original seed.
(plust the optional password)

Orignal Mnemonic:
venture whale soap pave enjoy bid skull journey exotic soon phone proof

Output Shares:
1. stage middle dune innocent acid chimney clog focus metal nut flat tissue era female advice senior
2. stage era draw run glue brass cruel token produce sort wide tragic real tray wagon exit
3. stage slush economy focus oak vote box cruel license belt slow shoot sock session elder panda
4. stage clump donor major grape glad network quote sort above mad rule left verify such gate
5. stage proof earth genre music middle river guess topic swim rebel outer adult spend harvest rapid

Advantages:
You can loose 2 of the 5 and still be able to restore the original seed.
None of the share holders will have any knowledge of the seed nor any chance to restore it.

Be carefull not to set too low fault tolerancy level. For example: 9 of 10 works, but if you can only have 8 Shares you won't be able to restore any part of your seed.
