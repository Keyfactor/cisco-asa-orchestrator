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
using CiscoAsaOrchestrator.Client;
using CiscoAsaOrchestrator.Client.Models;
using Keyfactor.Logging;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Extensions.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.RegularExpressions;


namespace Keyfactor.Extensions.Orchestrator.CiscoAsa.Jobs
{
    public class Inventory : IInventoryJobExtension
    {
        public string ExtensionName => "CiscoAsa";

        private ILogger _logger;

        private readonly IPAMSecretResolver _resolver;

        public Inventory(IPAMSecretResolver resolver)
        {
            _resolver = resolver;
        }

        private string ResolvePamField(string name, string value)
        {
            _logger.LogTrace($"Attempting to resolved PAM eligible field {name}");
            return _resolver.Resolve(value);
        }

        private string ServerPassword { get; set; }
        private string ServerUserName { get; set; }
        private CiscoAsaClient Client { get; set; }

        //Job Entry Point
        public JobResult ProcessJob(InventoryJobConfiguration config, SubmitInventoryUpdate submitInventory)
        {
            _logger = LogHandler.GetClassLogger(GetType());
            _logger.LogDebug($"Begin Inventory...");

            ServerPassword = ResolvePamField("ServerPassword", config.ServerPassword);
            ServerUserName = ResolvePamField("ServerUserName", config.ServerUsername);
            var inventoryItems = new List<CurrentInventoryItem>();

            try
            {
                Client = new CiscoAsaClient(config, ServerUserName, ServerPassword);
                var limit = 100;
                var offset = 0;
                var certList = Client.GetCertificateList(offset, limit);
                var total = certList?.rangeInfo?.total;

                CliResponse bindings = Client.GetBindings();
                var items = bindings?.response[0]?.Split('\n');
                string pattern = @"^(.*?):\s*(.*?)(?:\s*\(.*\))?$";
                var regex = new Regex(pattern, RegexOptions.Multiline);

                var keyValuePairs= regex.Matches(bindings?.response[0]?.ToString())?
                    .ToDictionary(match => match.Groups[1].Value.Trim(), match => match.Groups[2].Value.Trim()); 

                var entryParams = TransformDictionary(keyValuePairs);

                while (offset <= total)
                {
                    if (offset > 0)
                        certList = Client.GetCertificateList(offset, limit);

                    offset += limit - 1;

                    if (certList != null)
                    {
                        inventoryItems.AddRange(certList.items.Select(
                            c =>
                            {
                                try
                                {
                                    _logger.LogTrace(
                                        $"Building Cert List Inventory Item Alias: {c.name} Keypair?: {c?.keyPair?.Length > 0}");
                                    return BuildInventoryItem(c.name, c?.keyPair?.Length > 0, entryParams);
                                }
                                catch
                                {
                                    _logger.LogWarning(
                                        $"Could not fetch the certificate: {c.name}.");

                                    return new CurrentInventoryItem();
                                }
                            }).Where(acsii => acsii?.Certificates != null).ToList());
                    }
                    else
                    {
                        return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = "Failed to get list of certs from Cisco, returned null." };
                    }
                }
            }
            catch (Exception ex)
            {
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = $"Unexpected Error Occured During Inventory: {LogHandler.FlattenException(ex)}" };
            }

            try
            {
                submitInventory.Invoke(inventoryItems);
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
            }
            catch (Exception ex)
            {
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = $"Unexpected Error Occured Submitting Inventory to Keyfactor: {LogHandler.FlattenException(ex)}" };
            }
        }


        static Dictionary<string, object> TransformDictionary(Dictionary<string, string> inputDict)
        {
            var transformedDict = new Dictionary<string, object>();

            foreach (var kvp in inputDict.Where(kvp => kvp.Key.Contains("Interface", StringComparison.InvariantCultureIgnoreCase)))
            {
                string key = kvp.Key;
                object value = kvp.Value;

                // Apply transformations based on the specified rules
                key = key.Replace("interface", "", StringComparison.InvariantCultureIgnoreCase).Trim();

                if (key.Contains("VPNLB", StringComparison.InvariantCultureIgnoreCase))
                    key = string.Concat(key.AsSpan(key.LastIndexOf(' ') + 1), " vpnlb-ip");

                transformedDict[key] = value;
            }

            return transformedDict;
        }


        protected virtual CurrentInventoryItem BuildInventoryItem(string alias, bool privateKey, Dictionary<string, object> entryParams)
        {
            try
            {
                _logger.MethodEntry();
                _logger.LogTrace($"Alias: {alias} PrivateKey: {privateKey}");
                _logger.LogTrace($"Got modAlias: {alias}, certAttributes and mapSettings");

                var interfaces = string.Join(",", entryParams.Where(kvp => kvp.Value.ToString() == alias).Select(kvp => kvp.Key));

                var pemArray = Client.GetCertificate(alias);
                if (pemArray == null) return null;

                var combinedPemString = string.Join(Environment.NewLine, pemArray.certificate);

                _logger.MethodExit();
                return new CurrentInventoryItem
                {
                    Alias = alias,
                    Certificates = new[] { combinedPemString },
                    ItemStatus = OrchestratorInventoryItemStatus.Unknown,
                    PrivateKeyEntry = privateKey,
                    UseChainLevel = false,
                    Parameters = new Dictionary<string, object> { ["interfaces"] = interfaces }
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred in Inventory.BuildInventoryItem: {LogHandler.FlattenException(e)}");
                throw;
            }
        }
    }
}