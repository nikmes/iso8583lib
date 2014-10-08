using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using LibTLV;
using ConvertionsHelper;
using LogingUtils;

namespace iso8583
{
    // Enum types used by iso8583 module

    public enum rType
    {
        INCOMMING,
        OUTGOING
    }

    public enum FieldType
    {
        A,       // Alpha, including blanks
        N,       // Numeric values only
        S,       // Special characters only
        AN,      // Alphanumeric
        AS,      // Alpha & Special Characters only
        NS,      // Numeric & Special Characters only
        ANS,     // Alphabetic, numberic and special characters
        B,       // Binary data
        Z,       // Track 2 and 3 code set as defined in ISO/IEC 7813 and ISO/IEC 4909 respectively
        NULL,    // No data type   
        BITMAP   // BITMAP
    }

    public enum LengthType
    {
        FIXED,   // Fixed Length field, no length indicator     
        LLVAR,   // variable length field, 1 bytes used for length indicator
        LLLVAR   // variable length field, 2 bytes used for length indicator
    }

    public enum EncodingType
    {
        ASCII,   // Value: 0800   30 38 30 30 [4 bytes to represent]
        BCD,     // Value: 0800 - 08 00       [2 bytes to represent]
        EBCDIC,  // Value: 
        HEX,     // 
        NULL,    // No encoding type
    }

    public enum Particiation
    {
        MANDATORY,  // Field is mandatory in a message
        OPTIONAL,   // Field is optional in a message
        DONTEXIST   // Field dose not exists at all in a message
    }
       
    public class isoHeader
    {
        byte[] headerData;  // holds the raw data of the header

        public isoHeader()
        {
            headerData = new byte[5];
        }

        public isoHeader(byte[] header)
        {
            // initialize header data with incomming byte array
            headerData = header;
        }

        public isoHeader(String header)
        {
            // sets the header data from HEX string
            headerData = ConvHelper.hex2bytes(header);
        }

        public void setHeader(String header)
        {
            // sets the header data from HEX string
            headerData = ConvHelper.hex2bytes(header);
        }

        public void setHeader(byte[] header)
        {
            // sets the header data from byte array
            headerData = header;
        }

        public byte[] getHeaderBYTE()
        {
            // returns the byte array with header data
            return headerData;
        }

        public String getHeaderHEX()
        {
            // returns the hexadecimal representation of the header
            return ConvHelper.bytes2hex(headerData, headerData.Length, 0);
        }

        public String getHeaderBINARY()
        {
            // returns the binary bitmap representation of the header
            return ConvHelper.hex2bin(ConvHelper.bytes2hex(headerData, headerData.Length, 0));
        }

        public int getHeaderLength()
        {
            // returns the binary bitmap representation of the header
            return headerData.Length;
        }
    }

    public class isoBitmap
    {

        byte[] m_bitmapData;        // holds the raw bytes of bitmap

        public isoBitmap()
        {
            // Initialize Bitmap Properties       
            m_bitmapData = new byte[24];
        }

        public isoBitmap(byte[] bitmap)
        {
            // Initialize bitmap with incomming raw data 
            m_bitmapData = bitmap;
        }

        public isoBitmap(String bitmap)
        {
            // Initialize bitmap with incomming HEXADECIMAL representation of data 
            m_bitmapData = ConvHelper.hex2bytes(bitmap);
        }

        public bool bitIsSet(int field)
        {
            // Checks if a bit in the bitmap is set
            if (field < 2)
            {
                // nothing to set. Field 000 Message type and 001 Bitmap are always present
                return true;
            }

            String bitmapBin = ConvHelper.hex2bin(ConvHelper.bytes2hex(m_bitmapData, m_bitmapData.Length, 0));

            if (bitmapBin[field - 1] == '1')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void setBit(int fieldNumber)
        {
            // Sets field participation indicator in bitmap. Even if field in message has data it wont be send  

            if (fieldNumber < 2)
            {
                // nothing to set. Field 000 Message type and 001 Bitmap are always present
                return;
            }

            String bitmapBin = ConvHelper.hex2bin(ConvHelper.bytes2hex(m_bitmapData, m_bitmapData.Length, 0));

            StringBuilder sb = new StringBuilder(bitmapBin);

            sb[fieldNumber - 1] = '1';

            bitmapBin = sb.ToString();

            String BitmapHex = ConvHelper.bin2hex(bitmapBin);

            m_bitmapData = ConvHelper.hex2bytes(BitmapHex);
        }

        public void clearBit(int fieldNumber)
        {
            // Clears field participation indicator in bitmap. Even if field in message has data it wont be send 
 
            if (fieldNumber < 3)
            {
                return;
            }

            String bitmapBin = ConvHelper.hex2bin(ConvHelper.bytes2hex(m_bitmapData, m_bitmapData.Length, 0));

            StringBuilder sb = new StringBuilder(bitmapBin);

            sb[fieldNumber - 1] = '0';

            bitmapBin = sb.ToString();

            String BitmapHex = ConvHelper.bin2hex(bitmapBin);

            m_bitmapData = ConvHelper.hex2bytes(BitmapHex);
        }

        public byte[] getBitmapBYTE()
        {
            // Returns the raw data of bitmap to be included in outgoing buffer
            // should detect if first,second,third bitmap is used and return appropriate part of byte sequence
            return (m_bitmapData);
        }

        public string getBitmapHEX()
        {
            // Returns the hexadecimal representation of raw data of bitmap
            return ConvHelper.bytes2hex(m_bitmapData, m_bitmapData.Length, 0);
        }

        public string getBitmapBINARY()
        {
            // Returns the binary sequence representation of the bitmap            
            return ConvHelper.hex2bin(ConvHelper.bytes2hex(m_bitmapData, m_bitmapData.Length, 0));
        }
    }

