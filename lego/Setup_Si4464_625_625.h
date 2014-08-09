/**************************************************************
*** Configuration File generated automatically by           ***
*** SILICON LABS's Wireless Development Suite               ***
*** WDS GUI Version: 3.1.11.0                                ***
*** Device: Si4464 Rev.: B1                                 ***
***                                                         ***
***                                                         ***
*** For further details please consult with the device      ***
*** datasheet, available at <http://www.silabs.com>         ***
***************************************************************/

/* 
//IMPORTANT USAGE GUIDE
//If you intend to use this definition:
#define Si4460_DEFINED_VALUES

//then use it this way:
U8 commandArray[] = {Si4460_DEFINED_VALUES}; 

// U8 is a macro for 'byte' value at different compilers
// defined in the compiler_defs.h
*/
#define EZRADIOPRO2_SI4464_B1

#define XO_TUNE 0x11, 0x00, 0x01, 0x00, 0x52

#define PREAMBLE_CONFIG_STD_1 0x11, 0x10, 0x01, 0x01, 0x14
#define PREAMBLE_CONFIG 0x11, 0x10, 0x01, 0x04, 0x01
#define SYNC_CONFIG 0x11, 0x11, 0x01, 0x00, 0x01
#define SYNC_BITS_31_24 0x11, 0x11, 0x01, 0x01, 0x2D
#define SYNC_BITS_23_16 0x11, 0x11, 0x01, 0x02, 0xD4

/* Input file content:
%%	Crys_freq(Hz)	Crys_tol(ppm)	IF_mode	High_perf_Ch_Fil	OSRtune	Ch_Fil_Bw_AFC	ANT_DIV	PM_pattern
 	30000000	0	2	1	0	0	0	0
%%	MOD_type	Rsymb(sps)	Fdev(Hz)	RXBW(Hz)	Mancheste	AFC_en	Rsymb_error	Chip-Version
 	3	625	625	200000	0	0	0.0	3
%%	RF Freq.(MHz)	API_TC	fhst	inputBW	BERT	RAW_dout	D_source	Hi_pfm_div
 	437.345	28	0	0	0	0	0	1
*/

#define CENTER_FREQ 437345000
#define XTAL_FREQ 30000000
#define CHANNEL_SPACING 250000
#define CHANNEL_NUMBER 0
#define XTAL_CAP_BANK 0x52
#define MODULATION_TYPE "2GFSK"
#define MODULATION_TYPE_VALUE 3
#define MANCHESTER_CODE "Off"
#define MANCHESTER_CODE_VALUE 0
#define DEVIATION 625
#define DATA_RATE 625
#define ANTENNA_DIVERSITY "Disabled"
#define ANTENNA_DIVERSITY_VALUE 0


// # WB filter 9 (BW =   2.05 kHz);  NB-filter 9 (BW = 2.05 kHz) 

// Modulation index: 2

// Calculator svn revision 6342

// EZRadio PRO2 modem calculator output header file  modem_params.h

// RF frequency is 437.35 MHz
// Mod type: 2GFSK,	DataRate: 0.63 ksps;  FreqDev: 0.63 kHz;  RX BW: 2.00 kHz 

// all for TX in this section:
// DataRate: 3 API bytes;  NCOMODulus: 4 bytes; FreqDev: 3 bytes, for TX 
#define MODEM_DATA_RATE_2_14 0x11, 0x20, 0x0A, 0x03,   0x00, 0x09, 0xC4, 0x04, 0x2D, 0xC6, 0xC0, 0x00, 0x00, 0x2C
#define MODEM_TX_RAMP_DELAY_5 0x11, 0x20, 0x01, 0x18,   0x01
// PA ramp time control: 1 byte
#define PA_TC_5  0x11, 0x22, 0x01, 0x03, 0x1C


// all for general parameters in both TRX 
#define MODEM_MOD_TYPE_7 0x11, 0x20, 0x03, 0x00,  0x03, 0x00, 0x07
#define MODEM_CLKGEN_BAND_5 0x11, 0x20, 0x01, 0x51,   0x0A
// SYNTH CONTROL GROUP: 7 API bytes, for both TRX 
#define SYNTH_PFDCP_CPFF_11 0x11, 0x23, 0x07, 0x00, 0x2C, 0x0E, 0x0B, 0x04, 0x0C, 0x73, 0x03
// FREQ CONTROL GROUP: 8 API bytes,  TRX
#define FREQ_CONTROL_INTE_12 0x11, 0x40, 0x08, 0x00,  0x39, 0x0A, 0x80, 0x57, 0x00, 0x00, 0x20,  0xFE
//#define FREQ_CONTROL_INTE_12 0x11, 0x40, 0x04, 0x00,  0x39, 0x0A, 0x80, 0x57


