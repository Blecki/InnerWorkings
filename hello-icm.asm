; Set display mode High02
MTA B N. 0X01. MTA A N. 0X02. OUT
MTA B N. 0X01. MTA A N. 0X03. OUT

; Set palette position 1 to white
MTA B N. 0XFF. MTA A N. 0X02. OUT
MTA B N. 0XF1. MTA A N. 0X03. OUT

; Map memory to page 64.
MTA B N. 0X40. MTA A N. 0X02. OUT
MTA B N. 0X02. MTA A N. 0X03. OUT

; Write some pixels
;LLH. 0X40. 0X00
;STR N. 0XFF

MTA A N. 0X00

:LINE-LOOP-TOP

MTA B N. 0X80
LLH. $LINE-LOOP-END
BIE

PSH A
MFA A E
MFA A D
MTA A N. 0X40. MFA A C

LLH. $FUNC-PLOT-PIXEL
CAL
POP A
ADD A N. 0X01

JPL. $LINE-LOOP-TOP
:LINE-LOOP-END

;MTA A N. 0X10. MFA A D. MFA A E
;MTA A N. 0X40. MFA A C
;LLH. $FUNC-PLOT-PIXEL
;CAL

; Refresh display
MTA A N. 0X03. MTA B N. 0X03. OUT

STP


; Function plot pixel - Write a pixel at a specified x, y location
:FUNC-PLOT-PIXEL ;mem x y
; C: MEM
; D: X
; E: Y

LLH. 0X00. 0X00. MTA A C. MFA A H ;Load HL with the mem pointer

MTA A E. MUL A N. 0X10 ;Find offset of row into mem.
OVE B ;Offset may have overflowed.
CAD A ;Add offset to HL
MTA A H. ADD A B. MFA A H. ;Add overflow to H

MTA B N. 0X03. MTA A D. SSR ;Find offset of column
CAD A ;Add offset to HL

MTA B D. MOD B N. 0X08 ;Find offset of pixel in word
MTA A N. 0X80. SSR ;Generate pixel mask

LOD B ;Load pixel
BOR B A ;Fill pixel
STR B ;Store pixel

RET