    public class isoFieldDefinition
    {
        public  FieldType     fieldType;
        public  EncodingType  fieldEncoding;
        public  LengthType    lengthType;
        public  EncodingType  lengthEncoding;
        public  String        name;
        public  String        description;
        public  int           number;
        public  int           lenLength;
        public  int           fixedLength;

        public isoFieldDefinition(int fn, FieldType ft, EncodingType fe, LengthType lt, EncodingType le, int fl)
        {

            fieldType = ft;                         // the data type of the field data

            lengthType = lt;                        // the length type FIXED,LVAR, LLVAR or LLLVAR
            
            if (lengthType == LengthType.LLVAR)
            {
                lenLength = 1;                      // check the encoding to decide how many bytes needed. if BCD then 1 byte is enough

                lengthEncoding = le;                // the length indicator encoding type

                fixedLength = 0;
            }
            else if (lengthType == LengthType.LLLVAR)
            {
                lenLength = 2;                      // check the encoding to decide how many bytes needed. if BCD then 2 bytes are enough

                lengthEncoding = le;                // the length indicator encoding type

                fixedLength = 0;
            }
            else if (lengthType == LengthType.FIXED)
            {
                lenLength = 0;                      // length indicator is 0 since is FIXED length field
                
                lengthEncoding = EncodingType.NULL; // encoding method is NULL since we have no length indicator to encode

                fixedLength = fl;
            }
            
            fieldEncoding = fe;                     // the field data encoding type

            number = fn;                            // field number

        }

        public void setDescription(String desc)
        {
            description = desc;
        }

        public string getDescription()
        {
            if (this.description != null)
            {
                return this.description;
            }
            else
            {
                return String.Empty;
            }
        }

        public void setName(String n)
        {
            name = n;
        }

        public string getName()
        {
            return name;
        }

        public LengthType getLengthType()
        {
            // returns the field legnthtype
            return lengthType;
        }

        public EncodingType getLengthEncoding()
        {
            // returns the field legnthtype
            return lengthEncoding;
        }

        public FieldType getType()
        {
            return fieldType;
        }

        public EncodingType getEncoding()
        {
            return fieldEncoding;
        }

        public int getLengthIndicatorLen()
        {
            // returns the size in bytes that length indicator holds
            return lenLength;
        }

        public int getLength()
        {
            return fixedLength;
        }
    }

    public class isoField
    {
        public int      length;
        public int      number;
        private byte[]  rawValue;

        public  FieldType       fieldType;
        public  EncodingType    fieldEncoding;
        public  LengthType      lengthType;
        public  EncodingType    lengthEncoding;
        public  String          name;
        public  String          description;
        public  int             lenLength;

        public isoField()
        {

        }

        public void setValueBYTE(byte[] myVal)
        {
            rawValue = myVal;

            if (lengthType == LengthType.LLLVAR || lengthType == LengthType.LLVAR)
            {
                if (fieldType == FieldType.N || fieldType == FieldType.B)
                {
                    length = myVal.Length * 2;
                }
                else
                {
                    length = myVal.Length;
                }
            }
        }

        public void setValue(String myVal)
        {
            //based on field type set the field val

            if (fieldType == FieldType.AN || fieldType == FieldType.ANS)
            {
                rawValue = ConvHelper.hex2bytes(ConvHelper.ascii2hex(myVal));
            }
            else if (fieldType == FieldType.N || fieldType == FieldType.B)
            {
                rawValue = ConvHelper.hex2bytes(myVal);
            }

            if (lengthType != LengthType.FIXED)
            {
                // dont overide dialect definition FIXED length casuse builder will copy bytes based on that
                // so is risky
                if (fieldType == FieldType.N || fieldType == FieldType.B)
                {
                    length = myVal.Length * 2;
                }
                else
                {
                    length = myVal.Length;
                }

            }

        }

        public string getValueASCII()
        {
            if (rawValue != null)
                return ConvHelper.hex2ascii(ConvHelper.bytes2hex(rawValue, rawValue.Length, 0));
            else
                return String.Empty;
        }

        public byte[] getValueBYTE()
        {
            return rawValue;
        }

        public string getValueHEX()
        {
            return ConvHelper.bytes2hex(rawValue, rawValue.Length, 0);
        }

        public string getValue()
        {
            if (this.number == 63)
            {
                return ConvHelper.bytes2hex(rawValue, rawValue.Length, 0);

            }
            else if (this.fieldType == FieldType.ANS || this.fieldType == FieldType.A || this.fieldType == FieldType.AN)
            {
                return ConvHelper.hex2ascii(ConvHelper.bytes2hex(rawValue, rawValue.Length, 0));
            }
            else
            {
                return ConvHelper.bytes2hex(rawValue, rawValue.Length, 0);
            }
        }