// all for RX below:
// MDM_CTRL: 1 byte,  IF_CONTROL: 1B,  IF_FREQ: 3B,  DECIMATION_CFG: 2B,  for all RX 
#define MODEM_MDM_CTRL_11 0x11, 0x20, 0x07, 0x19,   0x80, 0x08, 0x03, 0x80, 0x00, 0xF0, 0x11
// BCR: 10 bytes,  for all RX 
#define MODEM_BCR_OSR_1_14 0x11, 0x20, 0x0A, 0x22,  0x00, 0xFA, 0x02, 0x0C, 0x4A, 0x01, 0x06, 0x02, 0xC2, 0x00
// AFC: 8 bytes,  for (G)FSK RX 
#define MODEM_AFC_GEAR_12 0x11, 0x20, 0x08, 0x2C,  0x04, 0x36, 0x80, 0x02, 0x05, 0xD3, 0x40, 0x00
//#define MODEM_AFC_GEAR_12 0x11, 0x20, 0x08, 0x2C,  0x04, 0x36, 0x80, 0x02, 0x05, 0xD3, 0x80, 0x00

// AGC: 4 bytes,  for all RX 
#define MODEM_AGC_CONTRL_5 0x11, 0x20, 0x01, 0x35,   0xE2
#define MODEM_AGC_WINDOW_SIZE_7 0x11, 0x20, 0x03, 0x38,  0x11, 0x37, 0x37

// 4FSK: 5 bytes,  for 4(G)FSK RX 
#define MODEM_FSK4_GAIN1_9 0x11, 0x20, 0x05, 0x3B,  0x00, 0x1A, 0x80, 0x00, 0x00
// OOK: 4 bytes, for RX 
#define MODEM_OOK_PDTC_8 0x11, 0x20, 0x04, 0x40,  0x29, 0x0C, 0xA4, 0x02
// raw: 4 bytes, for RX 
#define MODEM_RAW_SEARCH_8 0x11, 0x20, 0x04, 0x44,  0xD6, 0x83, 0x00, 0xD8
// AntDiv: 2 bytes, for RX 
#define MODEM_ANT_DIV_MODE_6 0x11, 0x20, 0x02, 0x48,  0x01, 0x80
// RSSI_comp: 1 bytes, for RX 
#define MODEM_RSSI_COMP_5  0x11, 0x20, 0x01, 0x4E, 0x3A

// RX chfil coeff:  WB filter k1=9 (BW= 2.0 kHz), NB filter k2=9 (BW= 2.0 kHz)
// MODEM_CHFLT_RX1 GROUP: 18 API bytes,  for all RX 
#define MODEM_CHFLT_RX1_CHFLT_COE13_7_0_13 0x11, 0x21, 0x09, 0x00,  0x0C, 0x01, 0xE4, 0xB9, 0x86, 0x55, 0x2B, 0x0B, 0xF8
#define MODEM_CHFLT_RX1_CHFLT_COE4_7_0_13  0x11, 0x21, 0x09, 0x09,  0xEF, 0xEF, 0xF2, 0xF8, 0xFC, 0x05, 0x00, 0xFF, 0x0F
// MODEM_CHFLT_RX2 GROUP: 18 API bytes, 
#define MODEM_CHFLT_RX2_CHFLT_COE13_7_0_13 0x11, 0x21, 0x09, 0x12,  0x0C, 0x01, 0xE4, 0xB9, 0x86, 0x55, 0x2B, 0x0B, 0xF8
#define MODEM_CHFLT_RX2_CHFLT_COE4_7_0_13  0x11, 0x21, 0x09, 0x1B,  0xEF, 0xEF, 0xF2, 0xF8, 0xFC, 0x05, 0x00, 0xFF, 0x0F


#define PA_MODE 0x11, 0x22, 0x01, 0x00, 0x08

#define PA_PWR_LVL 0x11, 0x22, 0x01, 0x01, 0x69
//#define PA_PWR_LVL 0x11, 0x22, 0x01, 0x01, 0x7F E
//#define PA_PWR_LVL 0x11, 0x22, 0x01, 0x01, 0x24 home
#define PA_BIAS_CLKDUTY 0x11, 0x22, 0x01, 0x02, 0x00 
