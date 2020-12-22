using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using OSIsoft.AF.PI;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Time;


namespace GCWebAPI.Controllers
{

    public class TSOpsController : ApiController
    {
        
        /// <summary>
        /// Default Controller
        /// </summary>
        /// <remarks>No action is taken against the backend server</remarks>
        /// <returns>String with developer's name</returns>
        [HttpGet]
        // GET api/TSOps
        public string Get()
        {
            return "GCWebAPI developed by Gustavo Chermont";
        }
        
        /// <summary>
        /// Gets Point configuration
        /// </summary>
        /// <returns>Returns all PI Point attributes</returns>
        [HttpGet]
        [ActionName("Point")]
        //GET api/tsops/Point?server={serverName}&point={pointName}
        public List<string> GetPointConfig([FromUri]string server, [FromUri]string point)
        {
            PIServer srv;

            if (server != null)
            {
                try
                {
                    srv = new PIServers()[server];
                    srv.Connect();
                }
                catch
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest); //Server not found
                }
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest); //Server is null
            }

            PIPoint requestedPoint;

            try
            {
                requestedPoint = PIPoint.FindPIPoint(srv, point); //Finds desired PI Point               
            }
            catch
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest); //PI Point not found
            }

            IDictionary<string, object> attributes = requestedPoint.GetAttributes(); //Gets all PI Point attributes

            List<string> attributeList = new List<string>();

            attributeList.Add("Server : " + requestedPoint.Server.Name); //Adds the server name to the Result list

            foreach (KeyValuePair<string, object> pair in attributes)
            {
                attributeList.Add(pair.Key + ":" + pair.Value); //Converts Idictionary to List 
            }

            return attributeList;
        }
        
        
        /// <summary>
        /// Gets current value for PI Point
        /// </summary>
        /// <returns>Returns current value and timestamp for PI Point</returns>
        [HttpGet]
        [ActionName("Snapshot")]
        // GET api/tsops/Snapshot?server={serverName}&point={pointName>}
        public IEnumerable<string> GetSnapshot([FromUri]string server, [FromUri]string point)
        {
            PIServer srv;

            //Instantiates PI Data Archive
            if (server != null)
            {
                try
                {
                    srv = new PIServers()[server];
                    srv.Connect();
                }
                catch
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest); //Server not found
                }
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest); //Server is null

            }

            //Finds PI Point and gets current value
            try
            {
                PIPoint requestedPoint = PIPoint.FindPIPoint(srv, point); //Finds desired PI Point

                AFValue val = requestedPoint.CurrentValue(); //Gets current value

                IEnumerable<string> result = new string[] { "Server: " + val.PIPoint.Server, "Point Name : " + val.PIPoint.Name, "Current Value: " + val.Value, "Timestamp: " + val.Timestamp };

                return result;
            }
            catch (HttpResponseException e)
            {
                throw new HttpResponseException(e.Response); 
            }
        }
               
        /// <summary>
        /// Creates PI Point
        /// </summary>
        /// <returns>Returns status 201 if successful</returns>
        [HttpPost]
        [ActionName("Point")]
        // POST api/TSOps/Point?server={server}
        public HttpResponseMessage CreatePIPoint([FromUri]string server, [FromBody]JObject data)
        {
            PIServer srv;

            //Instantiates PI Data Archive
            if (server != null)
            {
                try
                {
                    srv = new PIServers()[server];
                    srv.Connect();
                }
                catch
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest); //Server not found
                }
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest); //Server is null
            }

            //New PI Point attribute collection
            IDictionary<string, object> attributes = new Dictionary<string, object>();

            //Classic PI Point attribute collection
            IDictionary<string, object> ptClassAttributes = srv.PointClasses["classic"].GetAttributes();


            foreach (KeyValuePair<string, JToken> pair in data)
            {
                //always create classic PI Points
                if(pair.Key.ToString() == "PointClass")
                    attributes.Add(pair.Key.ToString(), "Classic");
                else
                {
                    if (ptClassAttributes.ContainsKey(pair.Key.ToString())) //Confirms the attribute name exists
                        attributes.Add(pair.Key, pair.Value.ToString());
                }                
            }

            try
            {
                PIPoint newPoint = srv.CreatePIPoint(data["Name"].ToString(), attributes); //Create PI Point
                return new HttpResponseMessage(HttpStatusCode.Created);

            }
            catch(HttpResponseException e)
            {
                throw new HttpResponseException(e.Response); //tag could not be created
            }
            catch(PIException e)
            {
                throw new Exception(e.Message); //tag could not be created - Most likely attributes are not set correctly
            }
        }

        //Request Body Example - All classic attributes can be used

        //{
        //    "Name" : "<string>",
        //    "PointSource" : "<string>",
        //    "EngUnits" : "<string>",
        //    "Future" : "<string>",
        //    "PointType" : "<string>",
        //    "Location1" : "<string>",
        //    "Location2" : "<string>",
        //    "Location3" : "<string>",
        //    "Location4" : "<string>",
        //    "Location5" : "<string>",
        //    "InstrumentTag" : "<string>"
        //}

        /// <summary>
        /// Updates PI Point current value
        /// </summary>
        /// <returns>Returns the previous and new values and timestamps</returns>
        [HttpPost]
        [ActionName("Snapshot")]
        // POST api/TSOps/Snapshot?server={serverName}&point={pointName}
        public IEnumerable<string> UpdateSnapshot([FromUri]string server, [FromUri]string point, [FromBody]JObject data)
        {
            PIServer srv;

            //Instantiates PI Data Archive
            if (server != null)
            {
                try
                {
                    srv = new PIServers()[server];
                    srv.Connect();
                }
                catch
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest); //Server not found
                }
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest); //Server is null
            }

            AFValue newVal = new AFValue();
            newVal.Value = data["Value"].ToString();

            AFTime time = new AFTime(data["Timestamp"].ToString());
            newVal.Timestamp = time;


            //Finds PI Point and gets current value
            try
            {
                PIPoint requestedPoint = PIPoint.FindPIPoint(srv, point);

                AFValue currVal = requestedPoint.CurrentValue();

                requestedPoint.UpdateValue(newVal, OSIsoft.AF.Data.AFUpdateOption.Insert);

                IEnumerable<string> result = new string[] { "Server: " + requestedPoint.Server.Name, "Point Name : " + requestedPoint.Name, "Previous Value: " + currVal.Value, "Previous Timestamp: " + currVal.Timestamp, "New Value: " + newVal.Value, "new Timestamp: " + newVal.Timestamp };

                return result;
            }
            catch (HttpResponseException e)
            {
                throw new HttpResponseException(e.Response); 
            }

        }

        //Request Body Example 

        //{
        //    "Value" : "<value>",
        //    "Timestamp" : "<PI Time value>",
        //}
    }
}