        public int getLengthIndicatorLen()
        {
            // returns the size in bytes that length indicator holds
            return lenLength;
        }

        public void setLength(int len)
        {
            length = len;
        }

        public int getLength()
        {
            // returns the length of the field

            if (fieldType == FieldType.N || fieldType == FieldType.B)
            {
                return (length / 2);
            }
            else
            {
                return length;
            }

        }

        public int getFieldLength()
        {
            /*
             * returns the length of data in the field data buffer 
             * (no length indicators calculated if field is LLVAR or LLLVAR)
             */

            return rawValue.Length;
        }

        public int getFieldLengthInclusive()
        {
            /*
             * returns the length of field if is FIXED or the length of field 
             * (assuming field value is set) + the LengthIndicator bytes
             */

            if (this.lengthType == LengthType.LLVAR)
            {
                if (fieldType == FieldType.N || fieldType == FieldType.B)
                {
                    return (length / 2) + 1;
                }
                else
                {
                    return (length) + 1;
                }

            }
            else if (this.lengthType == LengthType.LLLVAR)
            {
                if (fieldType == FieldType.N || fieldType == FieldType.B)
                {
                    return (length / 2) + 2;
                }
                else
                {
                    return (length) + 2;
                }
            }
            else
            {
                if (fieldType == FieldType.N || fieldType == FieldType.B)
                {
                    return (length / 2);
                }
                else
                {
                    return length;
                }
            }
        }

    }

    public class isoDialect
    {
        private int numberOfFields;

        private string name;

        public List<isoFieldDefinition> fieldList;
            
