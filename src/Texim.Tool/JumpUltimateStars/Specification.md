# Jump Ultimate Start images

## DSTX

| Offset | Type        | Description |
| ------ | ----------- | ----------- |
| 0x00   | char[4]     | DSTX        |
| 0x04   |
| 0x05   | type        |
| 0x06   |
| 0x07   |
| 0x08   | DSIG offset |
| 0x10   | metadata    |

## DSIG

| Offset | Type               | Description |
| ------ | ------------------ | ----------- |
| 0x00   | char[4]            | DSIG        |
| 0x04   |
| 0x05   | nds image format   |
| 0x06   | number of palettes |
| 0x08   | width              |
| 0x0A   | height             |
| 0x0C   | bgr555[]           | palette     |
| ...    | pixels[]           | image       |

## Koma name table

| ID  | Name   |
| --- | ------ |
| 0   | _none_ |
| 1   | es     |
| 2   | jj     |
| 3   | op     |
| 4   | rb     |
| 5   | rk     |
| 6   | bl     |
| 7   | yo     |
| 8   | mr     |
| 9   | mo     |
| 10  | nn     |
| 11  | bb     |
| 12  | hk     |
| 13  | hs     |
| 14  | yh     |
| 15  | bc     |
| 16  | bu     |
| 17  | pj     |
| 18  | hh     |
| 19  | nk     |
| 20  | na     |
| 21  | db     |
| 22  | tl     |
| 23  | ds     |
| 24  | dn     |
| 25  | dg     |
| 26  | to     |
| 27  | tz     |
| 28  | ss     |
| 29  | sd     |
| 30  | dt     |
| 31  | tc     |
| 32  | sk     |
| 33  | nb     |
| 34  | oj     |
| 35  | cb     |
| 36  | kk     |
| 37  | kn     |
| 38  | gt     |
| 39  | ct     |
| 40  | tr     |
| 41  | ig     |
| 42  | is     |
