using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs;
using System.Net.Http;
using System.Text;
using System.Net;

namespace FunctionAppTest
{
    [TestClass]
    public class HttpTriggerTest 
    {
        private dynamic log;
        private ExecutionContext context;

        [TestInitialize]
        public void Test_Initialize()
        {
            log = Mock.Of<ILogger>();
            context = Mock.Of<ExecutionContext>();
        }

        [TestMethod]
        public async Task Test_RunMethod()
        {
            Environment.SetEnvironmentVariable("PAT_TOKEN", "patToken");

            var data = File.ReadAllText(@"./../../../testFiles/test.json");
            var request = TestFactory.CreateHttpRequest();
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");
            request.RequestUri = new Uri("https://dummyuri?repoName=dummyrepo");
            request.Method = HttpMethod.Post;

            // call the endpoint
            var response = (HttpResponseMessage)await GridEventHandler.Run(request, log, context);
            string result = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(result, "Unable to process the request: machinelearningservices-runcompleted");
        }

        [TestMethod]
        public async Task Test_RunMethodForSubscriptionValidationRequest()
        {
            var data = File.ReadAllText(@"./../../../testFiles/testSuscriptionValidationResponse.json");
            var request = TestFactory.CreateHttpRequest();
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");
            request.RequestUri = new Uri("https://dummyuri?repoName=dummyrepo");
            request.Method = HttpMethod.Post;

            var result = await GridEventHandler.Run(request, log, context);
            var resultString = await result.Content.ReadAsStringAsync();
            Assert.AreEqual(resultString , "{\"validationResponse\":\"512d38b6-c7b8-40c8-89fe-f46f9e9622b6\"}");

        }

        [TestMethod]
        public async Task Test_RunMethodForInvalidEvent()
        {
            var data = File.ReadAllText(@"./../../../testFiles/requestWithInvalidEventType.json");
            var request = TestFactory.CreateHttpRequest();
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");
            request.RequestUri = new Uri("https://dummyuri?repoName=dummyrepo");
            request.Method = HttpMethod.Post;

            var result = await GridEventHandler.Run(request, log, context);
            var resultString = await result.Content.ReadAsStringAsync();

            Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
            Assert.AreEqual(resultString, "Unable to process the request");

        }

        [TestMethod]
        public async Task Test_RunMethodWhenEventTypeIsMissing()
        {
            var data = File.ReadAllText(@"./../../../testFiles/requestWithoutEventType.json");
            var request = TestFactory.CreateHttpRequest();
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");
            request.RequestUri = new Uri("https://dummyuri?repoName=dummyrepo");
            request.Method = HttpMethod.Post;

            var result = await GridEventHandler.Run(request, log, context);
            var resultString = await result.Content.ReadAsStringAsync();

            Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
            Assert.AreEqual(resultString, "Unable to process the request");

        }

        [TestMethod]
        public async Task Test_RunMethodForInvalidRequestObject()
        {
            var data = File.ReadAllText(@"./../../../testFiles/testInvaidRequestObject.json");
            var request = TestFactory.CreateHttpRequest();
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");
            request.RequestUri = new Uri("https://dummyuri?repoName=dummyrepo");
            request.Method = HttpMethod.Post;

            var result = await GridEventHandler.Run(request, log, context);
            var resultString = await result.Content.ReadAsStringAsync();

            Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
            Assert.AreEqual(resultString, "Unable to process the request");

        }
    }
}