        public isoDialect (int totalFields)
        {

            Logger.Instance.Log("Initializing ISO Dialect hardcoded...");

            name = "imsp";

            fieldList = new List<isoFieldDefinition>();

            numberOfFields = totalFields;

            Logger.Instance.Log("Number of fields = " + totalFields.ToString());

            // Field[000][MTI]
            isoFieldDefinition f000 = new isoFieldDefinition(0, FieldType.N, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 4);
            f000.setDescription("Message Type Identifier");
            fieldList.Add(f000);

            // Field[001][BITMAP]
            isoFieldDefinition f001 = new isoFieldDefinition(1, FieldType.B, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 48);
            f001.setDescription("Bitmap Indicator");
            fieldList.Add(f001);

            // Field[002][ACOUNT NUMBER] ISO NUMBERIC - LLVAR - LENGTHBYTES BINARY
            isoFieldDefinition f002 = new isoFieldDefinition(2, FieldType.N, EncodingType.NULL, LengthType.LLVAR, EncodingType.BCD, 0);
            f002.setDescription("Account Number");
            fieldList.Add(f002);

            // Field[003][PROCESSING CODE] ISO NUMBERIC - FIXED
            isoFieldDefinition f003 = new isoFieldDefinition(3, FieldType.N, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 6);
            f003.setDescription("Processing Code");
            fieldList.Add(f003);

            // Field[004][TRASACTION AMOUNT] ISO NUMBERIC - FIXED
            isoFieldDefinition f004 = new isoFieldDefinition(4, FieldType.N, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 12);
            f004.setDescription("Transaction Amount");
            fieldList.Add(f004);

            // Field[005][NOT USED] 
            isoFieldDefinition f005 = new isoFieldDefinition(5, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f005);
            
            // Field[006][NOT USED] 
            isoFieldDefinition f006 = new isoFieldDefinition(6, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f006);
            
            // Field[007][NOT USED]
            isoFieldDefinition f007 = new isoFieldDefinition(7, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f007);
            
            // Field[008][NOT USED] 
            isoFieldDefinition f008 = new isoFieldDefinition(8, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f008);
            
            // Field[009][NOT USED]
            isoFieldDefinition f009 = new isoFieldDefinition(9, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f009);

            // Field[010][NOT USED]
            isoFieldDefinition f010 = new isoFieldDefinition(10, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f010);          
            
            // Field[011][SYSTEM TRACE AUDIT NUMBER] ISO NUMBERIC - FIXED
            isoFieldDefinition f011 = new isoFieldDefinition(11, FieldType.N, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 6);
            f011.setDescription("System Trace Audit Number");
            fieldList.Add(f011);

            // Field[011][TIME LOCAL TRANSACTION] ISO NUMBERIC - FIXED
            isoFieldDefinition f012 = new isoFieldDefinition(12, FieldType.N, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 6);
            f012.setDescription("Local Transaction Time");
            fieldList.Add(f012);

            // Field[013][DATE LOCAL TRANSACTION] ISO NUMBERIC - FIXED
            isoFieldDefinition f013 = new isoFieldDefinition(13, FieldType.N, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 4);
            f013.setDescription("Local Transaction Date");
            fieldList.Add(f013);

            // Field[014][EXPIRATION DATE] ISO NUMBERIC - FIXED
            isoFieldDefinition f014 = new isoFieldDefinition(14, FieldType.N, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 4);
            f014.setDescription("Account Expiration Date");
            fieldList.Add(f014);

            // Field[015][NOT USED]
            isoFieldDefinition f015 = new isoFieldDefinition(15, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f015);

            // Field[015][NOT USED]
            isoFieldDefinition f016 = new isoFieldDefinition(16, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f016);

            // Field[017][NOT USED]
            isoFieldDefinition f017 = new isoFieldDefinition(17, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f017);

            // Field[018][NOT USED]
            isoFieldDefinition f018 = new isoFieldDefinition(18, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f018);

            // Field[019][NOT USED]
            isoFieldDefinition f019 = new isoFieldDefinition(19, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f015);

            // Field[020][NOT USED]
            isoFieldDefinition f020 = new isoFieldDefinition(20, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f020);

            // Field[021][NOT USED]
            isoFieldDefinition f021 = new isoFieldDefinition(21, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f021);

            // Field[022][POINT OF SERVICE ENTRY MODE]
            isoFieldDefinition f022 = new isoFieldDefinition(22, FieldType.N, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 4);
            f022.setDescription("Point of Service Entry Mode");
            fieldList.Add(f022);

            // Field[023][NOT USED]
            isoFieldDefinition f023 = new isoFieldDefinition(23, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 4);
            fieldList.Add(f023);

            // Field[024][Network International Identifier]
            isoFieldDefinition f024 = new isoFieldDefinition(24, FieldType.N, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 4);
            f024.setDescription("Network International Identifier");
            fieldList.Add(f024);

            // Field[025][Network International Identifier]
            isoFieldDefinition f025 = new isoFieldDefinition(25, FieldType.N, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 2);
            f025.setDescription("POS Condition Code");
            fieldList.Add(f025);

            // Field[026][NOT USED]
            isoFieldDefinition f026 = new isoFieldDefinition(26, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f026);

            // Field[027][NOT USED]
            isoFieldDefinition f027 = new isoFieldDefinition(27, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f027);

            // Field[028][NOT USED]
            isoFieldDefinition f028 = new isoFieldDefinition(28, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f028);

            // Field[029][NOT USED]
            isoFieldDefinition f029 = new isoFieldDefinition(29, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f029);

            // Field[030][NOT USED]
            isoFieldDefinition f030 = new isoFieldDefinition(30, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f030);

            // Field[031][NOT USED]
            isoFieldDefinition f031 = new isoFieldDefinition(31, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f031);

            // Field[032][NOT USED]
            isoFieldDefinition f032 = new isoFieldDefinition(32, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f032);

            // Field[033][NOT USED]
            isoFieldDefinition f033 = new isoFieldDefinition(33, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f033);

            // Field[034][NOT USED]
            isoFieldDefinition f034 = new isoFieldDefinition(34, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f034);

            // Field[035][TRACK 2 DATA] ISO NUMBERIC - LLVAR - LENGTHBYTES BINARY
            isoFieldDefinition f035 = new isoFieldDefinition(35, FieldType.Z, EncodingType.NULL, LengthType.LLVAR, EncodingType.BCD, 0);
            f035.setDescription("Track2 Data");
            fieldList.Add(f035);

            // Field[036][NOT USED]
            isoFieldDefinition f036 = new isoFieldDefinition(36, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f036);

            // Field[037][RETRIEVAL REFERENCE NUMBER]
            isoFieldDefinition f037 = new isoFieldDefinition(37, FieldType.AN, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 12);
            f037.setDescription("Retrieval Reference Number");
            fieldList.Add(f037);
            
            // Field[038][AUTHORIZATION CODE]
            isoFieldDefinition f038 = new isoFieldDefinition(38, FieldType.AN, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 6);
            f038.setDescription("Authorization Code");
            fieldList.Add(f038);

            // Field[039][RESPONSE CODE]
            isoFieldDefinition f039 = new isoFieldDefinition(39, FieldType.AN, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 2);
            f039.setDescription("Response Code");
            fieldList.Add(f039);

            // Field[040][NOT USED]
            isoFieldDefinition f040 = new isoFieldDefinition(40, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f040);

            // Field[41][CARD ACCEPTOR TERMINAL IDENTIFICATION]
            isoFieldDefinition f041 = new isoFieldDefinition(41, FieldType.ANS, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 8);
            f041.setDescription("Card Acceptor Terminal Identification");
            fieldList.Add(f041);
            
            // Field[042][CARD ACCEPTOR IDENTIFICATION CODE]
            isoFieldDefinition f042 = new isoFieldDefinition(42, FieldType.ANS, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 15);
            f042.setDescription("Card Acceptor Identification Code");
            fieldList.Add(f042);

            // Field[040][NOT USED]
            isoFieldDefinition f043 = new isoFieldDefinition(43, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f043);

            // Field[044][NOT USED]
            isoFieldDefinition f044 = new isoFieldDefinition(44, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f044);

            // Field[045][NOT USED]
            isoFieldDefinition f045 = new isoFieldDefinition(45, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f045);

            // Field[046][NOT USED]
            isoFieldDefinition f046 = new isoFieldDefinition(46, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f046);
            
            // Field[047][NOT USED]
            isoFieldDefinition f047 = new isoFieldDefinition(47, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f047);
            
            // Field[048][NOT USED]
            isoFieldDefinition f048 = new isoFieldDefinition(48, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f048);

            // Field[049][TRANSACTION CURRENCY CODE]
            isoFieldDefinition f049 = new isoFieldDefinition(49, FieldType.N, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 4);
            f049.setDescription("Transaction Currency Code");
            fieldList.Add(f049);

            // Field[050][NOT USED]
            isoFieldDefinition f050 = new isoFieldDefinition(50, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f050);

            // Field[051][NOT USED]
            isoFieldDefinition f051 = new isoFieldDefinition(51, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f051);

            // Field[52][PERSONAL IDENTIFICATION CODE]
            isoFieldDefinition f052 = new isoFieldDefinition(52, FieldType.B, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 16);
            f052.setDescription("Personal Identification Code");
            fieldList.Add(f052);

            // Field[053][NOT USED]
            isoFieldDefinition f053 = new isoFieldDefinition(53, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f053);

            // Field[054][NOT USED]
            isoFieldDefinition f054 = new isoFieldDefinition(54, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f054);
            
            // Field[55][ICC SYSTEM RELATED DATA]
            isoFieldDefinition f055 = new isoFieldDefinition(55, FieldType.B, EncodingType.NULL, LengthType.LLLVAR, EncodingType.BCD, 0);
            f055.setDescription("ICC System Related Data");
            fieldList.Add(f055);

            // Field[056][NOT USED]
            isoFieldDefinition f056 = new isoFieldDefinition(56, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f056);

            // Field[057][NOT USED]
            isoFieldDefinition f057 = new isoFieldDefinition(57, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f057);

            // Field[058][NOT USED]
            isoFieldDefinition f058 = new isoFieldDefinition(58, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f058);

            // Field[059][NOT USED]
            isoFieldDefinition f059 = new isoFieldDefinition(59, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f059);

            // Field[060][Private Use - Batch Number + SoftID + OrigData(stan/mti) + origAmt + EncKey(110/210) ]
            isoFieldDefinition f060 = new isoFieldDefinition(60, FieldType.ANS, EncodingType.NULL, LengthType.LLLVAR, EncodingType.BCD, 0);
            f060.setDescription("Private Use - Batch Number + SoftID + OrigData(stan/mti) + origAmt + EncKey(110/210)");
            fieldList.Add(f060);

            // Field[061][NOT USED]
            isoFieldDefinition f061 = new isoFieldDefinition(61, FieldType.NULL, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 0);
            fieldList.Add(f061);

            // Field[062][RECONCILIATION TOTALS]
            isoFieldDefinition f062 = new isoFieldDefinition(62, FieldType.ANS, EncodingType.NULL, LengthType.LLLVAR, EncodingType.BCD, 0);
            f062.setDescription("Reconciliation totals");
            fieldList.Add(f062);

            // Field[063][ADDITIONAL DATA]
            isoFieldDefinition f063 = new isoFieldDefinition(63, FieldType.ANS, EncodingType.NULL, LengthType.LLLVAR, EncodingType.BCD, 0);
            f063.setDescription("Additional Data");
            fieldList.Add(f063);

            // Field[064][MAC - MESSAGE AUTHENTICATION CODE]
            isoFieldDefinition f064 = new isoFieldDefinition(64, FieldType.B, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 8);
            f064.setDescription("MAC - Message Authentication Code");
            fieldList.Add(f064);

            // Field[065][BITMAP]
            isoFieldDefinition f065 = new isoFieldDefinition(65, FieldType.B, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 1);
            f065.setDescription("Bitmap Extender");
            fieldList.Add(f065);

            // Field[066][MAC - MESSAGE AUTHENTICATION CODE]
            isoFieldDefinition f066 = new isoFieldDefinition(66, FieldType.B, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 8);
            f066.setDescription("MAC - Message Authentication Code");
            fieldList.Add(f066);

            // Field[067][MAC - MESSAGE AUTHENTICATION CODE]
            isoFieldDefinition f067 = new isoFieldDefinition(67, FieldType.B, EncodingType.NULL, LengthType.FIXED, EncodingType.NULL, 8);
            f067.setDescription("MAC - Message Authentication Code");
            fieldList.Add(f067);
        }

