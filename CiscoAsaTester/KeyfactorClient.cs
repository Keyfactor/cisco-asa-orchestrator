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

using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiscoAsaTester
{
    public class KeyfactorClient
    {
        public async Task<KeyfactorEnrollmentResult> EnrollCertificate(string commonName)
        {
            var options = new RestClientOptions("https://KeyfactorCommandURL");
            var client = new RestClient(options);
            var request = new RestRequest("/KeyfactorAPI/Enrollment/PFX", Method.Post);
            request.AddHeader("X-Keyfactor-Requested-With", "APIClient");
            request.AddHeader("x-certificateformat", "PFX");
            request.AddHeader("Authorization", "Basic BasicAuthTokenForUsercEM=");
            request.AddHeader("Content-Type", "application/json");
            var enrollRequest = new KeyfactorEnrollmentRequest
            {
                password = "sldfklsdfsldjfk",
                populateMissingValuesFromAD = false,
                subject = $"CN={commonName}",
                includeChain = true,
                renewalCertificateId = 0,
                certificateAuthority = "DC-CA.Command.local\\CommandCA1",
                timestamp = DateTime.Now,
                template = "2YearTestWebServer",
                keyType="RSA",
                keyLength=4096
            };
            SaNs sans = new SaNs();
            List<string> dnsList = new List<string> { $"{commonName}" };
            sans.DNS = dnsList;
            enrollRequest.saNs = sans;
            request.AddBody(enrollRequest);
            var response = await client.ExecuteAsync<KeyfactorEnrollmentResult>(request);
            return response.Data;

        }
    }
}
