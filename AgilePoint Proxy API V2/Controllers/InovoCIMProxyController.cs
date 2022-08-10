using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AgilePoint_Proxy_API_V2.Controllers
{
    [ApiController]
    [Route("api")]
    public class InovoCIMProxyController : ControllerBase
    {
        private readonly ILogger<InovoCIMProxyController> _logger;
        public IConfiguration configuration;
        private IDataService _data;
        private string agileUsername;
        private string agilePassword;
        private string agileProcessName;
        private string CreateProcessInstanceURL;
        private string UUIDURL;
        private string PIDURL;
        private string GetProcInstAttrsURL;

        public InovoCIMProxyController(ILogger<InovoCIMProxyController> logger, IConfiguration config, DataService data)
        {
            _logger = logger;
            this.configuration = config;
            this.agileUsername = (configuration.GetSection("AgileUser").GetSection("Username").Value);
            this.agilePassword = (configuration.GetSection("AgileUser").GetSection("Password").Value);
            this.agileProcessName = (configuration.GetSection("ProcessName").GetSection("ProcessName").Value);
            this.CreateProcessInstanceURL = (configuration.GetSection("URLs").GetSection("BaseURL").Value) +
                                            (configuration.GetSection("URLs").GetSection("ProcessInstanceURL").Value);
            this.UUIDURL = (configuration.GetSection("URLs").GetSection("BaseURL").Value) +
                           (configuration.GetSection("URLs").GetSection("UUIDURL").Value);
            this.PIDURL = (configuration.GetSection("URLs").GetSection("BaseURL").Value) +
                          (configuration.GetSection("URLs").GetSection("PIDURL").Value);
            this.GetProcInstAttrsURL = (configuration.GetSection("URLs").GetSection("BaseURL").Value) +
                          (configuration.GetSection("URLs").GetSection("GetProcInstAttrs").Value);
            _data = data;
        }

        [HttpGet]
        [Route("createprocinstob")]
        public async Task<IActionResult> GetProcInstAttrOB(int SOURCEID, int Login, int Serviceid, int Loadid, string ProcName, int ContactID)
        {
            try
            {
                string agileProcessInstanceData = JsonConvert.SerializeObject(await CreateInstance(SOURCEID, ProcName, Serviceid, Loadid, Login, ContactID));
                Thread.Sleep(1000);
                string workObjectUUID =
                    (JsonConvert.DeserializeObject<Dictionary<string, string>>(agileProcessInstanceData))["WorkObjectID"];
                string uri = @"http://10.21.160.106:13490/AgilePointServer/Workflow/GetCustomAttrsbyID/" +
                             workObjectUUID;

                bool hasURL = false;
                string sRedirectURL = String.Empty;



                while (!hasURL)
                {
                    HTTPOperations ops = new HTTPOperations("wfs-hcc", this.agileUsername,
                        this.agilePassword, ProcName, "en-US");
                    string varData = ops.GetData(uri);
                    string xmlData =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(varData)["GetCustomAttrsByIDResult"];
                    XmlDocument xmlDoc = new XmlDocument();
                    XmlDocument xmlDoc2 = new XmlDocument();
                    xmlDoc.LoadXml(xmlData);
                    string json = JsonConvert.SerializeXmlNode(xmlDoc);
                    string jsonConvert = (JsonConvert.DeserializeObject<JObject>(json)).ToString();
                    var jo = JObject.Parse(jsonConvert);
                    var XmlData2 = jo["ArrayOfNameValue"]["NameValue"].Last.Last.Last.Last.Last;
                    xmlDoc2.LoadXml(XmlData2.ToString());
                    string json2 = JsonConvert.SerializeXmlNode(xmlDoc2);
                    jo = JObject.Parse(json2);
                    sRedirectURL = jo["pd:AP"]["pd:processFields"]["pd:sRedirectURL"].ToString();
                    hasURL = !String.IsNullOrEmpty(sRedirectURL);
                }
                return Redirect(sRedirectURL);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _logger.LogError("Error: " + e.Message);
                return Content("Error with request: " + e.Message + "\r\n" + e.StackTrace);
            }

            
        }

        [HttpGet]
        [Route("createprocinstib")]
        public async Task<IActionResult> GetProcInstAttrIB(int SOURCEID, int Login, int Serviceid, string ProcName, int ContactID)
        {
            try
            {
                string agileProcessInstanceData = JsonConvert.SerializeObject(await CreateInstanceIB(SOURCEID, ProcName, Serviceid, Login, ContactID));
                Thread.Sleep(500);
                string workObjectUUID =
                    (JsonConvert.DeserializeObject<Dictionary<string, string>>(agileProcessInstanceData))["WorkObjectID"];
                string uri = @"http://10.21.160.106:13490/AgilePointServer/Workflow/GetCustomAttrsbyID/" +
                             workObjectUUID;

                bool hasURL = false;
                string sRedirectURL = String.Empty;



                while (!hasURL)
                {
                    HTTPOperations ops = new HTTPOperations("wfs-hcc", this.agileUsername,
                        this.agilePassword, ProcName, "en-US");
                    string varData = ops.GetData(uri);
                    string xmlData =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(varData)["GetCustomAttrsByIDResult"];
                    XmlDocument xmlDoc = new XmlDocument();
                    XmlDocument xmlDoc2 = new XmlDocument();
                    xmlDoc.LoadXml(xmlData);
                    string json = JsonConvert.SerializeXmlNode(xmlDoc);
                    string jsonConvert = (JsonConvert.DeserializeObject<JObject>(json)).ToString();
                    var jo = JObject.Parse(jsonConvert);
                    var XmlData2 = jo["ArrayOfNameValue"]["NameValue"].Last.Last.Last.Last.Last;
                    xmlDoc2.LoadXml(XmlData2.ToString());
                    string json2 = JsonConvert.SerializeXmlNode(xmlDoc2);
                    jo = JObject.Parse(json2);
                    sRedirectURL = jo["pd:AP"]["pd:processFields"]["pd:sRedirectURL"].ToString();
                    hasURL = !String.IsNullOrEmpty(sRedirectURL);
                }
                return Redirect(sRedirectURL);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _logger.LogError("Error: " + e.Message);
                return Content("Error with request: " + e.Message + "\r\n" + e.StackTrace);
            }


        }

        private async Task<AgileProcess> CreateInstance(int SOURCEID, string AgileProcessName, int SERVICEID, int LOADID, int LOGIN, int CONTACTID)
        {
            try
            {
                #region [ Declarations ]

                HTTPOperations ops = new HTTPOperations("wfs-hcc", this.agileUsername, this.agilePassword,
                    AgileProcessName, "en-US");

                #endregion

                #region [ Get Requests ]

                string ProcInstUUID =
                    (JsonConvert.DeserializeObject<Dictionary<string, string>>(ops.GetData(this.UUIDURL)))[
                        "GetUUIDResult"];
                string CustomUUID =
                    (JsonConvert.DeserializeObject<Dictionary<string, string>>(ops.GetData(this.UUIDURL)))[
                        "GetUUIDResult"];
                string ReleasedPID =
                    (JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        ops.GetData(this.PIDURL + AgileProcessName)))["GetReleasedPIDResult"];

                #endregion

                #region [ Get Proc Inst Attrs ]

                //CustomAtt[] atts = { new CustomAtt(@"iSourceID", SOURCEID.ToString()) };

                List<CustomAtt> attsList = new List<CustomAtt>();

                var dyn = await _data.SelectMany<dynamic, dynamic>("EXEC [API].[spGetProcInstVarsOB] @INSOURCEID, @INSERVICEID, @INLOADID, @INLOGIN, @INCONTACTID", new { INSOURCEID = SOURCEID, INSERVICEID = SERVICEID, INLOADID = LOADID, INLOGIN = LOGIN, INCONTACTID = CONTACTID}, "UI");

                if(dyn != null)
                {
                    attsList.Add(new CustomAtt(@"iSourceID", SOURCEID.ToString()));
                    foreach (IDictionary<string, object> row in dyn)
                    {
                        foreach (var pair in row)
                        {
                            attsList.Add(new CustomAtt() { Name = pair.Key, Value = pair.Value.ToString() });
                        }
                    }
                }

                #region [ Get Mappings ]
                string queryMap = $"EXEC [API].[spGetAttributeMap]";
                var dynamicMapping = await _data.SelectMany<dynamic, dynamic>(queryMap, new { }, "UI");
                Dictionary<string, string> map = new();
                foreach (var pair in dynamicMapping)
                {
                    map.Add(pair.AttributeName, pair.AgilePointName);
                }
                #endregion

                string queryAGPEvent = $"EXEC [API].[spGetAdditionalAttributesSRCID] @INSOURCEID";
                var dynamicAGPEvent = await _data.SelectMany<dynamic, dynamic>(queryAGPEvent, new { INSOURCEID = SOURCEID }, "UI");
                if (dynamicAGPEvent.Count > 0)
                {
                    foreach (var itemNew in dynamicAGPEvent)
                    {
                        var checknew = itemNew.ValueString;
                        var tempObjNew = Newtonsoft.Json.JsonConvert.DeserializeObject(itemNew.ValueString);
                        foreach (var pairNew in tempObjNew)
                        {
                            if (map.ContainsKey(pairNew.Name))
                            {
                                string ConvertedName = map[pairNew.Name];
                                attsList.Add(new CustomAtt() { Name = ConvertedName, Value = pairNew.Value.ToString() });
                                //var temp = pair;

                                //dynamicResult.Add(temp);
                            }
                            else
                            {
                                attsList.Add(new CustomAtt() { Name = pairNew.Name.ToString(), Value = pairNew.Value.ToString() });
                            }
                        }
                    }
                }

                string queryAttributes = $"EXEC [API].[spGetAccountAttributesBySourceID] @INSOURCEID";
                var dynamicAttributes = await _data.SelectMany<dynamic, dynamic>(queryAttributes, new { INSOURCEID = SOURCEID }, "UI");
                if (dynamicAttributes.Count > 0)
                {
                    foreach (var item in dynamicAttributes)
                    {
                        var checknew = item.ValueString;
                        var tempObjNew = Newtonsoft.Json.JsonConvert.DeserializeObject(item.ValueString);
                        foreach (var pair in tempObjNew)
                        {
                            if (map.ContainsKey(pair.Name))
                            {
                                string ConvertedName = map[pair.Name];
                                attsList.Add(new CustomAtt(ConvertedName, pair.Value.ToString()));
                                //var temp = pair;

                                //dynamicResult.Add(temp);
                            }
                            else
                            {
                                //tempAccList.Add(new KVPair(pair.Name, pair.Value.ToString()));
                            }
                        }
                    }
                }

                CustomAtt[] atts = attsList.ToArray();
                #endregion

                #region [ Set Process Variables ]

                ProcessVariables procVar = new ProcessVariables(
                    CustomUUID,
                    "wfs-hcc\\" + this.agileUsername,
                    ReleasedPID,
                    ProcInstUUID,
                    AgileProcessName + "-" + ProcInstUUID,
                    "" + null + "\\",
                    CustomUUID,
                    "" + null,
                    atts);
                string CreateProcessJson = JsonConvert.SerializeObject(procVar);

                #endregion

                #region [ POSTs ]

                string ReturnCreate = ops.POSTMethod(this.CreateProcessInstanceURL, CreateProcessJson);

                #endregion

                return new AgileProcess
                {
                    ProcessInstanceID = procVar.ProcessInstID, ProcessName = AgileProcessName,
                    WorkObjectID = CustomUUID
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<AgileProcess> CreateInstanceIB(int SOURCEID, string AgileProcessName, int SERVICEID, int Login, int ContactID)
        {
            try
            {
                #region [ Declarations ]

                HTTPOperations ops = new HTTPOperations("wfs-hcc", this.agileUsername, this.agilePassword,
                    AgileProcessName, "en-US");

                #endregion

                #region [ Get Requests ]

                string ProcInstUUID =
                    (JsonConvert.DeserializeObject<Dictionary<string, string>>(ops.GetData(this.UUIDURL)))[
                        "GetUUIDResult"];
                string CustomUUID =
                    (JsonConvert.DeserializeObject<Dictionary<string, string>>(ops.GetData(this.UUIDURL)))[
                        "GetUUIDResult"];
                string ReleasedPID =
                    (JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        ops.GetData(this.PIDURL + AgileProcessName)))["GetReleasedPIDResult"];

                #endregion

                #region [ Get Proc Inst Attrs ]

                //CustomAtt[] atts = { new CustomAtt(@"iSourceID", SOURCEID.ToString()) };
                List<CustomAtt> attsList = new List<CustomAtt>();

                var dyn = await _data.SelectMany<dynamic, dynamic>("EXEC [API].[spGetProcInstVarsIB] @INSOURCEID, @INSERVICEID, @INLOGIN, @INCONTACTID", new { INSOURCEID = SOURCEID, INSERVICEID = SERVICEID, INLOGIN = Login, INCONTACTID = ContactID}, "UI");

                if (dyn != null)
                {
                    attsList.Add(new CustomAtt(@"iSourceID", SOURCEID.ToString()));
                    foreach (IDictionary<string, object> row in dyn)
                    {
                        foreach (var pair in row)
                        {
                            attsList.Add(new CustomAtt() { Name = pair.Key, Value = pair.Value.ToString() });
                        }
                    }
                }
                CustomAtt[] atts = attsList.ToArray();
                #endregion

                #region [ Set Process Variables ]

                ProcessVariables procVar = new ProcessVariables(
                    CustomUUID,
                    "wfs-hcc\\" + this.agileUsername,
                    ReleasedPID,
                    ProcInstUUID,
                    AgileProcessName + "-" + ProcInstUUID,
                    "" + null + "\\",
                    CustomUUID,
                    "" + null,
                    atts);
                string CreateProcessJson = JsonConvert.SerializeObject(procVar);

                #endregion

                #region [ POSTs ]

                string ReturnCreate = ops.POSTMethod(this.CreateProcessInstanceURL, CreateProcessJson);

                #endregion

                return new AgileProcess
                {
                    ProcessInstanceID = procVar.ProcessInstID,
                    ProcessName = AgileProcessName,
                    WorkObjectID = CustomUUID
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<bool> CancelInstance(string UN, string PW, AgileProcess inProcess,
            string CancelProcessInstanceURL)
        {
            try
            {
                #region [ Declarations ]

                HTTPOperations ops = new HTTPOperations("wfs-hcc", UN, PW, inProcess.ProcessName, "en-US");

                #endregion

                #region [ POSTs ]

                string ReturnCancel = ops.POSTMethod(CancelProcessInstanceURL + inProcess.ProcessInstanceID, "");

                #endregion

                //Ek wil hier iets insit wat check dat die process wel ge-kanselleer is
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<string> CreateEntry(string inString, string Type, string Name)
        {
            string outString = String.Empty;
            outString += "{\"Name\":\"/pd:AP/pd:processFields/pd:" + Name;
            outString += "\", \"Value\":";
            outString += (Type.ToLower()) switch
            {
                ("string") => "\"" + inString + "\"",
                ("date") => "\"" + inString + "\"",
                ("datetime") => "\"" + inString + "\"",
                ("time") => "\"" + inString + "\"",
                _ => inString,
            };
            return outString;
        }
    }
}