        public isoDialect (String defFileName)
        {
            // initialize iso dialect form xml defintion file
            name = "imsp";
        }
        
        public isoFieldDefinition getFieldDefinition(int fieldNum)
        {
            // return the field definition of field fieldNum
            return fieldList.ElementAt(fieldNum);
        }

        public int getTotalFields()
        {
            return numberOfFields;
        }

        public void saveToFile(string fileName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("isodialect");
            xmlDoc.AppendChild(rootNode);

            foreach (isoFieldDefinition f in fieldList)
            {
                XmlNode fieldNode = xmlDoc.CreateElement("field");
                
                //field number 
                XmlAttribute attribute = xmlDoc.CreateAttribute("num");         
                attribute.Value = f.number.ToString("D3");           
                fieldNode.Attributes.Append(attribute);

                //legnth type
                attribute = xmlDoc.CreateAttribute("lenType");
                attribute.Value = f.lengthType.ToString().PadRight(6,' ');
                fieldNode.Attributes.Append(attribute);

                // length encoding
                attribute = xmlDoc.CreateAttribute("lenEnc");
                attribute.Value = f.lengthEncoding.ToString().PadRight(4,' ');
                fieldNode.Attributes.Append(attribute);

                // length encoding
                attribute = xmlDoc.CreateAttribute("len");
                attribute.Value = f.getLength().ToString("D3");
                fieldNode.Attributes.Append(attribute);

                // field name
                attribute = xmlDoc.CreateAttribute("name");
                attribute.Value = f.name;
                fieldNode.Attributes.Append(attribute);

                // field description
                attribute = xmlDoc.CreateAttribute("desc");
                attribute.Value = f.description;
                fieldNode.Attributes.Append(attribute);

                //fieldNode.InnerText = "John Doe";
                rootNode.AppendChild(fieldNode);

            
            }

            xmlDoc.Save(fileName); 

        }

