using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.Management;
using static System.Net.WebRequestMethods;

namespace ModelsLibrary
{
    public static class SomiodConstants
    {
        public const string baseUrlSomiod = "http://localhost:44316/api/somiod";

        public const string defaultBrokerAddress = "mqtt://test.mosquitto.org";

        public const int defaultBrokerPort = 1883;

        public const string applicationName = "a";

    }
    public class SomiodApiClient
    {
        private readonly string baseUrl;
        private RestClient clientRest;

        public SomiodApiClient(string baseUrl)
        {
            this.baseUrl = baseUrl.TrimEnd('/');
            clientRest = new RestClient(this.baseUrl);
        }

        #region Application Operations

        public ApiResponse<Application> CreateApplication(string appName)
        {
            // First check if the application already exists
            var existingApp = GetApplication(appName);
            if (existingApp.IsSuccess)
            {
                return new ApiResponse<Application>
                {
                    IsSuccess = false,
                    StatusCode = 409, // Conflict
                    Data = existingApp.Data,
                    Message = $"Application '{appName}' already exists."
                };
            }

            // Application doesn't exist, proceed with creation
            var request = new RestRequest($"/", Method.Post);
            Application application = new Application
            {
                ResourceName = appName,
                ResType = "application"
            };
            
            // Serialize to JSON and add as body
            string jsonBody = JsonConvert.SerializeObject(application);
            request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
            
            var response = clientRest.Execute(request);

            if (response.IsSuccessful)
            {
                return new ApiResponse<Application>
                {
                    IsSuccess = true,
                    StatusCode = (int)response.StatusCode,
                    Data = application,
                    Message = "Application created successfully."
                };
            }
            else
            {
                return new ApiResponse<Application>
                {
                    IsSuccess = false,
                    StatusCode = (int)response.StatusCode,
                    Data = null,
                    Message = $"Failed to create application. Status Code: {response.StatusCode}, Error: {response.ErrorMessage}"
                };
            }
        }

        public ApiResponse<Application> GetApplication(string appName)
        {
            var request = new RestRequest($"/{appName}/", Method.Get);
            var response = clientRest.Execute<Application>(request);

            if (response.IsSuccessful && response.Data != null)
            {
                return new ApiResponse<Application>
                {
                    IsSuccess = true,
                    StatusCode = (int)response.StatusCode,
                    Data = response.Data,
                    Message = "Application retrieved successfully."
                };
            }
            else
            {
                return new ApiResponse<Application>
                {
                    IsSuccess = false,
                    StatusCode = (int)response.StatusCode,
                    Data = null,
                    Message = $"Application not found. Status Code: {response.StatusCode}"
                };
            }
        }

        public ApiResponse<List<string>> GetApplicationContainers(string appName)
        {
            var request = new RestRequest($"/{appName}/", Method.Get);
            request.AddHeader("somiod-discovery", "container");
            var response = clientRest.Execute(request);
            if (response.IsSuccessful)
            {
                var containers = JsonConvert.DeserializeObject<List<string>>(response.Content);
                return new ApiResponse<List<string>>
                {
                    IsSuccess = true,
                    StatusCode = (int)response.StatusCode,
                    Data = containers,
                    Message = "Containers retrieved successfully."
                };
            }
            else
            {
                return new ApiResponse<List<string>>
                {
                    IsSuccess = false,
                    StatusCode = (int)response.StatusCode,
                    Data = null,
                    Message = $"Failed to retrieve containers. Status Code: {response.StatusCode}, Error: {response.ErrorMessage}"
                };
            }
        }

