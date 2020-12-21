using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;

using OSIsoft.AF.PI;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Time;


namespace GCWebAPI.Controllers
{

    public class TSOpsController : ApiController
    {
        // GET api/TSOps
        /// <summary>
        /// Default Controller
        /// </summary>
        /// <remarks>No action is taken against the backend server</remarks>
        /// <returns>String with developer's name</returns>
        [HttpGet]
        public string Get()
        {
            return "GCWebAPI developed by Gustavo Chermont";
        }

        //GET api/tsops/Point?server=<serverName>&point=<pointName>
        /// <summary>
        /// Gets Point configuration
        /// </summary>
        /// <returns>Returns the PI Point attributes</returns>
        [HttpGet]
        [ActionName("Point")]
        public PIPoint GetPointConfig([FromUri]string server, [FromUri]string point)
        {
            PIServer srv;

            if (server != null)
            {
                try
                {
                    srv = new PIServers()[server];
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

            PIPoint requestedPoint = PIPoint.FindPIPoint(srv, point);


            return requestedPoint;
        }


        // GET api/tsops/Snapshot?server=<serverName>&point=<pointName>
        /// <summary>
        /// Gets current value for PI Point
        /// </summary>
        /// <returns>Returns current value and timestamp for PI Point</returns>
        [HttpGet]
        [ActionName("Snapshot")]
        public IEnumerable<string> GetSnapshot([FromUri]string server, [FromUri]string point)
        {
            PIServer srv;

            //Instantiates PI Data Archive
            if (server != null)
            {
                try
                {
                    srv = new PIServers()[server];
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
                PIPoint requestedPoint = PIPoint.FindPIPoint(srv, point);

                AFValue val = requestedPoint.CurrentValue();

                IEnumerable<string> result = new string[] { "Server: " + val.PIPoint.Server, "Point Name : " + val.PIPoint.Name, "Current Value: " + val.Value, "Timestamp: " + val.Timestamp };

                return result;
            }
            catch
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest); //Server is null
            }
        }



        /// <summary>
        /// Creates PI Point
        /// </summary>
        /// <returns>Returns status 201 if successful</returns>
        [HttpPost]
        [ActionName("Point")]
        // POST api/TSOps/CreatePoint?server=<server>
        public HttpResponseMessage PostCreatePIPoint([FromUri]string server, [FromBody]JObject data)
        {
            PIServer srv;

            //Instantiates PI Data Archive
            if (server != null)
            {
                try
                {
                    srv = new PIServers()[server];
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


            IDictionary<string, object> attributes = new Dictionary<string, object>();

            if (data["PointSource"] != null) { attributes.Add("PointSource", data["PointSource"].ToString()); }
            if (data["EngUnits"] != null) { attributes.Add("EngUnits", data["EngUnits"].ToString()); }
            if (data["Future"] != null) { attributes.Add("Future", data["Future"].ToString()); }
            if (data["PointType"] != null) { attributes.Add("PointType", data["PointType"].ToString()); }
            if (data["Description"] != null) { attributes.Add("Descriptor", data["Description"].ToString()); }
            if (data["Location1"] != null) { attributes.Add("Location1", data["Location1"].ToString()); }
            if (data["Location2"] != null) { attributes.Add("Location2", data["Location2"].ToString()); }
            if (data["Location3"] != null) { attributes.Add("Location3", data["Location3"].ToString()); }
            if (data["Location4"] != null) { attributes.Add("Location4", data["Location4"].ToString()); }
            if (data["Location5"] != null) { attributes.Add("Location5", data["Location5"].ToString()); }
            if (data["InstrumentTag"] != null) { attributes.Add("InstrumentTag", data["InstrumentTag"].ToString()); }


            try
            {
                PIPoint newPoint = srv.CreatePIPoint(data["Name"].ToString(), attributes);
                return new HttpResponseMessage(HttpStatusCode.Created);

            }
            catch
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest); //tag could not be created
            }
        }

        //Request Body Example - Not all attributes were implemented

        //{
        //    "Name" : "<string>",
        //    "PointSource" : "<string>",
        //    "EngUnits" : "<string>",
        //    "Future" : "<bool>",
        //    "PointType" : "<string>",
        //    "Description" : "<string>",
        //    "Location1" : "<int>",
        //    "Location2" : "<int>",
        //    "Location3" : "<int>",
        //    "Location4" : "<int>",
        //    "Location5" : "<int>",
        //    "InstrumentTag" : "<string>"
        //}

        /// <summary>
        /// Updates PI Point current value
        /// </summary>
        /// <returns>Returns the previous and new value and timestamp</returns>
        [HttpPost]
        [ActionName("Snapshot")]
        // POST api/TSOps/CreatePoint?server=<serverName>&point=<pointName>
        public IEnumerable<string> UpdateSnapshot([FromUri]string server, [FromUri]string point, [FromBody]JObject data)
        {
            PIServer srv;

            //Instantiates PI Data Archive
            if (server != null)
            {
                try
                {
                    srv = new PIServers()[server];
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
            catch
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest); //Server is null
            }

        }

        //Request Body Example 

        //{
        //    "Value" : "<string>",
        //    "Timestamp" : "<string>",
        //}





    }
}