        public void traceToConsole()
        {
            Logger.Instance.Log("Tracing dialect definition:");
            Logger.Instance.Log("Dialect name: " + name);
            Logger.Instance.Log("Number of fields: " + numberOfFields.ToString());
            foreach (isoFieldDefinition f in fieldList)
            {
                Logger.Instance.Log(String.Format("Field Number      : [{0}] ", f.number.ToString("D3")));
                if (f.lengthType == LengthType.FIXED)
                    Logger.Instance.Log(String.Format("Field Length Type : [FIXED] "));
                else if (f.lengthType == LengthType.LLVAR)
                    Logger.Instance.Log(String.Format("Field Length Type : [LLVAR] "));
                else if (f.lengthType == LengthType.LLLVAR)
                    Logger.Instance.Log(String.Format("Field Length Type : [LLLVAR] "));

                Logger.Instance.Log("");
            }
        }

        public void traceToFile()
        {
            Logger.Instance.Log("Tracing dialect definition:");
            Logger.Instance.Log("Dialect name: " + name);
            Logger.Instance.Log("Number of fields: " + numberOfFields.ToString());
            foreach (isoFieldDefinition f in fieldList)
            {
                    Logger.Instance.Log(String.Format("Field Number      : [{0}] ",  f.number.ToString("D3")));
                if (f.lengthType==LengthType.FIXED)
                    Logger.Instance.Log(String.Format("Field Length Type : [FIXED] "));
                else if (f.lengthType==LengthType.LLVAR)
                    Logger.Instance.Log(String.Format("Field Length Type : [LLVAR] "));
                else if (f.lengthType==LengthType.LLLVAR)
                    Logger.Instance.Log(String.Format("Field Length Type : [LLLVAR] "));

                Logger.Instance.Log("");
            }
        }
    }

    public class isoMessage
    {
        private List<isoField> fieldList;    // iso fields with their values

        public isoDialect msgDialect;        // iso message dialect definition

        public isoBitmap msgBitmap;          // iso message bitmap handler

        public isoHeader msgHeader;          // iso message header handler

        public byte[] requestBuffer;         // the raw data buffer that contains the request message

        public byte[] responseBuffer;        // the raw data buffer that contains the response message

        public String f63v;                  // Final value of field 63

        public int bufLength;                // the raw data buffer length in bytes

        public isoMessage(byte[] buffer, int bufLen)
        {
            fieldList       = new List<isoField>();

            requestBuffer   = buffer;
            
            bufLength       = bufLen;
            
            msgDialect      = new isoDialect(67);
            
            responseBuffer  = new byte[] { 0 };
            
            f63v            = String.Empty;

            msgBitmap       = new isoBitmap();

            msgHeader       = new isoHeader();

            // dialect initialized so initialize fields

            for (int i = 0; i < msgDialect.getTotalFields(); i++)
            {
                isoField nIsoField = new isoField();
                fieldList.Add(nIsoField);
                this.fieldList[i].number = i;
                this.fieldList[i].name           = msgDialect.fieldList[i].name;
                this.fieldList[i].fieldEncoding  = msgDialect.fieldList[i].fieldEncoding;
                this.fieldList[i].fieldType      = msgDialect.fieldList[i].fieldType;
                this.fieldList[i].length         = msgDialect.fieldList[i].fixedLength;
                this.fieldList[i].lengthType     = msgDialect.fieldList[i].lengthType;
                this.fieldList[i].description    = msgDialect.fieldList[i].description;
                this.fieldList[i].lengthEncoding = msgDialect.fieldList[i].lengthEncoding;
            }
        }

