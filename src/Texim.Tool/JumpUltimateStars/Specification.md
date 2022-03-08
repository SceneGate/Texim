# Jump Ultimate Start images

## DSTX

| Offset | Type    | Description        |
| ------ | ------- | ------------------ |
| 0x00   | char[4] | DSTX               |
| 0x04   | byte    | Unknown            |
| 0x05   | byte    | Type, must be 0x04 |
| 0x06   | short   | Number of elements |
| 0x08   | short   | DSIG offset        |
| 0x0C   | uint[]  | Sprite data        |
| ...    | DSIG    | Image with palette |

The sprite data is 4 bytes:

1. byte: Width in tiles (48 pixels)
2. byte: Height in tiles (48 pixels)
3. short: Tile index. Only if it's 0, use 1. Tile 0 is transparent tile.

## DSIG

| Offset | Type     | Description                                        |
| ------ | -------- | -------------------------------------------------- |
| 0x00   | char[4]  | DSIG                                               |
| 0x04   | byte     | Unknown                                            |
| 0x05   | byte     | nds image format. Different than 0x10, then 8 bpp. |
| 0x06   | short    | number of palettes                                 |
| 0x08   | short    | width                                              |
| 0x0A   | short    | height                                             |
| 0x0C   | bgr555[] | palette                                            |
| ...    | pixels[] | image                                              |

## ALAR

| Offset | Type       | Description                 |
| ------ | ---------- | --------------------------- |
| 0x00   | char[4]    | ALAR                        |
| 0x04   | byte       | Version (3 to follow)       |
| 0x05   | byte       | Minor version?              |
| 0x06   | int        | Number of files             |
| 0x0A   | short      | Reserved?                   |
| 0x0C   | int        | Number of entries           |
| 0x10   | short      | Data offset                 |
| 0x12   | short[]    | File info absolute pointers |
| ..     | FileInfo[] | File info list              |
| ..     | Stream[]   | File data                   |

### File info

| Offset | Type   | Description               |
| ------ | ------ | ------------------------- |
| 0x00   | short  | ID                        |
| 0x02   | short  | Unknown                   |
| 0x04   | int    | Absolute pointer          |
| 0x08   | int    | Size                      |
| 0x0C   | short  | Unknown                   |
| 0x0E   | short  | Unknown                   |
| 0x10   | short  | Unknown                   |
| 0x12   | string | Null-terminated file path |

## Koma

| Offset | Type          | Description |
| ------ | ------------- | ----------- |
| 0x00   | KomaElement[] | Entries     |

### Koma element

| Offset | Type  | Description          |
| ------ | ----- | -------------------- |
| 0x00   | short | Image ID             |
| 0x02   | short | Unknown              |
| 0x04   | byte  | Name table index     |
| 0x05   | byte  | Name number          |
| 0x06   | byte  | Unknown              |
| 0x07   | byte  | Unknown              |
| 0x08   | byte  | KShape group index   |
| 0x09   | byte  | KShape element index |
| 0x0A   | byte  | Unknown              |
| 0x0B   | byte  | Unknown              |

The name is the combination of the prefix from the table and the number:
`$"{table[index]}_{num:D2}"`

### Koma name table

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
