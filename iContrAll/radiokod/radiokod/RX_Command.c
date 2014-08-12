#include "reg.h"

void RX_Command(int p,char *x){
	*x='r';
	unsigned char d[]={CMD_START_RX,0,0,0,FIX_PACKET_LENGTH,0,0,0};
	wiringPiSPIDataRW(p,d,sizeof(d));
	CTS();
}