        public isoMessage(byte[] buffer, int bufLen, isoDialect isoDialect)
        {
            /*
             * dialect for same type of message should be initialized once somewhere and passed as reference here
             * in order not to load xml file every time
             */
            
            fieldList       = new List<isoField>();
            
            requestBuffer   = buffer;

            bufLength       = bufLen;
            
            msgDialect      = isoDialect; 
            
            responseBuffer  = new byte[] { 0 };
            
            f63v            = String.Empty;

            for (int i = 0; i < msgDialect.getTotalFields(); i++)
            {
                //isoFieldDefinition fDef =  msgDialect.getFieldDefinition(i);

                this.fieldList[i].number = i;

                //this.fieldList[i].name          = fDef.getName();
                //this.fieldList[i].fieldEncoding = fDef.getEncoding();
                //this.fieldList[i].fieldType     = fDef.getType();
                //this.fieldList[i].length        = fDef.getLength();
                //this.fieldList[i].lengthType    = fDef.getLengthType();
                //this.fieldList[i].description   = fDef.getDescription();

                this.fieldList[i].name = msgDialect.fieldList[i].name;
                this.fieldList[i].fieldEncoding = msgDialect.fieldList[i].fieldEncoding;
                this.fieldList[i].fieldType = msgDialect.fieldList[i].fieldType;
                this.fieldList[i].length = msgDialect.fieldList[i].fixedLength;
                this.fieldList[i].lengthType = msgDialect.fieldList[i].lengthType;
                this.fieldList[i].description = msgDialect.fieldList[i].description;
            }
        }

        public string getFieldDescription(int fn)
        {
            return fieldList[fn].description;
        }

        public int getFieldLength(int fn)
        {
            if (fieldList[fn].fieldType == FieldType.N || fieldList[fn].fieldType == FieldType.B)
            {
                return fieldList[fn].length/2;
            }
            else
            {
                return fieldList[fn].length;
            }
        }

        public string getLengthHEX()
        {
            return ConvHelper.bytes2hex(requestBuffer, 2, 0);
        }

        public int getLengthInt()
        {
            short result;

            Int16.TryParse((requestBuffer[0] + requestBuffer[1]).ToString("D4"), out result);

            return result;
        }

        public string getLengthDec()
        {
            return (requestBuffer[0] + requestBuffer[1]).ToString("D4");
        }

        public void Parse()
        {
            // number of bytes used for lengthIndicator
            int pos = 2;

            // Parse Header - 5 is the header length (tpdu) 
            // we should know from header defintion how many bytes is the header
            msgHeader = new isoHeader(requestBuffer.Skip(pos).Take(5).ToArray());
            pos += 5;

            if (this.fieldList[0].lengthType == LengthType.FIXED)
            {
                // should be removed - it keeps field data on dialect definition
                fieldList[0].setValueBYTE(requestBuffer.Skip(pos).Take(fieldList[0].getLength()).ToArray());

                // increase position in message buffer
                pos = pos + fieldList[0].getLength();
            }

            // The bitmap may be transmitted as 8 bytes of binary data or as 16 hexadecimal characters 0-9, A-F in the ASCII 
            // or EBCDIC character sets.

            msgBitmap = new isoBitmap(requestBuffer.Skip(pos).Take(fieldList[1].getLength()).ToArray());

            fieldList[1].setValueBYTE(requestBuffer.Skip(pos).Take(fieldList[1].getLength()).ToArray());

            pos += fieldList[1].getLength();

            // for each bit that exists get its value based on its definition

            for (int i = 2; i < this.msgDialect.getTotalFields(); i++)
            {

                if (msgBitmap.bitIsSet(i))
                {

                    if (fieldList[i].lengthType == LengthType.FIXED)
                    {
                        fieldList[i].setValueBYTE(requestBuffer.Skip(pos).Take(fieldList[i].getLength()).ToArray());
                        pos += fieldList[i].getLength();
                    }
                    else if (fieldList[i].lengthType == LengthType.LLVAR && fieldList[i].fieldType == FieldType.Z && fieldList[i].lengthEncoding == EncodingType.BCD)
                    {
                        int fieldLength;
                        Int32.TryParse(ConvHelper.bytes2hex(requestBuffer, 1, pos), out fieldLength);
                        int padLen = fieldLength % 2;
                        fieldLength = padLen + (fieldLength / 2);
                        pos += 1;
                        fieldList[i].setValueBYTE(requestBuffer.Skip(pos).Take(fieldLength).ToArray());
                        pos += fieldLength;
                    }
                    else if (fieldList[i].lengthType == LengthType.LLVAR && fieldList[i].lengthEncoding == EncodingType.BCD)
                    {
                        int fieldLength;
                        Int32.TryParse(ConvHelper.bytes2hex(requestBuffer, 1, pos), out fieldLength);
                        pos += 1;
                        if (fieldList[i].fieldType == FieldType.N)
                        {
                            fieldList[i].setValueBYTE(requestBuffer.Skip(pos).Take(fieldLength / 2).ToArray());
                            pos += fieldLength / 2;
                        }
                        else
                        {
                            fieldList[i].setValueBYTE(requestBuffer.Skip(pos).Take(fieldLength).ToArray());
                            pos += fieldLength;
                        }
                    }
                    else if ((fieldList[i].lengthType == LengthType.LLLVAR) && fieldList[i].lengthEncoding == EncodingType.BCD)
                    {
                        int fieldLength;
                        Int32.TryParse(ConvHelper.bytes2hex(requestBuffer, 2, pos), out fieldLength);
                        pos += 2;

                        if (fieldList[i].fieldType == FieldType.N)
                        {
                            fieldList[i].setValueBYTE(requestBuffer.Skip(pos).Take(fieldLength).ToArray());
                            pos += fieldLength / 2;
                        }
                        else
                        {
                            fieldList[i].setValueBYTE(requestBuffer.Skip(pos).Take(fieldLength).ToArray());
                            pos += fieldLength;
                        }
                    }
                }
            }
        }

