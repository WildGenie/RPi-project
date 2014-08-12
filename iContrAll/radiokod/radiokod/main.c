#include <stdio.h>
#include <stdlib.h>
#include <wiringPi.h>
#include "reg.h"
#include "radio_config_Si4464.h"
#include "Si446x_B0_defs.h"
#include "Setup_Si4464_625_625.h"
#include "config_rf_chip.c"
#include "RX_Command.c"
#include "TX_Command.c"
#include "Write_Tx_Fifo.c"
#include "Read_Rx_Fifo.c"
#include "Clear_Int_Flags.c"
#include "Read_Int_Status.c"
#include "kbhit.c"

char cont_tx=0;
int i;
char state=0;
unsigned char data[FIX_PACKET_LENGTH];

void CTS(void);
void power_up(void);
void Interrupt0(void);

void Interrupt0(void)
{
	if(state=='r')
	{
		fprintf(stdout,"packet received\n");
		Read_Rx_Fifo(P,data);
		Clear_Int_Flags(P);
		RX_Command(P,&state);
		for(i=0;i<FIX_PACKET_LENGTH;i++) fprintf(stdout,"%c",data[i]);
		for(i=0;i<FIX_PACKET_LENGTH;i++) fprintf(stdout,"%2.0X ",data[i]);
		fprintf(stdout,"\n");
		//printf("%X", readrssi(P));
	}

	if(state=='t')
	{
		fprintf(stdout,"packet sent\n");
		digitalWrite(TXRX,0);
		Clear_Int_Flags(P);
		if(cont_tx)
		{
			Write_Tx_Fifo(P,data);
			TX_Command(P,&state);
		}
		else RX_Command(P,&state);
	}
}

int main()
{
	if(wiringPiSetup()<0)
	{
		fprintf(stdout,"unable to set wiring pi\n");
		exit(-1);
	}
	pinMode(TXRX,OUTPUT);
	wiringPiSPISetup(P,5000000);
	pinMode(PWDN,OUTPUT);
	for(i=0;i<4;i++)
	{
		digitalWrite(PWDN,1);
		usleep(DD);
		digitalWrite(PWDN,0);
		usleep(DD);
	}
 	if(wiringPiISR(INT,INT_EDGE_FALLING,&Interrupt0)<0)
	{
		fprintf(stdout,"unable to set interrupt pin\n");
		exit(-1);
	}
	power_up();
	fprintf(stdout,"power up\n");
 	config_rf_chip(P);
	fprintf(stdout,"config rf chip\n");
	Clear_Int_Flags(P);
	RX_Command(P,&state);
	char key=0;
//	char txflag=0;
	char datacnt=0;

	if(cont_tx)
	{
		CTS();
		for(i=0;i<64;i++) data[i]='a'+(i%26);
		for(i=0;i<64;i++) printf("%c",data[i]);
		Write_Tx_Fifo(P,data);
		Clear_Int_Flags(P);
		TX_Command(P,&state);
		usleep(DD);
	}

	do
	{
		if(kbhit())
		{
			key=getchar();
			data[datacnt]=key;
			datacnt++;
		}
		if(key==10||datacnt>62)
		{
//			txflag=0;
			key=0;
			for(;datacnt<62;datacnt++) data[datacnt]=0x08;
			data[62]=10;
			data[63]=13;
			datacnt=0;
			Write_Tx_Fifo(P,data);
			Clear_Int_Flags(P);
			TX_Command(P,&state);
		}
		usleep(DD);
	}while(key!='q');
	fprintf(stdout,"\nprogram terminated\n");
	return 0 ;
}

void CTS(void)
{
	unsigned char a=0;
	while(a!=255)
	{
		unsigned char x[]={CMD_CTS_READ,0};
		wiringPiSPIDataRW(P,x,sizeof(x));
		a=x[1];
	}
}

void power_up(void)
{
	digitalWrite(PWDN,1);
	usleep(D);
	digitalWrite(PWDN,0);
	usleep(D);
	unsigned char pwr[3]={CMD_POWER_UP,1,0};
	wiringPiSPIDataRW(P,pwr,3);
	CTS();
	pwr[0]=CMD_GET_INT_STATUS;
	wiringPiSPIDataRW(P,pwr,1);
	CTS();
}
