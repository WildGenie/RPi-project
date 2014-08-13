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
        const byte CMD_TX_FIFO_WRITE = 0x66;
        const byte CMD_START_TX = 0x31;

		const int D = 10;
		const int DD = 100;
		const int FIX_PACKET_LENGTH = 64;
		const int P = 0;
		const int TXRX = 1;
		const int PWDN = 4;
		const int INT = 6;
        
        static byte[] RADIO_CONFIGURATION_DATA_ARRAY = new byte[] {
            0x07, 0x02, 0x01, 0x00, 0x01, 0xC9, 0xC3, 0x80,
            0x08, 0x13, 0x21, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00,
            0x05, 0x11, 0x00, 0x01, 0x00, 0x45,
            0x05, 0x11, 0x00, 0x01, 0x03, 0x60,
            0x06, 0x11, 0x01, 0x02, 0x00, 0x01, 0x30,
            0x08, 0x11, 0x02, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x0D, 0x11, 0x10, 0x09, 0x00, 0x08, 0x14, 0x00, 0x0F, 0x31, 0x00, 0x00, 0x00, 0x00,
            0x09, 0x11, 0x11, 0x05, 0x00, 0x01, 0xB4, 0x2B, 0x00, 0x00,
            0x05, 0x11, 0x12, 0x01, 0x00, 0x80,
            0x05, 0x11, 0x12, 0x01, 0x06, 0x02,
            0x10, 0x11, 0x12, 0x0C, 0x08, 0x00, 0x00, 0x00, 0x30, 0x30, 0x00, 0x07, 0x04, 0x00, 0x00, 0x00, 0x00,
            0x10, 0x11, 0x12, 0x0C, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x05, 0x11, 0x12, 0x01, 0x20, 0x00,
            0x10, 0x11, 0x20, 0x0C, 0x00, 0x03, 0x00, 0x07, 0x1E, 0x84, 0x80, 0x09, 0xC9, 0xC3, 0x80, 0x00, 0x06,
            0x05, 0x11, 0x20, 0x01, 0x0C, 0xD4,
            0x0C, 0x11, 0x20, 0x08, 0x18, 0x01, 0x00, 0x08, 0x03, 0x80, 0x00, 0x10, 0x20,
            0x0D, 0x11, 0x20, 0x09, 0x22, 0x00, 0x4B, 0x06, 0xD3, 0xA0, 0x07, 0xFF, 0x02, 0x00,
            0x0B, 0x11, 0x20, 0x07, 0x2C, 0x00, 0x23, 0x86, 0xD4, 0x00, 0xA9, 0xE0,
            0x05, 0x11, 0x20, 0x01, 0x35, 0xE2,
            0x0D, 0x11, 0x20, 0x09, 0x38, 0x11, 0x10, 0x10, 0x00, 0x1A, 0x20, 0x00, 0x00, 0x28,
            0x0F, 0x11, 0x20, 0x0B, 0x42, 0xA4, 0x03, 0xD6, 0x03, 0x00, 0x80, 0x01, 0x80, 0xFF, 0x0C, 0x02,
            0x05, 0x11, 0x20, 0x01, 0x4E, 0x40,
            0x05, 0x11, 0x20, 0x01, 0x51, 0x0A,
            0x10, 0x11, 0x21, 0x0C, 0x00, 0xA2, 0x81, 0x26, 0xAF, 0x3F, 0xEE, 0xC8, 0xC7, 0xDB, 0xF2, 0x02, 0x08,
            0x10, 0x11, 0x21, 0x0C, 0x0C, 0x07, 0x03, 0x15, 0xFC, 0x0F, 0x00, 0xA2, 0x81, 0x26, 0xAF, 0x3F, 0xEE,
            0x10, 0x11, 0x21, 0x0C, 0x18, 0xC8, 0xC7, 0xDB, 0xF2, 0x02, 0x08, 0x07, 0x03, 0x15, 0xFC, 0x0F, 0x00,
            0x08, 0x11, 0x22, 0x04, 0x00, 0x08, 0x7F, 0x00, 0x3D,
            0x0B, 0x11, 0x23, 0x07, 0x00, 0x34, 0x04, 0x0B, 0x04, 0x07, 0x70, 0x03,
            0x10, 0x11, 0x30, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x0C, 0x11, 0x40, 0x08, 0x00, 0x38, 0x0A, 0xAA, 0xAA, 0x44, 0x44, 0x20, 0xFE,
            0x00
        };

		static char state = '0';
        static byte[] data = new byte[FIX_PACKET_LENGTH];

        unsafe static void Interrupt0()
        {
            if (state == 'r')
            {
                Console.WriteLine("packet received\n");
                Read_Rx_Fifo(P, data);
                Clear_Int_Flags(P);
                RX_Command(P, ref state);

                string s = Encoding.UTF8.GetString(data);

                //for (int i = 0; i < FIX_PACKET_LENGTH; i++)
                //{

                //    Console.Write(data[i].ToString());
                //}

                Console.WriteLine(s);
            }

            if (state == 't')
            {
                Console.WriteLine("packet sent\n");
                GPIO.digitalWrite(TXRX, 0);
                Clear_Int_Flags(P);
                RX_Command(P, ref state);
            }
        }

		static void Main(string[] args)
		{
			if (Init.WiringPiSetup()<0)
			{
				Console.WriteLine("unable to set wiring pi\n");
				return;
			}

			GPIO.pinMode(TXRX, (int)GPIO.GPIOpinmode.Output);
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
            Console.WriteLine("config rf chip");

            Clear_Int_Flags(P);
            RX_Command(P, ref state);
            
            Console.WriteLine(data.Length);

            while (Console.ReadKey().KeyChar!='Q')
            {
                //
                string sendMessage = "00000112LC10000101xxxx1xxxxxxx";
                byte testByte = 50;

                byte[] sendMessageBytes = Encoding.UTF8.GetBytes(sendMessage);

                Array.Copy(sendMessageBytes, 0, data, 0, sendMessageBytes.Length);
                // hozzáfüzzűk a szart
                data[sendMessageBytes.Length] = testByte;

                if (sendMessageBytes.Length > 62) continue;

                for (int i = data.Length; i < 62; i++)
                {
                    data[i] = 0x08;
                }

                data[62] = 10;
                data[63] = 13;

                Write_Tx_Fifo(P, data);
                Clear_Int_Flags(P);
                TX_Command(P, ref state);

                Thread.Sleep(DD);
            }
            //while ((key = Console.ReadKey().KeyChar)!='q')
            //{
            //    data[datacnt++] = (byte)key;
            //    if (key == 10 || datacnt > 62)
            //    {
            //        //			txflag=0;
            //        key = (char)0;
            //        for (; datacnt < 62; datacnt++)
            //        {
            //            data[datacnt] = 0x08;
            //        }

            //        data[62] = 10;
            //        data[63] = 13;
            //        datacnt = 0;

            //        Write_Tx_Fifo(P, data);
            //        Clear_Int_Flags(P);
            //        TX_Command(P, ref state);
            //    }
            //    Thread.Sleep(DD);
            //}
            Console.WriteLine("program terminated");
            
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

        unsafe static void config_rf_chip(int p)
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

        unsafe static void Write_Tx_Fifo(int p, byte[] x)
        {
	        byte[] data = new byte[FIX_PACKET_LENGTH+1];
	        for(int i=0;i<FIX_PACKET_LENGTH;i++) data[i+1]=x[i];
	        data[0]=CMD_TX_FIFO_WRITE;
            fixed (byte* pData = data)
            {
	            SPI.wiringPiSPIDataRW(p,pData,FIX_PACKET_LENGTH+1);
            }
	        CTS();
        }

        unsafe static void TX_Command(int p,ref char x)
        {
	        x='t';
	        GPIO.digitalWrite(TXRX,1);
	        byte[] d = new byte[] {CMD_START_TX,0,0,0,FIX_PACKET_LENGTH,0};
            fixed (byte* pD = d)
            {
                SPI.wiringPiSPIDataRW(p, pD, d.Length);
            }
	        CTS();
        }
	}
}
