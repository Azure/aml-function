using System;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Net;
using System.Collections.Specialized;

public static class GridEventHandler
{

    private static bool isValidRequestObject(dynamic requestObject)
    {
        if (requestObject == null || requestObject[0] == null
            || requestObject[0]["eventType"] == null || requestObject[0]["data"] == null)
            return false;

        return true;
    }

    private static string ParseEventGridValidationCode(dynamic requestObject)
    {
        var webhook_res = string.Empty;
        if (requestObject != null && requestObject[0]["data"] != null)
        {
            var validationCode = requestObject[0].data.validationCode;
            if (validationCode != null)
            {
                webhook_res = Newtonsoft.Json.JsonConvert.SerializeObject(new Newtonsoft.Json.Linq.JObject { ["validationResponse"] = validationCode });
            }
        }
        return webhook_res;
    }

    private static HttpResponseMessage createHttpResponse(HttpStatusCode statusCode, string content)
    {
        HttpResponseMessage response = new HttpResponseMessage(statusCode);
        response.Content = new StringContent(content);
        return response;
    }

    private static string getEventSource(string current_event)
    {
        string[] event_data = current_event.Split('.');
        return event_data.ElementAtOrDefault(1);
    }

    private static string getEventType(string current_event)
    {
        string event_type = string.Empty;
        string[] event_data = current_event.Split('.');
        event_type = getEventSource(current_event);

        for (int index = 2; index < event_data.Length; index++)
        {
            event_type += "-" + event_data[index];
        }
        return event_type.ToLower();
    }

    private static object getRequestDataFromRequestObject(string event_source, dynamic requestObject)
    {
        var req_data = requestObject;

        if (!string.IsNullOrEmpty(event_source) && requestObject != null && requestObject[0] != null && requestObject[0]["data"] != null)
        {
            req_data = requestObject[0]["data"];
        }
        else
        {
            return null;
        } 

        switch (event_source)
        {
            case "AppConfiguration":
                req_data = ParseAppConfigurationEvent(requestObject);
                break;

            case "Web/sites":
                req_data = ParseAppServiceEvent(requestObject);
                break;

            case "Storage":
                req_data = ParseBlobStorageEvent(requestObject);
                break;

            case "ContainerRegistry":
                req_data = ParseContainerRegistryEvent(requestObject);
                break;

            case "EventHub":
                req_data = ParseEventHubEvent(requestObject);
                break;

            case "Devices":
                req_data = ParseIoTDevicesEvent(requestObject);
                break;

            case "KeyVault":
                req_data = ParseKeyVaultEvent(requestObject);
                break;

            case "MachineLearningServices":
                req_data = ParseMachineLearningEvent(requestObject);
                break;

            case "Maps":
                req_data = ParseMapsEvent(requestObject);
                break;

            case "Media":
                req_data = ParseMediaEvent(requestObject);
                break;

            case "Resources":
                req_data = ParseResourcesEvent(requestObject);
                break;

            case "ServiceBus":
                req_data = ParseServiceBusEvent(requestObject);
                break;

            case "SignalRService":
                req_data = ParseSignalRServiceEvent(requestObject);
                break;

            default:
                return null;

        }

        //Handling the scenario if data is string instead of Json
        string req_type = req_data.GetType().ToString();
        if (req_type == "Newtonsoft.Json.Linq.JValue")
        {
            String tmp = req_data.ToString();
            req_data = JsonConvert.DeserializeObject(tmp);
        }

        return req_data;
    }


