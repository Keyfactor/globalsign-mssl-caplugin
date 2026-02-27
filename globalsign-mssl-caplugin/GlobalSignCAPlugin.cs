// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using Keyfactor.AnyGateway.Extensions;
using Keyfactor.Extensions.CAPlugin.GlobalSign.Api;
using Keyfactor.Extensions.CAPlugin.GlobalSign.Client;
using Keyfactor.Logging;
using Keyfactor.PKI.Enums.EJBCA;
using Microsoft.Extensions.Logging;
using Order;
using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Keyfactor.Extensions.CAPlugin.GlobalSign;

public class GlobalSignCAPlugin : IAnyCAPlugin
{
    private ICertificateDataReader? _certificateDataReader; 
    private ILogger Logger;

    private GlobalSignCAConfig Config { get; set; } = new();
    private bool _enabled = false;

    public void Initialize(IAnyCAPluginConfigProvider configProvider, ICertificateDataReader certificateDataReader)
    {
        Logger = LogHandler.GetClassLogger(GetType());
        Logger.MethodEntry();
        _enabled = (bool)configProvider.CAConnectionData["Enabled"];
        if (!_enabled)
        {
            Logger.LogWarning($"The CA is currently in the Disabled state. It must be Enabled to perform operations. Skipping config validation and MSSL Client creation...");
            Logger.MethodExit();
            return;
        }
        Config = new GlobalSignCAConfig
        {
            IsTest = bool.Parse((string)configProvider.CAConnectionData["TestAPI"]),
            Enabled = bool.Parse((string)configProvider.CAConnectionData["Enabled"]),
            Password = (string)configProvider.CAConnectionData["GlobalSignPassword"],
            Username = (string)configProvider.CAConnectionData["GlobalSignUsername"],
            PickupDelay = int.Parse((string)configProvider.CAConnectionData["DelayTime"]),
            PickupRetries = int.Parse((string)configProvider.CAConnectionData["RetryCount"]),
            DateFormatString = (string)configProvider.CAConnectionData["DateFormatString"],
            ORDER_PROD_URL = (string)configProvider.CAConnectionData["OrderAPIProdURL"],
            ORDER_TEST_URL = (string)configProvider.CAConnectionData["OrderAPITestURL"],
            QUERY_TEST_URL = (string)configProvider.CAConnectionData["QueryAPITestURL"],
            QUERY_PROD_URL = (string)configProvider.CAConnectionData["QueryAPIProdURL"],
            SyncStartDate = configProvider.CAConnectionData.ContainsKey("SyncStartDate")
                ? (string)configProvider.CAConnectionData["SyncStartDate"]
                : string.Empty,
            SyncIntervalDays = configProvider.CAConnectionData.TryGetValue("SyncIntervalDays", out var val)
                               && int.TryParse(val as string, out var parsed)
                ? parsed
                : 0
        };
        _certificateDataReader = certificateDataReader;
        Logger.MethodExit();
    }

    public async Task<AnyCAPluginCertificate> GetSingleRecord(string caRequestID)
    {
        Logger.MethodEntry();
        try
        {
            if (Config == null) throw new InvalidOperationException("Config is not initialized.");
            var apiClient = new GlobalSignApiClient(Config, Logger);
            var response = await apiClient.PickupCertificateById(caRequestID);
            return response;
        }
        catch (Exception uEx)
        {
            Logger.LogError($"Error requesting certificate detail for caRequestID: {caRequestID}");
            Logger.LogError(uEx.Message);
            throw;
        }
    }

