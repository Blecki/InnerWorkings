
PSH N. 0X07		;Push argument to stack

LLT. $FIB. CAL	;Call function

ADS N. 0X01 	;Clean up stack

STP

:FIB 			;Function start
	RSP				;Fetch argument into A
	CAD N. 0X02 	
	LOD A

	MTA B N. 0X02 	;If A < 2, return A
	LLT. $END-FIB
	BLT

	PSH A

	SUB A N. 0X01 	;Call FIB A - 1
	PSH A
	LLT. $FIB. CAL
	ADS N. 0X01

	MFA A C ;Swap stored value on stack with A
	POP A
	PSH C

	SUB A N. 0X02 	;Call FIB A - 2
	PSH A
	LLT. $FIB. CAL
	ADS N. 0X01 

	POP B 			;Add values
	ADD A B

:END-FIB
RET 			;Return A