/***************************************************************************

	File :	SM2API.H
	By :	ACC

	(C) COPYRIGHT 1998 INTELLUTION INC.
	ALL RIGHTS RESERVED


!Overview:
	C callable 'backdoor' functions to access SM2 data
	index can be 0-19999 for either analog or digital
	analog values are FLOATs
	digital values (via this api) are 16 bit groups.
	analog and digital data are independent.

	link sm2api.lib to your C program to get to the functions below

!ngoff
*+
Modification Log:
	Vers	  Date	 By	Notes
	----	-------- ---	-----
		07/01/98 acc
		08/28/2001  acc   add Get/SetAnalogAlarm, Get/SetDigitalAlarm
		10/19/2001 acc add text data & exception support
		03/21/2007 mfr add double support GetDouble and SetDouble

	
*-
****************************************************************************/
#include <datatype.h>

UINT16	GetAnalog(UINT16 index, FLOAT *data);
UINT16	SetAnalog(UINT16 index, FLOAT data);
UINT16	SetAnalogAlarm(UINT16 index, INT16 alm);
UINT16	GetAnalogAlarm(UINT16 index, INT16 *alm);
UINT16	GetDigital(UINT16 index, UINT16 *data);
UINT16	SetDigital(UINT16 index, UINT16 data);
UINT16	SetDigitalEx(UINT16 index, UINT16 data, UINT16 mask);
UINT16	SetDigitalAlarm(UINT16 index, INT16 alm);
UINT16	GetDigitalAlarm(UINT16 index, INT16 *alm);
UINT16	GetText(UINT16 index, char *data, int count);
UINT16	SetText(UINT16 index, char *data, int count);
UINT16	SetTextAlarm(UINT16 index, INT16 alm);
UINT16	GetTextAlarm(UINT16 index, INT16 *alm);

UINT16	GetCommError(UINT16 *data);
UINT16	SetCommError(UINT16 data);

UINT16	GetDouble(UINT16 index, double *data);
UINT16	SetDouble(UINT16 index, double data);
