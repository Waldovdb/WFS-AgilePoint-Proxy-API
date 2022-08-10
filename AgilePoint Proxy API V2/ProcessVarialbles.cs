using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgilePoint_Proxy_API_V2
{
    class ProcessVariables
    {
        public bool blnStartImmediately { get; set; } = true;
        public string CustomID { get; set; }
        public string Initiator { get; set; }
        public string ProcessID { get; set; }
        public string ProcessInstID { get; set; }
        public string ProcInstName { get; set; }
        public string SuperProcInstID { get; set; }
        public string WorkObjID { get; set; }
        public string WorkObjInfo { get; set; }
        public CustomAtt[] Attributes { get; set; }

        public ProcessVariables(string inCustID, string inInit, string inProcID, string inProcInstID, string inProcInstName, string inSupProcInstID, string inWorkObjID, string inWorkObjInfo)
        {
            this.CustomID = inCustID;
            this.Initiator = inInit;
            this.ProcessID = inProcID;
            this.ProcessInstID = inProcInstID;
            this.ProcInstName = inProcInstName;
            this.SuperProcInstID = inSupProcInstID;
            this.WorkObjID = inWorkObjID;
            this.WorkObjInfo = inWorkObjInfo;
            this.Attributes = new CustomAtt[]{};
        }
        
        public ProcessVariables(string inCustID, string inInit, string inProcID, string inProcInstID, string inProcInstName, string inSupProcInstID, string inWorkObjID, string inWorkObjInfo, CustomAtt[] attributes)
        {
            this.CustomID = inCustID;
            this.Initiator = inInit;
            this.ProcessID = inProcID;
            this.ProcessInstID = inProcInstID;
            this.ProcInstName = inProcInstName;
            this.SuperProcInstID = inSupProcInstID;
            this.WorkObjID = inWorkObjID;
            this.WorkObjInfo = inWorkObjInfo;
            this.Attributes = attributes;
        }
    }
}
