using FsMonitor.CrossCutting.Crypto;
using RestSharp;
using RestSharp.Authenticators;
using System;
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
            _restClient.Authenticator = new HttpBasicAuthenticator(Decrypt.GetDecrypt(userName), Decrypt.GetDecrypt(password));
            _restClient.Timeout = -1;

            if (!string.IsNullOrEmpty(proxyAddress))
            {
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
            request.AddParameter("name", fileName);
            request.AddParameter("hash", folderHash);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            try
            {
                var response = _restClient.Execute(request);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    return response.Content;
                }

                if (response.ResponseStatus == ResponseStatus.Error)
                {
                   throw new System.Exception(response.ErrorMessage);
                }

                throw new WebException(response.Content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