    public async Task Synchronize(BlockingCollection<AnyCAPluginCertificate> blockingBuffer, DateTime? lastSync,
        bool fullSync, CancellationToken cancelToken)
    {
        Logger.MethodEntry();
        var syncType = fullSync ? "full" : "incremental";
        Logger.LogDebug($"Performing {syncType} sync");
        try
        {
            if (Config == null) throw new InvalidOperationException("Config is not initialized.");
            var apiClient = new GlobalSignApiClient(Config, Logger);

            var fullSyncFrom = new DateTime(2000, 01, 01);
            if (!string.IsNullOrEmpty(Config.SyncStartDate)) fullSyncFrom = DateTime.Parse(Config.SyncStartDate);

            var syncFrom = lastSync;
            var certs = await apiClient.GetCertificatesForSyncAsync(fullSync, syncFrom, fullSyncFrom,
                Config.SyncIntervalDays);

            foreach (var c in certs)
            {
                var orderStatus = (GlobalSignOrderStatus)Enum.Parse(typeof(GlobalSignOrderStatus),
                    c.CertificateInfo?.CertificateStatus ?? string.Empty);
                DateTime? subDate = DateTime.TryParse(c.OrderInfo?.OrderDate, out var orderDate) ? orderDate : null;
                DateTime? resDate = DateTime.TryParse(c.OrderInfo?.OrderCompleteDate, out var completeDate)
                    ? completeDate
                    : null;
                DateTime? revDate = DateTime.TryParse(c.OrderInfo?.OrderDeactivatedDate, out var deactivateDate)
                    ? deactivateDate
                    : null;

                var certToAdd = new AnyCAPluginCertificate
                {
                    CARequestID = c.OrderInfo?.OrderId ?? string.Empty,
                    ProductID = c.OrderInfo?.ProductCode ?? string.Empty,
                    Status = OrderStatus.ConvertToKeyfactorStatus(orderStatus),
                    CSR = c.Fulfillment?.OriginalCSR ?? string.Empty,
                    Certificate = c.Fulfillment?.ServerCertificate?.X509Cert ?? string.Empty,
                    RevocationReason = 0,
                    RevocationDate = orderStatus == GlobalSignOrderStatus.Revoked ? revDate : null
                };
                Logger.LogTrace(
                    $"Synchronization: Adding certificate with request ID {c.OrderInfo?.OrderId ?? string.Empty} to the results");
                blockingBuffer.Add(certToAdd);
            }

            blockingBuffer.CompleteAdding();
        }
        catch (Exception ex)
        {
            Logger.LogError("Unhandled exception during sync. Stopping sync process");
            Logger.LogError(ex.Message);
            blockingBuffer.CompleteAdding();
        }

        Logger.MethodExit();
    }

    public async Task<int> Revoke(string caRequestID, string hexSerialNumber, uint revocationReason)
    {
        Logger.MethodEntry();
        try
        {
            if (Config == null) throw new InvalidOperationException("Config is not initialized.");
            var apiClient = new GlobalSignApiClient(Config, Logger);
            var returnval = await apiClient.RevokeCertificateById(caRequestID);
            return returnval;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Unhandled exception revoking certificate with request id {caRequestID}");
            Logger.LogError(ex.Message);
            throw;
        }
    }

