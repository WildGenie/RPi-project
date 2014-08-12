#include "reg.h"
void Write_Tx_Fifo(int p,char x[]){
	char i;
	unsigned char data[FIX_PACKET_LENGTH+1];
	for(i=0;i<FIX_PACKET_LENGTH;i++) data[i+1]=x[i];
	data[0]=CMD_TX_FIFO_WRITE;
	wiringPiSPIDataRW(p,data,FIX_PACKET_LENGTH+1);
	CTS();
}
