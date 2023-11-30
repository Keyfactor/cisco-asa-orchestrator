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

using CiscoAsaOrchestrator;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Orchestrators.Extensions;
using Newtonsoft.Json.Linq;

namespace Keyfactor.Extensions.Orchestrator.PaloAlto
{
    public class Validators
    {
        public static bool ValidateAddCliCommands(string json)
        {
            JObject jsonObject = JObject.Parse(json);
            JArray commandsArray = jsonObject["commands"] as JArray;

            foreach (var item in commandsArray)
            {
                if (!(item is JValue jValue) || !jValue.Value<string>().StartsWith("ssl trust-point"))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateRemoveCliCommands(string json)
        {
            JObject jsonObject = JObject.Parse(json);
            JArray commandsArray = jsonObject["commands"] as JArray;

            foreach (var item in commandsArray)
            {
                if (!(item is JValue jValue) || !jValue.Value<string>().StartsWith("crypto key zeroize rsa label"))
                {
                    return false;
                }
            }

            return true;
        }

        public static (bool valid, JobResult result) ValidateStoreProperties(StoreProperties storeProperties,long jobHistoryId)
        {
            var errors = string.Empty;

            if (storeProperties?.CommitToDisk==null)
            {
                    errors +="You need a boolean custom field setup for CommitToDisk";
            }


            var hasErrors = (errors.Length > 0);

            if (hasErrors)
            {
                var result = new JobResult
                {
                    Result = OrchestratorJobStatusJobResult.Failure,
                    JobHistoryId = jobHistoryId,
                    FailureMessage = $"The store setup is not valid. {errors}"
                };

                return (false, result);
            }

            return (true, new JobResult());
        }
    }
}