    public async Task<EnrollmentResult> Enroll(string csr, string subject, Dictionary<string, string[]> san,
        EnrollmentProductInfo productInfo, RequestFormat requestFormat, EnrollmentType enrollmentType)
    {
        Logger.MethodEntry();
        var requestor = productInfo.ProductParameters.TryGetValue("ContactName", out var cnValue) && !string.IsNullOrWhiteSpace(cnValue)
            ? cnValue
            : throw new ArgumentException("The 'ContactName' parameter is missing or has an invalid value.");

        Logger.LogDebug($"Resolving requesting user as '{requestor}'");
        try
        {
            var apiClient =
                new GlobalSignApiClient(Config!, Logger); // Use null-forgiving operator since Config is set in Initialize
            Logger.LogDebug("Parsing enrollment values:");
            var priorSn = string.Empty;
            if (productInfo.ProductParameters.ContainsKey("priorcertsn"))
            {
                priorSn = productInfo.ProductParameters["PriorCertSN"];
                Logger.LogDebug($"Prior cert sn: {priorSn}");
            }

            bool InternalIP;
            if (productInfo.ProductParameters.TryGetValue("InternalIP", out string? iIP))
            {
                InternalIP = bool.TryParse(iIP, out var parsedIP) && parsedIP;
            }
            else
            {
                Logger.LogTrace("The 'InternalIP' parameter is missing from ProductParameters. Defaulting to false.");
                InternalIP = false;
            }
            bool privateDomain;
            string requesterEmail = string.Empty;
            string requesterTel = string.Empty;
            if (productInfo.ProductParameters.TryGetValue("PrivateDomain", out string? prD))
            {
                privateDomain = bool.TryParse(prD, out var parsedD) && parsedD;
                if (privateDomain)
                {
                    if (!productInfo.ProductParameters.TryGetValue("RequesterEmail", out requesterEmail) || string.IsNullOrWhiteSpace(requesterEmail))
                    {
                        Logger.LogWarning("The 'RequesterEmail' parameter is required when 'PrivateDomain' is true but was not provided or is empty.");
                        requesterEmail = string.Empty;
                    }
                    if (!productInfo.ProductParameters.TryGetValue("RequesterTel", out requesterTel) || string.IsNullOrWhiteSpace(requesterTel))
                    {
                        Logger.LogWarning("The 'RequesterTel' parameter is required when 'PrivateDomain' is true but was not provided or is empty.");
                        requesterTel = string.Empty;
                    }
                }
            }
            else
            {
                Logger.LogTrace("The 'PrivateDomain' parameter is missing from ProductParameters. Defaulting to false.");
                privateDomain = false;
            }
            string? commonName = null;
            GetDomainsDomainDetail? domain = null;
            var allDomains = await apiClient.GetDomains();
            var validDomainStatus = new List<string> { "3", "7", "9", "10" };
            var validDomains = allDomains.Where(d => validDomainStatus.Contains(d.DomainStatus)).ToList();
            if (validDomains.Count == 0 && !privateDomain)
                throw new Exception("No domains found that are valid for certificate enrollment");
            try
            {
                commonName = ParseSubject(subject, "CN=");
            }
            catch
            {
                Logger.LogWarning("Subject is missing a CN value. Using SAN domain lookup instead");
            }

            var rawSanList = new StringBuilder();
            rawSanList.Append("Raw SAN List:\n");
            foreach (var sanType in san.Keys)
            {
                rawSanList.Append($"SAN Type: {sanType}. Values: ");
                foreach (var indivSan in san[sanType]) rawSanList.Append($"{indivSan},");
                rawSanList.Append('\n');
            }

            Logger.LogTrace(rawSanList.ToString());

            // build a case‐insensitive SAN lookup
            var sanDict = new Dictionary<string, string[]>(san, StringComparer.OrdinalIgnoreCase);
            if (sanDict.TryGetValue("dnsname", out var dnsSans))
                Logger.LogTrace($"DNS SAN Count: {dnsSans.Length}");
            if (sanDict.TryGetValue("ipaddress", out var ipSans))
                Logger.LogTrace($"IP SAN Count: {ipSans.Length}");

            // only try to resolve a domain if we don't already have a commonName
            if (string.IsNullOrWhiteSpace(commonName))
            {
                // 1) Try IP SANs first
                if (ipSans != null)
                    foreach (var ipSan in ipSans)
                    {
                        if (string.IsNullOrWhiteSpace(ipSan))
                            continue;

                        var tempDomain = validDomains?
                            .FirstOrDefault(d =>
                                !string.IsNullOrEmpty(d?.DomainName) &&
                                ipSan.EndsWith($".{d.DomainName}", StringComparison.OrdinalIgnoreCase)
                            );

                        if (tempDomain != null)
                        {
                            Logger.LogDebug($"ipSAN Domain match found for ipSAN: {ipSan}");
                            domain = tempDomain;
                            commonName = ipSan;
                            break;
                        }
                    }

                // 2) If still not found, try DNS SANs
                if (domain == null && dnsSans != null)
                    foreach (var dnsSan in dnsSans)
                    {
                        if (string.IsNullOrWhiteSpace(dnsSan))
                            continue;

                        var tempDomain = validDomains?
                            .FirstOrDefault(d =>
                                !string.IsNullOrEmpty(d?.DomainName) &&
                                dnsSan.EndsWith(d.DomainName, StringComparison.OrdinalIgnoreCase)
                            );

                        if (tempDomain != null)
                        {
                            Logger.LogDebug($"SAN Domain match found for SAN: {dnsSan}");
                            domain = tempDomain;
                            commonName = dnsSan;
                            break;
                        }
                    }
            }
            // If private domain skip domain resolution.
            if (privateDomain)
            {
                var profiles = await apiClient.GetProfiles();
                var fillProfile = profiles.FirstOrDefault();
                // If PrivateDomain is true, we don't need to fully resolve a domain
                domain = new GetDomainsDomainDetail()
                {
                    ContactInfo = new ContactInfoDomain()
                    {
                        Email = requesterEmail,
                        Phone = requesterTel,
                        FirstName = requestor,
                        LastName = requestor

                    }
                };
                domain.MSSLProfileID = fillProfile.MSSLProfileId;
            }

            // 3) Fallback: if we did obtain a commonName (or it was already set), try matching it
            if (domain == null && !string.IsNullOrWhiteSpace(commonName))
                domain = validDomains?
                    .FirstOrDefault(d =>
                        !string.IsNullOrEmpty(d?.DomainName) &&
                        commonName.EndsWith(d.DomainName, StringComparison.OrdinalIgnoreCase)
                    );


            if (domain == null) throw new Exception("Unable to determine GlobalSign domain");

            

            Logger.LogDebug(
                $"Domain info:\nDomain Name: {domain?.DomainName}\nMsslDomainId: {domain?.DomainID}\nMsslProfileId: {domain?.MSSLProfileID}");
            Logger.LogDebug($"Using common name: {commonName}");
            var months = (int.Parse(productInfo.ProductParameters["CertificateValidityInYears"]) * 12).ToString();
            Logger.LogDebug($"Using validity: {months} months.");

            var sanList = new List<string>();
            if (sanDict.TryGetValue("dnsname", out string[]? dnsnames))
                foreach (var dnsSan in dnsnames)
                    sanList.Add(dnsSan);

            if (sanDict.TryGetValue("ipaddress", out string[]? ipaddresses))
                foreach (var ipSan in ipaddresses)
                    sanList.Add(ipSan);

            var productType = GlobalSignCertType.AllTypes.FirstOrDefault(x =>
                x.ProductCode.Equals(productInfo.ProductID, StringComparison.InvariantCultureIgnoreCase));

            switch (enrollmentType)
            {
                case EnrollmentType.New:

                    Logger.LogDebug(
                        $"Issuing new certificate request for product code {productType?.ProductCode ?? string.Empty}");
                    // Fix for CS8601: Use null-coalescing operator to assign default values if the source is null

                    var request = new GlobalSignEnrollRequest(Config, privateDomain, InternalIP)
                    {
                        MsslDomainId = domain?.DomainID ?? string.Empty,
                        MsslProfileId = domain?.MSSLProfileID ?? string.Empty,
                        CSR = csr,
                        Licenses = "1",
                        OrderKind = "new",
                        Months = months,
                        FirstName = requestor,
                        LastName = requestor,
                        Email = domain?.ContactInfo?.Email ?? string.Empty,
                        Phone = domain?.ContactInfo?.Phone ?? string.Empty,
                        CommonName = commonName ?? string.Empty,
                        ProductCode = productType?.ProductCode ?? string.Empty,
                        SANs = sanList
                    };
                    return await apiClient.Enroll(request);
                case EnrollmentType.RenewOrReissue:
                    //Determine whether this is a renewal or a reissue:
                    var renewal = false;
                    var order_id = await _certificateDataReader.GetRequestIDBySerialNumber(priorSn);
                    var expirationDate = _certificateDataReader.GetExpirationDateByRequestId(order_id);
                    if (expirationDate == null)
                    {
                        var localcert = await GetSingleRecord(order_id);
                        expirationDate = localcert.RevocationDate;
                    }

                    if (expirationDate < DateTime.Now) renewal = true;
                    if (renewal)
                    {
                        Logger.LogDebug(
                            $"Issuing certificate renewal request for cert with request ID {order_id} and product code {productType?.ProductCode ?? string.Empty}");
                        var renewRequest = new GlobalSignRenewRequest(Config, privateDomain, InternalIP)
                        {
                            MsslDomainId = domain?.DomainID ?? string.Empty,
                            MsslProfileId = domain?.MSSLProfileID ?? string.Empty,
                            CSR = csr,
                            Licenses = "1",
                            OrderKind = "renewal",
                            Months = months,
                            FirstName = requestor,
                            LastName = requestor,
                            Email = domain?.ContactInfo?.Email ?? string.Empty,
                            Phone = domain?.ContactInfo?.Phone ?? string.Empty,
                            CommonName = commonName ?? string.Empty,
                            ProductCode = productType?.ProductCode ?? string.Empty,
                            RenewalTargetOrderId = order_id,
                            SANs = sanList
                        };
                        return await apiClient.Renew(renewRequest);
                    }

                    Logger.LogDebug($"Issuing certificate reissue request for cert with request ID {order_id}");
                    var reissueRequest = new GlobalSignReissueRequest(Config)
                    {
                        CSR = csr,
                        OrderID = order_id
                    };

                    return await apiClient.Reissue(reissueRequest, priorSn);
                case EnrollmentType.Reissue:
                case EnrollmentType.Renew:
                default:
                    return new EnrollmentResult
                        { Status = 30, StatusMessage = $"Unsupported enrollment type {enrollmentType}" };
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Unhandled exception enrolling for certificate with subject {subject}");
            Logger.LogError(ex.Message);
            return new EnrollmentResult
            {
                StatusMessage = $"{ex.Message}",
                Status = (int)EndEntityStatus.FAILED
            };
        }
    }

    public async Task Ping()
    {
        Logger.MethodEntry();
        if (!_enabled)
        {
            Logger.LogWarning($"The CA is currently in the Disabled state. It must be Enabled to perform operations. Skipping config validation and MSSL Client creation...");
            Logger.MethodExit();
            return;
        }
        try
        {
            Logger.LogInformation("Ping reqeuest recieved");
        }
        catch (Exception e)
        {
            Logger.LogError($"There was an error contacting GlobalSign: {e.Message}.");
            throw new Exception($"Error attempting to ping GlobalSign: {e.Message}.", e);
        }

        Logger.MethodExit();
    }

    public async Task ValidateCAConnectionInfo(Dictionary<string, object> connectionInfo)
    {
        Logger = LogHandler.GetClassLogger(GetType());
        Logger.MethodEntry();
        try
        {
            if (!(bool)connectionInfo["Enabled"])
            {
                Logger.LogWarning($"The CA is currently in the Disabled state. It must be Enabled to perform operations. Skipping validation...");
                Logger.MethodExit(LogLevel.Trace);
                return;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Exception: {LogHandler.FlattenException(ex)}");
        }
        Config = new GlobalSignCAConfig
        {
            IsTest = bool.Parse((string)connectionInfo["TestAPI"]),
            Password = (string)connectionInfo["GlobalSignPassword"],
            Username = (string)connectionInfo["GlobalSignUsername"],
            PickupDelay = int.Parse((string)connectionInfo["DelayTime"]),
            PickupRetries = int.Parse((string)connectionInfo["RetryCount"]),
            DateFormatString = (string)connectionInfo["DateFormatString"],
            ORDER_PROD_URL = (string)connectionInfo["OrderAPIProdURL"],
            ORDER_TEST_URL = (string)connectionInfo["OrderAPITestURL"],
            QUERY_TEST_URL = (string)connectionInfo["QueryAPITestURL"],
            QUERY_PROD_URL = (string)connectionInfo["QueryAPIProdURL"],
            SyncStartDate = connectionInfo.TryGetValue("SyncStartDate", out object? value)
                ? (string)value : string.Empty,
            SyncIntervalDays = connectionInfo.TryGetValue("SyncIntervalDays", out var val)
                               && int.TryParse(val as string, out var parsed)
                ? parsed
                : 0
        };
        var apiClient = new GlobalSignApiClient(Config, Logger);
        var response = await apiClient.GetDomains();
        response.ForEach(x => Logger.LogInformation($"Connection established for {x}"));
        Logger.MethodExit();
    }

    public Task ValidateProductInfo(EnrollmentProductInfo productInfo, Dictionary<string, object> connectionInfo)
    {
        Config = new GlobalSignCAConfig
        {
            IsTest = bool.Parse((string)connectionInfo["TestAPI"]),
            Password = (string)connectionInfo["GlobalSignPassword"],
            Username = (string)connectionInfo["GlobalSignUsername"],
            PickupDelay = int.Parse((string)connectionInfo["DelayTime"]),
            PickupRetries = int.Parse((string)connectionInfo["RetryCount"]),
            DateFormatString = (string)connectionInfo["DateFormatString"],
            ORDER_PROD_URL = (string)connectionInfo["OrderAPIProdURL"],
            ORDER_TEST_URL = (string)connectionInfo["OrderAPITestURL"],
            QUERY_TEST_URL = (string)connectionInfo["QueryAPITestURL"],
            QUERY_PROD_URL = (string)connectionInfo["QueryAPIProdURL"],
            SyncStartDate = connectionInfo.TryGetValue("SyncStartDate", out object? value)
                ? (string)value : string.Empty,
            SyncIntervalDays = connectionInfo.TryGetValue("SyncIntervalDays", out var val)
                               && int.TryParse(val as string, out var parsed)
                ? parsed
                : 0
        };
        var certType = GlobalSignCertType.AllTypes.Find(x =>
            x.ProductCode.Equals(productInfo.ProductID, StringComparison.InvariantCultureIgnoreCase));

        if (certType == null) throw new ArgumentException($"Cannot find {productInfo.ProductID}", "ProductId");

        Logger.LogInformation($"Validated {certType.DisplayName} ({certType.ProductCode})configured for AnyGateway");

        return Task.CompletedTask;
    }

    public Dictionary<string, PropertyConfigInfo> GetCAConnectorAnnotations()
    {
        return new Dictionary<string, PropertyConfigInfo>
        {
            [Constants.GLOBALSIGNUSER] = new()
            {
                Comments = "GlobalSign MSSL API Username",
                Hidden = false,
                DefaultValue = "",
                Type = "String"
            },
            [Constants.GLOBALSIGNPASS] = new()
            {
                Comments = "GlobalSign MSSL API Password",
                Hidden = true,
                DefaultValue = "",
                Type = "String"
            },
            [Constants.DATEFORMAT] = new()
            {
                Comments = "Date format string. Default is yyyy-MM-ddTHH:mm:ss.fffZ",
                Hidden = false,
                DefaultValue = "yyyy-MM-ddTHH:mm:ss.fffZ",
                Type = "String"
            },
            [Constants.ORDERPRODURL] = new()
            {
                Comments =
                    "MSSL Order Prod API URL. Default is https://system.globalsign.com/kb/ws/v2/ManagedSSLService",
                Hidden = false,
                DefaultValue = "https://system.globalsign.com/kb/ws/v2/ManagedSSLService",
                Type = "String"
            },
            [Constants.ORDERTESTURL] = new()
            {
                Comments =
                    "MSSL Order Test API URL. Default is https://test-gcc.globalsign.com/kb/ws/v2/ManagedSSLService",
                Hidden = false,
                DefaultValue = "https://test-gcc.globalsign.com/kb/ws/v2/ManagedSSLService",
                Type = "String"
            },
            [Constants.QUERYPRODURL] = new()
            {
                Comments = "MSSL Query Prod API URL. Default is https://system.globalsign.com/kb/ws/v1/GASService",
                Hidden = false,
                DefaultValue = "https://system.globalsign.com/kb/ws/v1/GASService",
                Type = "String"
            },
            [Constants.QUERYTESTURL] = new()
            {
                Comments = "MSSL Query Test API URL. Default is https://test-gcc.globalsign.com/kb/ws/v1/GASService",
                Hidden = false,
                DefaultValue = "https://test-gcc.globalsign.com/kb/ws/v1/GASService",
                Type = "String"
            },
            [Constants.ISTEST] = new()
            {
                Comments = "Enable the use of the test GlobalSign API endpoints. Default is false.",
                Hidden = false,
                DefaultValue = "false",
                Type = "Bool"
            },
            [Constants.PICKUPDELAY] = new()
            {
                Comments =
                    "This is the number of seconds between retries when attempting to download a certificate. Default is 150.",
                Hidden = false,
                DefaultValue = "150",
                Type = "Integer"
            },
            [Constants.PICKUPRETRIES] = new()
            {
                Comments =
                    "This is the number of times the AnyGateway will attempt to pickup an new certificate before reporting an error. Default is 5.",
                Hidden = false,
                DefaultValue = "5",
                Type = "Integer"
            },
            [Constants.SYNCINTERNVALDAYS] = new()
            {
                Comments =
                    "OPTIONAL: Required if SyncStartDate is used. Specifies how to page the certificate sync. Should be a value such that no interval of that length contains > 500 certificate enrollments.",
                Hidden = false,
                DefaultValue = "5",
                Type = "Integer"
            },
            [Constants.SYNCSTARTDATE] = new()
            {
                Comments =
                    "If provided, full syncs will start at the specified date.",
                Hidden = false,
                DefaultValue = "2000-01-01",
                Type = "Integer"
            },
            [Constants.Enabled] = new()
            {
                Comments = "Flag to Enable or Disable gateway functionality. Disabling is primarily used to allow creation of the CA prior to configuration information being available.",
                Hidden = false,
                DefaultValue = true,
                Type = "Boolean"
            }
        };
    }


    public Dictionary<string, PropertyConfigInfo> GetTemplateParameterAnnotations()
    {
        return new Dictionary<string, PropertyConfigInfo>
        {
            [EnrollmentConfigConstants.CertificateValidityInYears] = new()
            {
                Comments = "Number of years the certificate will be valid for",
                Hidden = false,
                DefaultValue = "1",
                Type = "Number"
            },
            [EnrollmentConfigConstants.SlotSize] = new()
            {
                Comments =
                    "Maximum number of SANs that a certificate may have - valid values are [FIVE, TEN, FIFTEEN, TWENTY, THIRTY, FOURTY, FIFTY, ONE_HUNDRED]",
                Hidden = false,
                DefaultValue = "FIVE",
                Type = "String"
            },
            [EnrollmentConfigConstants.RootCAType] = new()
            {
                Comments =
                    "The certificate's root CA - Depending on certificate expiration date, SHA_1 not be allowed. Will default to SHA_2 if expiration date exceeds sha1 allowed date. Options are GlobalSign R certs.",
                Hidden = false,
                DefaultValue = "GLOBALSIGN_ROOT_R3",
                Type = "String"
            }
        };
    }

    public List<string> GetProductIds()
    {
        var ProductIDs = new List<string>();
        foreach (var certType in GlobalSignCertType.AllTypes) ProductIDs.Add(certType.ProductCode);

        return ProductIDs;
    }

    private void ThrowValidationException(List<string> errors)
    {
        var validationMsg = $"Validation errors:\n{string.Join("\n", errors)}";
        throw new AnyCAValidationException(validationMsg);
    }

    private static bool IsIPAddress(string input)
    {
        return IPAddress.TryParse(input, out _);
    }

    #region Private Methods

    private static string ParseSubject(string subject, string rdn)
    {
        var escapedSubject = subject.Replace("\\,", "|");
        var rdnString = escapedSubject.Split(',').ToList().FirstOrDefault(x => x.Contains(rdn));

        if (!string.IsNullOrEmpty(rdnString))
            return rdnString.Replace(rdn, "").Replace("|", ",").Trim();
        throw new Exception($"The request is missing a {rdn} value");
    }

    #endregion Private Methods
}