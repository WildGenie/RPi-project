using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RadioWithWiringPi
{
	class Program
	{
        const byte CMD_RX_FIFO_READ = 0x77;
        const byte CMD_CTS_READ = 0x44;
        const byte FIFO_INFO = 0x15;
        const byte CMD_GET_INT_STATUS = 0x20;
        const byte CMD_START_RX = 0x32;
        const byte CMD_POWER_UP = 0x02; 

		const int D = 10;
		const int DD = 100;
		const int FIX_PACKET_LENGTH = 64;
		const int P = 0;
		const int TXRX = 1;
		const int PWDN = 4;
		const int INT = 6;


		static char state = '0';

		static void Main(string[] args)
		{
			if (Init.WiringPiSetup()<0)
			{
				Console.WriteLine("unable to set wiring pi\n");
				return;
			}

			GPIO.pinMode(1, (int)GPIO.GPIOpinmode.Output);
			SPI.wiringPiSPISetup(0, 5000000);
			GPIO.pinMode(PWDN, (int)GPIO.GPIOpinmode.Output);

			for (int i = 0; i < 4; i++)
			{
				GPIO.digitalWrite(PWDN, 1);
				Thread.Sleep(DD);
				GPIO.digitalWrite(PWDN, 0);
				Thread.Sleep(DD);
			}

			if (PiThreadInterrupts.wiringPiISR(INT, (int)PiThreadInterrupts.InterruptLevels.INT_EDGE_FALLING, Interrupt0) < 0)
			{
				Console.WriteLine("unable to set interrupt pin\n");
				return;
			}

            power_up();
            Console.WriteLine("power up");
            
            config_rf_chip(P);


		}

        unsafe static void CTS()
        {
	        byte a=0;
	        while(a!=255)
	        {
		        byte[] x = new byte [] {CMD_CTS_READ,0};
		        fixed(byte* pX = x)
                {
                    SPI.wiringPiSPIDataRW(0, pX, x.Length);
                }
		        a=x[1];
	        }
        }

        static byte[] data = new byte[FIX_PACKET_LENGTH];

		unsafe static void Interrupt0()
		{
			if(state=='r')
			{
				Console.WriteLine("packet received\n");
				Read_Rx_Fifo(P,data);
				Clear_Int_Flags(P);
				RX_Command(P, ref state);

                for (int i = 0; i < FIX_PACKET_LENGTH; i++)
                {
                    Console.Write(data[i].ToString());
                }

				Console.WriteLine("\n");
			}

			if(state=='t')
			{
				Console.WriteLine("packet sent\n");
				GPIO.digitalWrite(TXRX,0);
				Clear_Int_Flags(P);
				RX_Command(P,ref state);
			}
		}

        unsafe static void Read_Rx_Fifo(int p, byte[] x)
        {
	        byte[] data=new byte[FIX_PACKET_LENGTH+1];
	        data[0]=CMD_RX_FIFO_READ;

            for (int i = 0; i < FIX_PACKET_LENGTH; i++)
			{
			    data[i+1]=0;
			}
            
            
	            fixed(byte* pData = data)
                {
                    SPI.wiringPiSPIDataRW(p, pData ,FIX_PACKET_LENGTH+1);
                }
            
    
	        for (int i = 0; i < FIX_PACKET_LENGTH; i++)
			{
			    x[i]=data[i+1];
            }
	
	        CTS();
	        byte[] t= new byte[] {0x15,0x03};

            
                fixed (byte* pT = t)
                {
                    SPI.wiringPiSPIDataRW(p, pT, 2);
                }
            
	        
	        CTS();
        }
        
        unsafe static void Clear_Int_Flags(int p)
        {
	        byte[] a = new byte[] {CMD_GET_INT_STATUS,0,0,0};
            fixed (byte* pA = a)
            {
                SPI.wiringPiSPIDataRW(p, pA, a.Length);
            }
	        CTS();
        }

        unsafe static void RX_Command(int p, ref char x)
        {
	        x='r';
	        byte[] d = new byte[] {CMD_START_RX,0,0,0,FIX_PACKET_LENGTH,0,0,0};
            fixed (byte* pD = d)
            {
                SPI.wiringPiSPIDataRW(p, pD, d.Length);
            }
	        CTS();
        }

        unsafe static void power_up()
        {
	        GPIO.digitalWrite(PWDN,1);
	        Thread.Sleep(D);
	        GPIO.digitalWrite(PWDN,0);
	        Thread.Sleep(D);
	        byte[] pwr = new byte[3] {CMD_POWER_UP,1,0};
            fixed (byte* pPwr = pwr)
            {
                SPI.wiringPiSPIDataRW(P, pPwr, 3);
            }
	        CTS();
	        pwr[0]=CMD_GET_INT_STATUS;
            fixed (byte* pPwr = pwr)
            {
                SPI.wiringPiSPIDataRW(P, pPwr, 3);
            }
	        CTS();
        }

        unsafe void config_rf_chip(int p)
        {

            //	unsigned char config[30][17];
            int i, j, k;
            byte len;
            byte[] data = new byte[16];
            byte[] ARRAY = RADIO_CONFIGURATION_DATA_ARRAY;
            k = 0;
            len = 1;
            for (i = 0; len > 0; i++)
            {
                len = ARRAY[i + k];
                // ????????????????????????????
                if (len > 0)
                    for (j = 1; j < (int)len + 1; j++)
                        data[j - 1] = ARRAY[i + j + k];
                if (len > 0)
                {
                    fixed (byte* pData = data)
                    {
                        SPI.wiringPiSPIDataRW(p, pData, len);
                    }
                }
                k += len;
            }
        }
	}
}
