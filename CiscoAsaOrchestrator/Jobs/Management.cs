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
using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Common.Enums;
using Microsoft.Extensions.Logging;
using Keyfactor.Orchestrators.Extensions.Interfaces;
using Newtonsoft.Json;
using CiscoAsaOrchestrator.Client;
using Org.BouncyCastle.Pkcs;
using System.Collections.Generic;
using CiscoAsaOrchestrator.Client.Models;
using CiscoAsaOrchestrator;

namespace Keyfactor.Extensions.Orchestrator.CiscoAsa.Jobs
{
    public class Management : IManagementJobExtension
    {
        //Necessary to implement IManagementJobExtension but not used.  Leave as empty string.
        public string ExtensionName => "CiscoAsa";
        private ILogger _logger;
        private readonly IPAMSecretResolver _resolver;

        public Management(IPAMSecretResolver resolver)
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

        private StoreProperties CertStoreProperties { get; set; }

        protected internal virtual AsymmetricKeyEntry KeyEntry { get; set; }

        private static readonly Func<string, string> Pemify = ss =>
    ss.Length <= 64 ? ss : ss.Substring(0, 64) + "\n" + Pemify(ss.Substring(64));

        //Job Entry Point
        public JobResult ProcessJob(ManagementJobConfiguration config)
        {
            _logger = LogHandler.GetClassLogger(GetType());
            _logger.MethodEntry();

            CertStoreProperties = JsonConvert.DeserializeObject<StoreProperties>(
            config.CertificateStoreDetails.Properties,
            new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Populate });

            ServerPassword = ResolvePamField("ServerPassword", config.ServerPassword);
            ServerUserName = ResolvePamField("ServerUserName", config.ServerUsername);

            var complete = new JobResult
            {
                Result = OrchestratorJobStatusJobResult.Failure,
                JobHistoryId = config.JobHistoryId,
                FailureMessage = "Invalid Management Operation"
            };

            try
            {
                //Management jobs, unlike Discovery, Inventory, and Reenrollment jobs can have 3 different purposes:
                switch (config.OperationType)
                {
                    case CertStoreOperationType.Add:
                        _logger.LogTrace("Adding...");
                        _logger.LogTrace($"Add Config Json {JsonConvert.SerializeObject(config)}");
                        complete = PerformAddition(config);
                        break;
                    case CertStoreOperationType.Remove:
                        complete = PerformRemoval(config);
                        break;
                    default:
                        //Invalid OperationType.  Return error.  Should never happen though
                        return complete;
                }
            }
            catch (Exception ex)
            {
                return new JobResult() { Result = OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = $"Custom message you want to show to show up as the error message in Job History in KF Command {LogHandler.FlattenException(ex)}" };
            }
            return complete;
        }

        private bool CheckForDuplicate(CiscoAsaClient client, string certificateName)
        {
            try
            {
                var certificate = client.GetCertificate(certificateName);
                if (certificate == null)
                    return false;

                return true;
            }
            catch (Exception e)
            {
                _logger.LogTrace(
                    $"Error Checking for Duplicate Cert in Management.CheckForDuplicate {LogHandler.FlattenException(e)}");
                throw;
            }
        }
        private static JobResult ReturnJobResult(ManagementJobConfiguration config,
    MessageList messages)
        {
            if (messages == null)
                return new JobResult
                {
                    Result = OrchestratorJobStatusJobResult.Success,
                    JobHistoryId = config.JobHistoryId,
                    FailureMessage = ""
                };

            foreach (var message in messages.messages)
            {
                if (message.level.ToUpper() == "ERROR")
                {
                    return new JobResult
                    {
                        Result = OrchestratorJobStatusJobResult.Failure,
                        JobHistoryId = config.JobHistoryId,
                        FailureMessage = $"Result returned error {message.details}"
                    };
                }
            }

            return new JobResult
            {
                Result = OrchestratorJobStatusJobResult.Success,
                JobHistoryId = config.JobHistoryId,
                FailureMessage = ""
            };

        }

        private JobResult PerformRemoval(ManagementJobConfiguration config)
        {
            var client = new CiscoAsaClient(config, ServerUserName, ServerPassword); //Api base URL Plus Key
            var trustpointResponse = client.RemoveTrustpoint(config.JobCertificate.Alias);

            var trustpointJobResponse = ReturnJobResult(config, trustpointResponse);
            if (trustpointJobResponse.Result == OrchestratorJobStatusJobResult.Failure)
                return trustpointJobResponse;

            var keyPairResponse = client.RemoveKepair(config.JobCertificate.Alias);
            if (keyPairResponse.response[0].Contains("ERROR:", StringComparison.InvariantCultureIgnoreCase))
                return new JobResult
                {
                    Result = OrchestratorJobStatusJobResult.Failure,
                    JobHistoryId = config.JobHistoryId,
                    FailureMessage = $"Could not remove keypair."
                };

            if (Convert.ToBoolean(CertStoreProperties.CommitToDisk))
                client.SaveConfiguration();

            return new JobResult
            {
                Result = OrchestratorJobStatusJobResult.Success,
                JobHistoryId = config.JobHistoryId,
                FailureMessage = $"Successfully Removed Certficate and Keypair."
            };
        }

