#include "reg.h"

void Frequency(int p,unsigned long f){
	//char s[]={0x34,3};
	//wiringPiSPIDataRW(p,s,sizeof(s));
	//CTS();
	//usleep(D);
	//printf("ready ");
	char a[8];
	float k = 1.3333333333333333333333333333333e-7;
	char fint;
	long frac;
	f+=625;
	k*=(float)f;
	fint=(char)k-1;
	frac=(long)((k-fint)*524288);
	a[0]=0x11;
	a[1]=0x40;
	a[2]=0x04;
	a[3]=0x00;
	a[4]=fint;
	a[5]=(frac&0x00ff0000)>>16;
	a[6]=(frac&0x0000ff00)>>8;
	a[7]=frac&0x000000ff;
	wiringPiSPIDataRW(p,a,sizeof(a));
	CTS();
	//usleep(D);
}