        public ApiResponse<bool> DeleteApplication(string appName)
        {
            var request = new RestRequest($"/{appName}/", Method.Delete);
            var response = clientRest.Execute(request);

            if (response.IsSuccessful)
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = true,
                    StatusCode = (int)response.StatusCode,
                    Data = true,
                    Message = "Application deleted successfully."
                };
            }
            else
            {
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    StatusCode = (int)response.StatusCode,
                    Data = false,
                    Message = $"Failed to delete application. Status Code: {response.StatusCode}, Error: {response.ErrorMessage}"
                };
            }
        }

        #endregion

        #region Container Operations

        public ApiResponse<Container> CreateContainer(string appName, string containerName)
        {
            var request = new RestRequest($"/{appName}/", Method.Post);
            Container container = new Container
            {
                ResourceName = containerName,
                ResType = "container"
            };
            
            // Serialize to JSON and add as body
            string jsonBody = JsonConvert.SerializeObject(container);
            request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
            
            var response = clientRest.Execute(request);
            if (response.IsSuccessful)
            {
                return new ApiResponse<Container>
                {
                    IsSuccess = true,
                    StatusCode = (int)response.StatusCode,
                    Data = container,
                    Message = "Container created successfully."
                };
            }
            else
            {
                return new ApiResponse<Container>
                {
                    IsSuccess = false,
                    StatusCode = (int)response.StatusCode,
                    Data = null,
                    Message = $"Failed to create container. Status Code: {response.StatusCode}, Error: {response.ErrorMessage}"
                };
            }
        }

        #endregion

        #region Subscription Operations

        public ApiResponse<List<string>> GetContainerSubscriptions(string containerUrl)
        {
            var request = new RestRequest(containerUrl, Method.Get);
            request.AddHeader("somiod-discovery", "subscription");
            var response = clientRest.Execute(request);
            if (response.IsSuccessful)
            {
                var subscriptions = JsonConvert.DeserializeObject<List<string>>(response.Content);
                return new ApiResponse<List<string>>
                {
                    IsSuccess = true,
                    StatusCode = (int)response.StatusCode,
                    Data = subscriptions,
                    Message = "Subscriptions retrieved successfully."
                };
            }
            else
            {
                return new ApiResponse<List<string>>
                {
                    IsSuccess = false,
                    StatusCode = (int)response.StatusCode,
                    Data = null,
                    Message = $"Failed to retrieve subscriptions. Status Code: {response.StatusCode}, Error: {response.ErrorMessage}"
                };
            }
        }

        public ApiResponse<string> GetSubscription(string subscriptionUrl)
        {
            var request = new RestRequest(subscriptionUrl, Method.Get);
            var response = clientRest.Execute(request);

            if (response.IsSuccessful)
            {
                return new ApiResponse<string>
                {
                    IsSuccess = true,
                    StatusCode = (int)response.StatusCode,
                    Data = response.Content,
                    Message = "Subscription retrieved successfully."
                };
            }

            return new ApiResponse<string>
            {
                IsSuccess = false,
                StatusCode = (int)response.StatusCode,
                Data = null,
                Message = response.ErrorMessage
            };
        }

        public ApiResponse<Subscription> CreateSubscription(string appName, string containerName, string subName)
        {

            var request = new RestRequest($"/{appName}/{containerName}", Method.Post);
            Subscription subscription = new Subscription
            {
                ResourceName = subName,
                ResType = "subscription",
                Evt = 1,
                Endpoint = SomiodConstants.defaultBrokerAddress + ":" + SomiodConstants.defaultBrokerPort
            };

            string jsonBody = JsonConvert.SerializeObject(subscription);
            request.AddJsonBody(jsonBody);

            var response = clientRest.Execute(request);
            if (response.IsSuccessful)
            {
                return new ApiResponse<Subscription>
                {
                    IsSuccess = true,
                    StatusCode = (int)response.StatusCode,
                    Data = subscription,
                    Message = "Subscription created successfully."
                };
            }
            else
            {
                return new ApiResponse<Subscription>
                {
                    IsSuccess = false,
                    StatusCode = (int)response.StatusCode,
                    Data = null,
                    Message = $"Failed to create subscription. Status Code: {response.StatusCode}, Error: {response.ErrorMessage}"
                };
            }
        }



        #endregion

        #region Content Instance Operations

        public ApiResponse<ContentInstance> CreateContentInstance(string appName, string containerName, ContentInstance ci)
        {
            var request = new RestRequest($"/{appName}/{containerName}/", Method.Post);
            
            // Set the res-type if not already set
            if (string.IsNullOrEmpty(ci.ResType))
            {
                ci.ResType = "content-instance";
            }
            
            // Serialize to JSON and add as body
            string jsonBody = JsonConvert.SerializeObject(ci);
            request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
            
            var response = clientRest.Execute(request);
            if (response.IsSuccessful)
            {
                return new ApiResponse<ContentInstance>
                {
                    IsSuccess = true,
                    StatusCode = (int)response.StatusCode,
                    Data = ci,
                    Message = "Content Instance created successfully."
                };
            }
            else
            {
                return new ApiResponse<ContentInstance>
                {
                    IsSuccess = false,
                    StatusCode = (int)response.StatusCode,
                    Data = null,
                    Message = $"Failed to create Content Instance. Status Code: {response.StatusCode}, Error: {response.ErrorMessage}"
                };
            }
        }

        #endregion
    }

    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
    }
}
