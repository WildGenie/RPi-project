#include "reg.h"
void TX_Command(int p,char *x){
	*x='t';
	digitalWrite(TXRX,1);
	unsigned char d[]={CMD_START_TX,0,0,0,FIX_PACKET_LENGTH,0};
	wiringPiSPIDataRW(p,d,sizeof(d));
	CTS();
}