        private string GenerateUniqueAlias(string Alias)
        {
            DateTime currentDateTime = DateTime.Now;
            long ticks = currentDateTime.Ticks;

            int dashIndex = Alias.LastIndexOf("||");
            if(dashIndex > 0)
                 Alias = Alias.Substring(0, dashIndex);

            // Convert ticks to integer
            var intDateTime = Math.Abs((int)(ticks / TimeSpan.TicksPerSecond));

            return string.Concat(Alias, "||", intDateTime.ToString());
        }

        private JobResult PerformAddition(ManagementJobConfiguration config)
        {
            //Temporarily only performing additions
            try
            {
                _logger.MethodEntry();
                var warnings = string.Empty;

                if (config.CertificateStoreDetails.StorePath.Length > 0)
                {
                    _logger.LogTrace(
                        $"Credentials JSON: Url: {config.CertificateStoreDetails.ClientMachine} Server UserName: {config.ServerUsername}");

                    var client =
                        new CiscoAsaClient(config,
                            ServerUserName, ServerPassword); //Api base URL Plus Key
                    _logger.LogTrace(
                        "Cisco Asa Client Created");

                    var duplicate = CheckForDuplicate(client, config.JobCertificate.Alias);
                    _logger.LogTrace($"Duplicate? = {duplicate}");

                    //Check for Duplicate already in Cisco Asa, if there, make sure the Overwrite flag is checked before replacing
                    if (duplicate && config.Overwrite || !duplicate)
                    {
                        var aliasName = config.JobCertificate.Alias;
                        if (duplicate)
                            aliasName = GenerateUniqueAlias(aliasName);

                        _logger.LogTrace("Either not a duplicate or overwrite was chosen....");
                        if (!string.IsNullOrWhiteSpace(config.JobCertificate.PrivateKeyPassword)) // This is a PFX Entry
                        {
                            _logger.LogTrace($"Found Private Key {config.JobCertificate.PrivateKeyPassword}");

                            if (string.IsNullOrWhiteSpace(aliasName))
                                _logger.LogTrace("No Alias Found");

                            //Get PKCS With Private Key
                            var certPKCS12Content = Pemify(config.JobCertificate.Contents);
                            var request = new AddCertificateRequest
                            {
                                certText = new List<string>
                                {
                                    "-----BEGIN PKCS12-----",
                                    certPKCS12Content,
                                    "-----END PKCS12-----"
                                },
                                certPass = config.JobCertificate.PrivateKeyPassword,
                                name = aliasName,
                                kind = "object#IdentityCertificate"
                            };

                            var addResponse = client.AddCertificate(request);
                            var interfacesCSV = config.JobProperties["interfaces"]?.ToString();
                            CliResponse bindingsResponse=null;
                            if (!string.IsNullOrEmpty(interfacesCSV))
                            {
                                var interfaces = interfacesCSV.Split(',');
                                bindingsResponse = client.SetBindings(aliasName, interfaces);
                            }

                            if (Convert.ToBoolean(CertStoreProperties.CommitToDisk))
                                client.SaveConfiguration();

                            if (addResponse == null)
                                return new JobResult
                                {
                                    Result = OrchestratorJobStatusJobResult.Failure,
                                    JobHistoryId = config.JobHistoryId,
                                    FailureMessage = $"Had trouble adding the certificate check logs for details"
                                };

                            if (bindingsResponse == null)
                                return new JobResult
                                {
                                    Result = OrchestratorJobStatusJobResult.Warning,
                                    JobHistoryId = config.JobHistoryId,
                                    FailureMessage = $"Certificate was added but could not be bound check logs for details"
                                };

                            return ReturnJobResult(config, addResponse);
                        }
                    }

                    return new JobResult
                    {
                        Result = OrchestratorJobStatusJobResult.Failure,
                        JobHistoryId = config.JobHistoryId,
                        FailureMessage =
                            $"Duplicate alias {config.JobCertificate.Alias} found in Cisco Asa, to overwrite use the overwrite flag."
                    };
                }

                return new JobResult
                {
                    Result = OrchestratorJobStatusJobResult.Failure,
                    JobHistoryId = config.JobHistoryId,
                    FailureMessage =
                        "Store Path is empty"
                };
            }
            catch (Exception e)
            {
                return new JobResult
                {
                    Result = OrchestratorJobStatusJobResult.Failure,
                    JobHistoryId = config.JobHistoryId,
                    FailureMessage =
                        $"Management/Add {e.Message}"
                };
            }
        }
    }
}