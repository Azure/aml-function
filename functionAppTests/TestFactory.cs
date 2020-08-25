using System.Net.Http;
using System.Text;

namespace FunctionAppTest
{
    public class TestFactory 
    {
        public static HttpRequestMessage CreateHttpRequest()
        {
            var requestMessage = new HttpRequestMessage();     
            return requestMessage;
        }
    }
}
