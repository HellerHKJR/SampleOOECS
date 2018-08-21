using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OOECS01
{
    public class ControlArg : EventArgs
    {
        public string Name { get; set; }
        public string CommandName { get; set; }
        public object[] Values { get; set; }
        public Hashtable MapValues { get; set; }

        public ControlArg(string name)
        {
            Name = name;
        }

        public ControlArg(string cmdName, params object[] values)
        {
            CommandName = cmdName;
            Values = values;
        }

        public ControlArg(string cmdName, Hashtable mapValues)
        {
            CommandName = cmdName;
            MapValues = mapValues;
        }

        public ControlArg(string cmdName, Hashtable mapValues, params object[] values)
        {
            CommandName = cmdName;
            MapValues = mapValues;
            Values = values;
        }

        public ControlArg(string name, string cmdName, Hashtable mapValues, params object[] values)
        {
            Name = name;
            CommandName = cmdName;
            MapValues = mapValues;
            Values = values;

            //ViewArg k = new ViewArg("abc", "Go", new Hashtable { { "dest", new List<object> { typeof(string), "Suwon" } }, { "departure", new object[] { typeof(string), "Seoul" } } }, "abc", "def", "ghi");
        }
    }

    public class SECSMessageArg : EventArgs
    {   //Use when received from host
        public SECSMessageArg(long nObjectID, long nStream, long nFunction, long nSysbyte)
        {
            this.nObjectID = nObjectID;
            this.nStream = nStream;
            this.nFunction = nFunction;
            this.nSB = nSysbyte;
        }
        public long nObjectID = 0;
        public long nStream = 0;
        public long nFunction = 0;
        public long nSB = 0;

        public override string ToString()
        {
            return string.Format("XGem ObjectID={0}, S{1}F{2}, SB={3}", nObjectID, nStream, nFunction, nSB);
        }
    }

    public class HostRemoteCommandArg : SECSMessageArg
    {
        public class RCMDPair
        {
            public RCMDPair(string cpName, string cpVal, byte cpAck)
            {
                this.cpName = cpName;
                this.cpVal = cpVal;
                this.cpAck = cpAck;
            }

            public string cpName;
            public string cpVal;
            public byte cpAck;
        }
        public HostRemoteCommandArg(long nObjectID, long nStream, long nFunction, long nSysbyte, string RCMD) : base(nObjectID, nStream, nFunction, nSysbyte)
        {
            this.RCMD = RCMD;
            cpList = new List<RCMDPair>();
        }

        public string RCMD = "";
        public long HcAck = 0;
        public List<RCMDPair> cpList;

        public void SetCPVal(string cpName, string cpVal)
        {
            cpList.Add(new RCMDPair(cpName, cpVal, 0));
        }

        public void SetHcAck(long HcAck = 0)
        {
            this.HcAck = HcAck;
        }

        public void SetCpAck(string cpName, byte cpAck)
        {
            foreach (RCMDPair refRCMD in cpList)
            {
                if (cpName.Equals(refRCMD.cpName))
                {
                    refRCMD.cpAck = cpAck;
                    break;
                }
            }
        }

        public T[] GetCpAcks<T>()
        {
            int cpCount = cpList.Count;
            T[] acks = new T[cpCount];

            int i = 0;
            foreach (RCMDPair cp in cpList)
            {
                acks[i++] = (T)Convert.ChangeType(cp.cpAck, typeof(T));
            }

            return acks;
        }

        public string[] GetCpNames()
        {
            int cpCount = cpList.Count;
            string[] cpNames = new string[cpCount];

            int i = 0;
            foreach (RCMDPair cp in cpList)
            {
                cpNames[i++] = cp.cpName;
            }

            return cpNames;
        }

        public string GetCpValue(string cpName)
        {
            string ret = "";
            foreach (RCMDPair refRCMD in cpList)
            {
                if (cpName.Equals(refRCMD.cpName))
                {
                    ret = refRCMD.cpVal;
                    break;
                }
            }

            return ret;
        }
    }
}
