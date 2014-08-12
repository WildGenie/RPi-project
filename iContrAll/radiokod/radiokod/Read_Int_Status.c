void Read_Int_Status(int p,char x[]){
	char i;
	unsigned char data[10];
	for(i=0;i<9;i++) data[i+1]=0;
	data[0]=CMD_GET_INT_STATUS;
	wiringPiSPIDataRW(p,x,10);
	for(i=0;i<9;i++) x[i]=data[i+1];
	CTS();
}

