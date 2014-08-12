unsigned char readrssi(int p){
	unsigned char t=0;
	unsigned char a[]={CMD_GET_MODEM_STATUS,0};
	wiringPiSPIDataRW(p,a,sizeof(a));
	usleep(10);
	unsigned char b[5]={CMD_CTS_READ,0,0,0,0};
	wiringPiSPIDataRW(p,b,sizeof(b));
	CTS();
	return b[4];
}