    private static dynamic ParseAppConfigurationEvent(dynamic requestObject)
    {
        return requestObject[0]["data"];
    }
    private static dynamic ParseAppServiceEvent(dynamic requestObject)
    {
        return requestObject[0]["data"];
    }
    private static dynamic ParseBlobStorageEvent(dynamic requestObject)
    {
        return requestObject[0]["data"];
    }
    private static dynamic ParseContainerRegistryEvent(dynamic requestObject)
    {
        return requestObject[0]["data"];
    }
    private static dynamic ParseEventHubEvent(dynamic requestObject)
    {
        return requestObject[0]["data"];
    }
    private static dynamic ParseIoTDevicesEvent(dynamic requestObject)
    {
        return requestObject[0]["data"];
    }
    private static dynamic ParseKeyVaultEvent(dynamic requestObject)
    {
        return requestObject[0]["data"];
    }
    private static dynamic ParseMachineLearningEvent(dynamic requestObject)
    {
        return requestObject[0]["data"];
    }
    private static dynamic ParseMapsEvent(dynamic requestObject)
    {
        return requestObject[0]["data"];
    }
    private static dynamic ParseMediaEvent(dynamic requestObject)
    {
        return requestObject[0]["data"];
    }
    private static dynamic ParseResourcesEvent(dynamic requestObject)
    {
        return requestObject[0]["data"];
    }
    private static dynamic ParseServiceBusEvent(dynamic requestObject)
    {
        return requestObject[0]["data"];
    }
    private static dynamic ParseSignalRServiceEvent(dynamic requestObject)
    {
        return requestObject[0]["data"];
    }

    [FunctionName("generic_triggers")]
    public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, ILogger log, ExecutionContext context)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string requestBody = await req.Content.ReadAsStringAsync();
        dynamic requestObject = JsonConvert.DeserializeObject(requestBody);

        if (!isValidRequestObject(requestObject))
        {
            log.LogInformation("Request object does not contain required data");
            return createHttpResponse(HttpStatusCode.BadRequest, "Unable to process the request");
        }

        string current_event = requestObject[0]["eventType"].ToString();

        //acknowledge if this is a subscription event
        if (current_event == "Microsoft.EventGrid.SubscriptionValidationEvent")
        {
            string webhook_res = ParseEventGridValidationCode(requestObject);
            if (!string.IsNullOrEmpty(webhook_res))
            {
                return createHttpResponse(HttpStatusCode.OK, webhook_res);
            }
        }

        string event_type = getEventType(current_event);
        log.LogInformation("event type : " + event_type);

        string event_source = getEventSource(current_event);
        log.LogInformation("event source : " + event_source);

        var uri = new Uri(req.RequestUri.ToString());
        NameValueCollection queryParams = uri.ParseQueryString();

        string repo_name = queryParams.Get("repoName");

        log.LogInformation("fetching repo name from query parameters: " + repo_name);


        if (!string.IsNullOrEmpty(repo_name))
        {
            var req_data = getRequestDataFromRequestObject(event_source, requestObject);
            if (req_data == null)
            {
                log.LogInformation("Request object does not contain required data");
                return createHttpResponse(HttpStatusCode.BadRequest, "Unable to process the request");
            }

            using (var httpClient = new System.Net.Http.HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Awesome-Octocat-App");
                httpClient.DefaultRequestHeaders.Accept.Clear();

                var PATTOKEN = Environment.GetEnvironmentVariable("PAT_TOKEN", EnvironmentVariableTarget.Process);
                
                if(PATTOKEN == null)
                {
                    log.LogInformation("PATTOKEN not provided");
                    return createHttpResponse(HttpStatusCode.BadRequest, "Unable to process the request");
                }

                httpClient.DefaultRequestHeaders.Add("Authorization", "token " + PATTOKEN);

                var client_payload = new Newtonsoft.Json.Linq.JObject
                {
                    ["unit "] = false,
                    ["integration"] = true,
                    ["data"] = req_data,
                    ["event_source"] = event_source
                };

                var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new Newtonsoft.Json.Linq.JObject { ["event_type"] = event_type, ["client_payload"] = client_payload });

                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync("https://api.github.com/repos/" + repo_name + "/dispatches", content);

                if(response.StatusCode != HttpStatusCode.OK)
                {
                    log.LogInformation(response.StatusCode + " Unable to process the request: " + await response.Content.ReadAsStringAsync());
                    return createHttpResponse(response.StatusCode, "Unable to process the request: " + event_type);
                }

                log.LogInformation("response from github " + await response.Content.ReadAsStringAsync());

                log.LogInformation("dispatched "+ event_type);
                return createHttpResponse(HttpStatusCode.OK, "dispatched "+ event_type );
            }
        }
        else
        {
            log.LogInformation("query param 'repoName' not provided");
            return createHttpResponse(HttpStatusCode.BadRequest, "Unable to process the request");
        }
    }
}
