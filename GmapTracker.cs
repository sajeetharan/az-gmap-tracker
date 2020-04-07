using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace GmapTracker
{
    public static class GmapTracker
    {
        [FunctionName("Tracker")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            string latlong = req.GetQueryNameValuePairs().FirstOrDefault(q => string.Compare(q.Key, "latlong", true) == 0).Value;

            dynamic data = await req.Content.ReadAsAsync<object>();
            latlong = latlong ?? data?.latlong;

            var URI = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + latlong + "&key=add your key";

            // call Google API
            var request = System.Net.WebRequest.Create(URI) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = 0;
            WebResponse response = request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string json = reader.ReadToEnd();
            log.Info(json);

            dynamic jsonData = JsonConvert.DeserializeObject(json);

            return latlong == null ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a valid latitude and longitude as a string") : req.CreateResponse(HttpStatusCode.OK, new
            {
                formatted_address = $"{jsonData.results[0].formatted_address}"
            }
           );
        }
    }
}
