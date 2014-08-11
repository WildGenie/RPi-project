#include "reg.h"
void Read_Rx_Fifo(int p,char x[]){
	char i;
	unsigned char data[FIX_PACKET_LENGTH+1];
	data[0]=CMD_RX_FIFO_READ;
	for(i=0;i<FIX_PACKET_LENGTH;i++) data[i+1]=0;
	wiringPiSPIDataRW(p,data,FIX_PACKET_LENGTH+1);
	for(i=0;i<FIX_PACKET_LENGTH;i++) x[i]=data[i+1];
	CTS();
	unsigned char t[]={0x15,0x03};
	wiringPiSPIDataRW(p,t,2);
	CTS();
}
