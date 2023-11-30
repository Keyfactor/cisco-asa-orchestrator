// Copyright 2023 Keyfactor
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Keyfactor.Logging;
using Microsoft.Extensions.Logging;
using System;
using Newtonsoft.Json;
using Keyfactor.Orchestrators.Extensions;
using RestSharp;
using CiscoAsaOrchestrator.Client.Models;
using System.Net;
using CiscoAsaOrchestrator.Models;
using System.Collections.Generic;
using System.Data.Common;
using Keyfactor.Extensions.Orchestrator.PaloAlto;

namespace CiscoAsaOrchestrator.Client
{
    public class CiscoAsaClient
    {
        private readonly ILogger _logger;
        private readonly string _restClientUrl;
        private readonly string _authorizationHeader;

        public CiscoAsaClient(InventoryJobConfiguration config,string serverUserName, string serverPassword)
        {
            _logger = LogHandler.GetClassLogger<CiscoAsaClient>();
            _restClientUrl = $"https://{config.CertificateStoreDetails.ClientMachine}";
            _authorizationHeader = GetAuthorizationHeader(serverUserName, serverPassword);
        }

        public CiscoAsaClient(ManagementJobConfiguration config, string serverUserName, string serverPassword)
        {
            _logger = LogHandler.GetClassLogger<CiscoAsaClient>();
            _restClientUrl = $"https://{config.CertificateStoreDetails.ClientMachine}";
            _authorizationHeader = GetAuthorizationHeader(serverUserName, serverPassword);
        }

        private string GetAuthorizationHeader(string username, string password)
        {
            var authString = $"{username}:{password}";
            return "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(authString));
        }

        private T ExecuteRequest<T>(string resource, Method method, object body = null)
        {
            try
            {
                _logger.MethodEntry();
                var options = new RestClientOptions(_restClientUrl);
                options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                options.MaxTimeout = 60000;
                var client = new RestClient(options);
                var request = new RestRequest(resource, method);

                if (body != null)
                {
                    request.AddJsonBody(body);
                }

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("User-Agent", "REST API Agent");
                request.AddHeader("Authorization", _authorizationHeader);

                var response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<T>(response.Content);
                }

                if(response.StatusCode==HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("Authentication to Cisco ASA Failed.");
                }

                _logger.MethodExit();
                return default(T);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred: {LogHandler.FlattenException(e)}");
                throw;
            }
        }

        public CertificateProperties GetCertificateProperties(string aliasName)
        {
            var resource = $"/api/certificate/details/{aliasName}";
            return ExecuteRequest<CertificateProperties>(resource, Method.Get);
        }

        public CertificateDetails GetCertificate(string aliasName)
        {
            var resource = $"/api/certificate/identity/{aliasName}/export/PEM";
            return ExecuteRequest<CertificateDetails>(resource, Method.Get);
        }

        public CliResponse GetBindings()
        {
            var resource = $"/api/cli";
            var body = "{\"commands\":[\"show ssl\"]}";
            return ExecuteRequest<CliResponse>(resource, Method.Post, body);
        }


        public CliResponse SaveConfiguration()
        {
            var resource = $"/api/commands/writemem";
            return ExecuteRequest<CliResponse>(resource, Method.Post);
        }

        public CliResponse SetBindings(string trustPointName, string[] bindingLocation)
        {
            var resource = $"/api/cli";
            string command = string.Empty;
            foreach(var location in bindingLocation)
            { 
                command += $"\"ssl trust-point {trustPointName} {location}\",";
            }

            command = command.TrimEnd(',');

            var body = "{\"commands\":[" + command + "]}";

            if(Validators.ValidateAddCliCommands(body))
                return ExecuteRequest<CliResponse>(resource, Method.Post, body);

            return null;
        }

        public CertificateList GetCertificateList(int offset, int limit)
        {
            var resource = $"api/certificate/identity?offset={offset}&limit={limit}";
            return ExecuteRequest<CertificateList>(resource, Method.Get);
        }

        public MessageList AddCertificate(AddCertificateRequest addRequest)
        {
            var resource = "/api/certificate/identity";
            return ExecuteRequest<MessageList>(resource, Method.Post, addRequest);
        }

        public CliResponse RemoveKepair(string alias)
        {
            var resource = $"/api/cli";
            var command = $"\"crypto key zeroize rsa label {alias} noconfirm\"";

            var body = "{\"commands\":[" + command + "]}";

            if (Validators.ValidateRemoveCliCommands(body))
                return ExecuteRequest<CliResponse>(resource, Method.Post, body);

            return null;
        }

        public MessageList RemoveTrustpoint(string alias)
        {
            var resource = $"/api/certificate/identity/{alias}";
            return ExecuteRequest<MessageList>(resource, Method.Delete);
        }

        public WriteToDiskResponse WriteToDisk()
        {
            var resource = "/api/commands/writemem";
            return ExecuteRequest<WriteToDiskResponse>(resource, Method.Post);
        }
    }
}
