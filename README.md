# BitcoinSeedSplitter
Splitting your 12 or 24 words mnemonic to  multiple equal and fault tolerant split-mnemonics (Shares) using shamir secret sharing.
Different from Slip39 as here we are safeguarding the seed words, not the derived master key. Problem is with Slip39: there is no way to go back to seed words from the derived master seed, hence it's not possible/easy to use it with any popular wallets.

<b>Explanation of the problem and solution:</b><br/>
We are talking about <b>equal</b> shares here. When you split your seed manually you end up with pieces, but it will matter which one you lost because your splits are NOT equal. They are not shares but actually just pieces of the original seed (also making brute force theoretically/future possible).
Example of easy but sub-optimal manual split:<br/>
1-2-3-4 (4x3 words=12 words seed)
1-2 <br/>
2-3 <br/>
3-4 <br/>
If you loose ANY 1(!) of these you are done. <br/>
If you add a fourth it's better:<br/>
1-4 <br/>
Now you can loose any ONE, but two right one is enough to  reconstruct the whole seed just as earlier. One is enough to guess "only" 6 words which maybe feasable in the future.<br/>
Using shares you can use a 3 of 4. Where you can loose ANY 1 but 3 would be needed to reconstruct the seed. Knowing one share would give you zero information hence brute for is impoosbile.<br/>
You can go way up, like using 6 of 10 which gives you very high fault tolerance with low risk of seed-rebuild. Even knowing 5 of the 10 will not make brute force possible.<br/>

<b>How this implemantation works:</b><br/>
You type in  your 12 or 24 words long BIP39 seed/mnemonic <b>(be careful on what device!!!)</b><br/>
Select how many splits you want (2-15) and how many will be needed to restore the original seed (1-14).<br/>
Select OPTIONAL password (this in NOT your BIP passpharse, this encrypts the seed itself).<br/>

<br/>
<b>Example:</b><br/>
You have a 12 words seed which you want to store safely in 5 places with fault tolerancy. 3 of the 5 shares will be enough to rebuild the original seed.
(plus the optional password)
<br/>
Orignal Mnemonic:<br/>
venture whale soap pave enjoy bid skull journey exotic soon phone proof
<br/>
Output Shares:<br/>
1. stage middle dune innocent acid chimney clog focus metal nut flat tissue era female advice senior<br/>
2. stage era draw run glue brass cruel token produce sort wide tragic real tray wagon exit<br/>
3. stage slush economy focus oak vote box cruel license belt slow shoot sock session elder panda<br/>
4. stage clump donor major grape glad network quote sort above mad rule left verify such gate<br/>
5. stage proof earth genre music middle river guess topic swim rebel outer adult spend harvest rapid<br/>
<br/>
<b>Advantages:</b><br/>
You can loose ANY 2 of the 5 and still be able to restore the original seed.<br/>
None of the share holders will have any knowledge of the seed nor any chance to restore it (eg. not weakening the seed)<br/>

<br/>
<b>Windows release available:</b>
https://github.com/GhostOfSatoshi/BitcoinSeedSplitter/releases
<br/><br/>

<b>*****Be carefull***** not to set too low fault tolerancy level. For example: 9 of 10 works, but if you can only have 8 of the Shares you won't be able to restore any part of your seed. You lost it all!</b>

Multiple implementations and Linux exe would be a nice addition.
<br/><br/>
<b>Implementation details:</b><br/>
You start with the bits from the original seed (all 12 or 24 words x 11 bits)<br/>
If password present: Get SHA256 hash of the ASCII password 100K times and  XOR the seed with it<br/>
Do the Sahmier secret sharing<br/>
Translate all Shares to ShareMnemonic using the BIP39 wordlist<br/>
<br/>
<b>Share build-up:</b><br/>
11 bits: SplitID (to identify you use the right shares to reconstruct)<br/>
4 bits:  ShareID (ID of current share)<br/>
4 bits:  Threshold (how many shares are needed to reconstruct as Shamir merge actually merges any number of shares, but the results is junk of course)<br/>
8 bits:  Length of data<br/>
X bits:  Data<br/>
4-11 bits: CRC like in the original BIP39 seed  (length depends on how many bits are optimal to get full bytes) <br/>
