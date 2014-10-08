using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibTLV
{
    public class TLV
    {
        public int NbOfObjects
        {
            get {return elementPool.Count;}
        }

        TLVObject parent = null;

        private List<TLVObject> elementPool = new List<TLVObject>();
        public List<TLVObject> ObjectList
        {
            get { return elementPool; }
            set { elementPool = value; }
        }
        
        private List<string> knownTags = null;
        public List<string> KnownTags
        {
            set
            {
                knownTags = value;
            }
        }
             
        public void Parse(byte[] encodedTLV)
        {
            _Parse(encodedTLV);
        }

        private int _Parse(byte[] encodedTLV)
        {
            string TLVTag;
            int TLVLen;
            byte[] TLVData;

            byte[] TLVTagBytes;
            int TagSize = 1;
            int LenSize;
            
            if ((encodedTLV[0] & 0x1F) == 0x1F)
            {
                TagSize++;
                for (int idx = 1; idx < encodedTLV.Length; ++idx)
                {
                    if ((encodedTLV[idx] & 0x80) == 0x80)
                        TagSize++;
                    else break;
                }
            }

            TLVTagBytes = new byte[TagSize];
            Array.Copy(encodedTLV, 0, TLVTagBytes, 0, TagSize);
            TLVTag = Utils.ToHexStr(TLVTagBytes, 0, TagSize);
            int tlvLenOffset = TagSize;
            
            if (encodedTLV[TagSize] < 128)
            {
                LenSize = 1;
                TLVLen = encodedTLV[TagSize];
            }
	        else
	        {
		        LenSize = 1 + (0x7F & encodedTLV[tlvLenOffset]);
                byte[] lenTmp = new byte[4];

		        int ofsetLenBytes = TagSize +1;
		        int nbLenBytes = LenSize -1;

                if (nbLenBytes > 4) // 4 bytes is quite enaugh
                    throw new Exception("Length error in TLV package"); 

		        for (int j=0; j<nbLenBytes; ++j )
			        lenTmp[nbLenBytes - j -1] = encodedTLV[ofsetLenBytes +j];

                TLVLen = BitConverter.ToInt32(lenTmp, 0);
	        }

            TLVData = new byte[TLVLen];
            Array.Copy(encodedTLV, TagSize + LenSize, TLVData, 0, TLVLen);
            TLVObject newObj = new TLVObject(TLVTag, TLVData);
            newObj.Parent = parent;
            if(newObj.Parent != null)
                newObj.Parent.ChildList.Add(newObj);
            elementPool.Add(newObj);

            if (((byte)encodedTLV[0] & 32) == 32)
            {
                TLVObject oldParent = parent;
                parent = newObj;
                int TotalLen = TLVData.Length;
                int Index = 0;
                while ((TotalLen - Index) > 0)
                {
                    byte[] SubTLV = new byte[TotalLen - Index];
                    Array.Copy(TLVData, Index, SubTLV, 0, SubTLV.Length);
                    Index += this._Parse(SubTLV);
                }
                parent = oldParent;
            }

            return TagSize + LenSize + TLVData.Length;
        }

        public void addTLVObject(TLVObject tlvObj)
        {
            if (tlvObj == null)
                return;

            elementPool.Add(tlvObj);
        }

        public TLVObject getFirstObject(string tag)
        {
            foreach (TLVObject obj in this.ObjectList)
            {
                if (obj.TagStr.Equals(tag))
                    return obj;
            }

            return null;
        }

        public TLVObject getObjectAt(int index)
        {
            if (index >= elementPool.Count)
                return null;

            return elementPool.ElementAt(index);
        }

        public List<TLVObject> getObjectList(string tag)
        {
            List<TLVObject> tlvList = new List<TLVObject>();
            
            foreach (TLVObject tlvItem in elementPool)
            {
                if(tlvItem.TagStr.Equals(tag))
                    tlvList.Add(tlvItem);
            }

            return tlvList;           
        }

        private bool isTagKnown(string tag)
        {
            if(knownTags == null)
                return true;

            if (knownTags.Contains(tag))
                return true;
            else 
                return false;
        }
    }
}