        public string getBitmapHex()
        {
            return msgBitmap.getBitmapHEX();
        }

        public string getBitmapBin()
        {
            return msgBitmap.getBitmapBINARY();
        }

        public bool isFieldSet(int field)
        {
            return msgBitmap.bitIsSet(field);
        }

        public void setField(int field)
        {
            msgBitmap.setBit(field);
        }

        public void clearField(int field)
        {
            msgBitmap.clearBit(field);
        }

        public byte[] packForTransmission()
        {
            // calculate the total length that buffer should have so we can Rezize the buffer

            int totLength = 2;
            int lenIndicator;
            int pos = 0;

            totLength = totLength + msgHeader.getHeaderBYTE().Length;

            if (f63v != String.Empty)
            {
                fieldList[63].setValueBYTE(ConvHelper.hex2bytes(f63v));
            }

            for (int i = 0; i < this.msgDialect.getTotalFields(); i++)
            {

                if (msgBitmap.bitIsSet(i))
                {
                    int fLen    = fieldList[i].getLength();
                    int fIndLen = fieldList[i].getLengthIndicatorLen();

                    totLength = totLength + fLen + fIndLen;
                }

            }

            bufLength = totLength;

            //trc.Print("Total Outgoing Length: " + totLength.ToString(), richTextBox);
            //trc.Print("Resizing Buffer to new length", richTextBox);

            // important dont forget to add 2 (the message length when u resize the array)
            Array.Resize(ref responseBuffer, totLength);

            responseBuffer.Initialize();

            totLength = totLength - 2;

            ConvHelper.hex2bytes(totLength.ToString("X").PadLeft(4, '0')).CopyTo(responseBuffer, 0);
            pos += 2;

            msgHeader.getHeaderBYTE().CopyTo(responseBuffer, pos);
            pos = pos + msgHeader.getHeaderLength();

            fieldList[0].getValueBYTE().CopyTo(responseBuffer, pos);
            pos = pos + fieldList[0].getFieldLengthInclusive();

            msgBitmap.getBitmapBYTE().CopyTo(responseBuffer, pos);
            pos += 24;

            // traverse all fields that exists in the bitmap and get length indicators and values
            // and append to the buffer

            for (int i = 0; i < this.msgDialect.getTotalFields(); i++)
            {
                if (i == 1 || i == 0)
                {
                    continue;
                }

                if (msgBitmap.bitIsSet(i))
                {
                    lenIndicator = fieldList[i].getLengthIndicatorLen();

                    if (lenIndicator > 0)
                    {
                        if (lenIndicator == 1)
                        {
                            if (fieldList[i].fieldType == FieldType.N || fieldList[i].fieldType == FieldType.Z)
                            {
                                ConvHelper.hex2bytes((2 * fieldList[i].getLength()).ToString("D2")).CopyTo(responseBuffer, pos);
                            }
                            else
                            {
                                ConvHelper.hex2bytes((msgDialect.fieldList[i].getLength()).ToString("D2")).CopyTo(responseBuffer, pos);
                            }
                        }
                        else if (lenIndicator == 2)
                        {
                            if (fieldList[i].fieldType == FieldType.N || fieldList[i].fieldType == FieldType.Z)
                            {
                                ConvHelper.hex2bytes((2 * fieldList[i].getLength()).ToString("D4")).CopyTo(responseBuffer, pos);
                            }
                            else
                            {
                                ConvHelper.hex2bytes((fieldList[i].getLength()).ToString("D4")).CopyTo(responseBuffer, pos);
                            }
                        }

                        pos = pos + lenIndicator;
                    }

                    fieldList[i].getValueBYTE().CopyTo(responseBuffer, pos);

                    pos = pos + fieldList[i].getLength();
                }
            }

            // Print the raw data buffer
            // trc.Print("ISO Message Response", richTextBox);

            return responseBuffer;
        }

        public byte[] getBufferHEX()
        {
            // returns the byte[] buffer that holds transmission data
            return requestBuffer;
        }

        public void setFieldValue(int fn, String val)
        {
            setField(fn);
            fieldList[fn].setValue(val);
        }

        public string getFieldValue(int fn)
        {
            if (msgBitmap.bitIsSet(fn))
            {
                return fieldList[fn].getValue();
            }
            else
            {
                return String.Empty;
            }
        }

        public void setField63(String tag, String val)
        {
            String f63value;
            String f63FinalValue;

            int length;

            setField(63);

            //Convert incomming tag to HEX
            f63value = ConvHelper.ascii2hex(tag);

            //Convert incomming value to HEX
            f63value = f63value + ConvHelper.ascii2hex(val);

            //Calculate their lengh
            length = f63value.Length / 2;

            // length of tag then tag then value
            f63FinalValue = length.ToString("D4") + f63value;

            f63v = f63v + f63FinalValue;

        }

        public string getBufferHex()
        {
            return ConvHelper.bytes2hex(responseBuffer, responseBuffer.Length, 0);
        }
    }
}
