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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiscoAsaOrchestrator.Client.Models
{
    public class CertItem
    {
        public string kind { get; set; }
        public string type { get; set; }
        public string serialNumber { get; set; }
        public bool status { get; set; }
        public string usage { get; set; }
        public string publicKeyType { get; set; }
        public string validityStartDate { get; set; }
        public string validityEndDate { get; set; }
        public List<string> issuer { get; set; }
        public List<string> subject { get; set; }
        public string associatedTP { get; set; }
        public string signatureAlgorithm { get; set; }
        public string objectId { get; set; }
    }
}
