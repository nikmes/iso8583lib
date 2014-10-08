using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTLV
{
    public class TLVObject
    {
        private TLVObject parent = null;
        public TLVObject Parent
        {
            get { return parent; }
            set { parent = value; }
        }
        private List<TLVObject> childList = new List<TLVObject>();
        public List<TLVObject> ChildList
        {
            get { return childList; }
            set { childList = value; }
        }

        public string TagStr
        {
            get
            {
                return tagStr;
            }
        }
        public string LengthStr
        {
            get
            {
                return Utils.ToHexStr(lenBytes, 0, lenBytes.Length);
            }
        }
        public string ValueStr
        {
            get
            {
                return Utils.ToHexStr(mValue, 0, mValue.Length);
            }
        }   
        public byte[] Tag
        {
            get
            {
                return Utils.HexToByteArray(tagStr);
            }
        }
        public byte[] Length
        {
            get
            {
                return lenBytes;
            }
        }
        public byte[] Value
        {
            get
            {
                return mValue;
            }
        }
        public int LengthInt
        {
            get
            {
                return mLen;
            }
        }

        private string tagStr;
        private int tagWidth;
        private int mLen;
        private int lenWidth;
        private byte[] lenBytes;
        private byte[] mValue;

        public TLVObject(string tag)
        {
            if ((tag.Length % 2) != 0)
                throw new Exception("Error in Tag (length)");

            tagWidth = tag.Length / 2;
            tagStr = tag;
            mLen = 0;
            mValue = new byte[0];
            SetLengthBytes();

            this.parent = null;
        }

        public TLVObject(string tag, byte[] value)
        {
            if ((tag.Length % 2) != 0)
                throw new Exception("Error in Tag (length)");

            tagWidth = tag.Length / 2;

            tagStr = tag;
            mLen = value.Length;
            mValue = value;
            SetLengthBytes();

            this.parent = null;
        }

        public TLVObject(string tag, string strVal)
        {
            if ((tag.Length % 2) != 0)
                throw new Exception("Error in Tag (length)");

            if ((strVal.Length % 2) != 0)
                throw new Exception("Error in Value (length)");

            tagWidth = tag.Length / 2;

            tagStr = tag;
            mLen = strVal.Length / 2;
            mValue = Utils.HexToByteArray(strVal);

            SetLengthBytes();

            this.parent = null;
        }

        public void SetValue(byte[] value)
        {
            mLen = value.Length;
            mValue = new byte[mLen];
            Array.Copy(value, 0, mValue, 0, mLen);

            SetLengthBytes();
        }

        public void SetValue(string value)
        {
            mLen = value.Length / 2;
            mValue = Utils.HexToByteArray(value);
            SetLengthBytes();
        }

        public void AddChildObject(TLVObject obj)
        {
            mValue = Utils.BufferConcat(mValue, obj.ToByteArray());
            mLen = mValue.Length;
            SetLengthBytes();
            obj.parent = this;
            this.childList.Add(obj);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("TAG:" + tagStr + " LEN:" + mLen.ToString() + Utils.ToHexStr(" DATA:", mValue, mValue.Length));
            if(this.parent !=null)
                sb.Append(" Parent:" + this.parent.TagStr);
            else
                sb.Append(" Parent: ROOT");
            sb.Append(" Children:");

            if (this.childList.Count == 0)
                sb.Append("N/A");
            
            foreach (TLVObject obj in this.childList)
                sb.Append( obj.tagStr + " ");

            return sb.ToString();
        }

        public byte[] ToByteArray()
        {

            byte[] Length=null;

            if (mLen < 128)
            {
                Length = new byte[1];
                Length[0] = (byte)this.mLen;
                lenWidth = 1;
            }
            else if (mLen < 256)
            {
                Length = new byte[2];
                Length[0] = 0x81;
                Length[1] = (byte)this.mLen;
                lenWidth = 2;
            }
            else
            {
                Length = new byte[3];
                Length[0] = 0x82;
                Length[1] = (byte) (mLen / 256);
                Length[2] = (byte) (mLen % 256);
                lenWidth = 3;
            }

            byte[] ret = new byte[tagWidth + mValue.Length + lenWidth];

            Array.Copy(Utils.HexToByteArray(tagStr), 0, ret, 0, tagWidth);
            Array.Copy(Length, 0, ret, tagWidth, lenWidth);

            if (this.mValue != null)
                Array.Copy(mValue, 0, ret, tagWidth + lenWidth, mValue.Length);

            return ret;
        }

        private void SetLengthBytes()
        {
            if (mLen < 128)
            {
                lenBytes = new byte[1];
                lenBytes[0] = (byte)mLen;
                lenWidth = 1;
            }
            else if (mLen < 256)
            {
    	        lenBytes = new byte[2];
                lenBytes[0] = 0x81;
                lenBytes[0] = (byte)mLen;
                lenWidth = 2;
            }
            else
            {
    	        lenBytes = new byte[3];
                lenBytes[0] = 0x82;
    	        lenBytes[1] = (byte) (mLen / 256);
    	        lenBytes[2] = (byte) (mLen % 256);
                lenWidth = 3;
            }
        }
    }
}
