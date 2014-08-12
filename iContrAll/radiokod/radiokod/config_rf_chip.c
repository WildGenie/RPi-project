void CTS(void);

void config_rf_chip(int p){

//	unsigned char config[30][17];
	int i,j,k;
	char len;
	unsigned char data[16];
	unsigned char ARRAY[]=RADIO_CONFIGURATION_DATA_ARRAY;
	k=0;
	len=1;
	for(i=0;len;i++)
	{
		len=ARRAY[i+k];
		if(len)
			for(j=1;j<(int)len+1;j++)
				data[j-1]=ARRAY[i+j+k];
		if(len)
			wiringPiSPIDataRW(p,data,len);
		k+=len;
	}

/*	for(i=0;i!=30;i++)//A config egy dimenzios tombbol 2 dimenziosat csinal. Minden sor 0. eleme a sor hossza.
	{
		len=ARRAY[i+k];
		config[i][0]=len;
		for(j=1;j!=(int)len+1;j++)
			config[i][j]=ARRAY[i+j+k];
		k+=len;
	}

	for(i=0;i!=30;i++)//A config bite-ok lekuldese SPI-on
	{
		len=config[i][0];
		for(j=1;j!=(int)len+1;j++)
			data[j-1]=config[i][j];
		wiringPiSPIDataRW(p,data,len);
		CTS();
	}
*/
}

