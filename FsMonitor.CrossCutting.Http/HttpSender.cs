using RestSharp;
using RestSharp.Authenticators;
using System.Net;

namespace FsMonitor.CrossCutting.Http
{
    public class HttpSender
    {
        private readonly RestClient _restClient;

        public HttpSender(string baseAddress, string userName, string password)
            : this(baseAddress, userName, password, null) { }
        public HttpSender(string baseAddress, string userName, string password, string proxyAddress)
        {
            BaseAddress = baseAddress;
            _restClient = new RestClient(BaseAddress);
            _restClient.Authenticator = new HttpBasicAuthenticator(userName, password);
            _restClient.Timeout = -1;

            if (!string.IsNullOrEmpty(proxyAddress)) {
                ProxyAddress = proxyAddress;
                _restClient.Proxy = new WebProxy(proxyAddress);
            }
        }

        public string BaseAddress { get; private set; }
        public string ProxyAddress { get; private set; }

        public string SendFormData(byte[] content, string fileName, string folderHash)
        {
            var request = new RestRequest(Method.POST);
            request.AddFile("file", content, fileName);
            request.AddParameter("file_name", fileName);
            request.AddParameter("folder_hash", folderHash);

            var response = _restClient.Execute(request);
            if(response.StatusCode == HttpStatusCode.OK) {
                
                return response.Content;
            }
            throw new WebException(response.Content);
        }
    }
}
