#define FIFO_INFO 0x15
void Clear_Int_Flags(int p)
{
	unsigned char a[]={CMD_GET_INT_STATUS,0,0,0};
	wiringPiSPIDataRW(p,a,sizeof(a));
	CTS();
}

