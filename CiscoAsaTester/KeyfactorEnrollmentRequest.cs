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

namespace CiscoAsaTester
{
    public class KeyfactorEnrollmentRequest
    {
        public string template { get; set; }
        public SaNs saNs { get; set; }
        public string certificateAuthority { get; set; }
        public bool includeChain { get; set; }
        public DateTime timestamp { get; set; }
        public string password { get; set; }
        public bool populateMissingValuesFromAD { get; set; }
        public string subject { get; set; }
        public string keyType { get; set; }
        public int keyLength { get; set; }
        public int renewalCertificateId { get; set; }
        public int certificateCollectionOrder { get; set; }
        public bool useLegacyEncryption { get; set; }
    }

    public class SaNs
    {
        public List<string> DNS { get; set; }
    }
}
