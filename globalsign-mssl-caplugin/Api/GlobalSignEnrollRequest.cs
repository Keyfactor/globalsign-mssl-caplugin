// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System.Net;
using System.Text;
using Keyfactor.Logging;
using Microsoft.Extensions.Logging;
using Order;

namespace Keyfactor.Extensions.CAPlugin.GlobalSign.Api;

public class GlobalSignEnrollRequest
{
    private readonly ILogger Logger;
    internal GlobalSignCAConfig Config;
    internal bool InternalIP = false;
    internal bool PrivateDomain = false;
    public GlobalSignEnrollRequest(GlobalSignCAConfig config,bool privateDomain, bool internalIP)
    {
        Config = config;
        Logger = LogHandler.GetClassLogger<GlobalSignCAPlugin>();
        this.InternalIP = internalIP;
        this.PrivateDomain = privateDomain;
        // Initialize non-nullable properties with default values
        CSR = string.Empty;
        ProductCode = string.Empty;
        CommonName = string.Empty;
        OrderKind = string.Empty;
        Licenses = string.Empty;
        Months = string.Empty;
        MsslProfileId = string.Empty;
        MsslDomainId = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Phone = string.Empty;
        Email = string.Empty;
        SANs = new List<string>();
        Seal = new PvSealInfo();
        EVProfile = new MsslEvProfileInfo();
    }

    public string CSR { get; set; }
    public string ProductCode { get; set; }
    public string CommonName { get; set; }

    public string BaseOption
    {
        get
        {
            if (!string.IsNullOrEmpty(CommonName))
            {
                if (CommonName.StartsWith("*"))
                    return "wildcard";
                return string.Empty; // Avoid null return
            }

            return string.Empty; // Avoid null return
        }
    }

    public string OrderKind { get; set; }
    public string Licenses { get; set; }
    public string Months { get; set; }
    public string MsslProfileId { get; set; }
    public string MsslDomainId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public List<string> SANs { get; set; }
    public PvSealInfo Seal { get; set; }
    public MsslEvProfileInfo EVProfile { get; set; }


    public BmV2PvOrderRequest Request
    {
        get
        {
            var request = new BmV2PvOrderRequest();
            request.OrderRequestHeader = new OrderRequestHeader { AuthToken = Config.GetOrderAuthToken() };
            request.MSSLProfileID = MsslProfileId;
            request.MSSLDomainID = MsslDomainId;
            if (PrivateDomain)
            {
                request.MSSLDomainID = "INTRANETSSLDOMAIN";
            }
            request.ContactInfo = new ContactInfo
            {
                FirstName = FirstName,
                LastName = LastName,
                Phone = Phone,
                Email = Email
            };
            if (SANs != null)
                if (SANs.Count > 0)
                {
                    var sans = new List<SANEntry>();
                    foreach (var item in SANs)
                    {
                        if (string.Equals(item, CommonName, StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.LogInformation($"SAN Entry {item} matches CN, removing from request");
                            continue;
                        }

						string trimCN = CommonName, trimItem = item;

						if (CommonName.StartsWith("*."))
						{
							trimCN = CommonName.Substring(2).ToLower();
							trimItem = item.ToLower();
							List<string> equivs = new List<string> { $"*.{trimCN}", $"www.{trimCN}", $"{trimCN}" };
							if (equivs.Contains(trimItem))
							{
								Logger.LogInformation($"SAN Entry {item} is equivalent to CN ignoring wildcards or www prefix, removing from request");
								continue;
							}
						}
						else if (CommonName.StartsWith("www."))
						{
							trimCN = CommonName.Substring(4).ToLower();
							trimItem = item.ToLower();
							List<string> equivs = new List<string> { $"www.{trimCN}", $"{trimCN}" };
							if (equivs.Contains(trimItem))
							{
								Logger.LogInformation($"SAN Entry {item} is equivalent to CN ignoring wildcards or www prefix, removing from request");
								continue;
							}
						}

						var entry = new SANEntry();
                        entry.SubjectAltName = item;
                        var sb = new StringBuilder();
                        sb.Append("Adding SAN entry of type ");
                        //Determine whether SAN has an IP address in it or not:
                        if (IsIPAddress(item))
                        {
                            //If toggle for intranet use for IPs was toggled, use this:
                            if (InternalIP)
                            {
                                entry.SANOptionType = "4";
                                sb.Append("INTERNAL IP");
                            }
                            else
                            {
                                entry.SANOptionType = "3";
                                sb.Append("PUBLIC IP");
                            }
                        }
                        else
                        {
                            if (item.StartsWith("*"))
                            {
                                entry.SANOptionType = "13";
                                sb.Append("WILDCARD");
                            }
                            else
                            {
                                entry.SANOptionType = "7";
                                sb.Append("FQDN");
                            }
                        }

                        sb.Append($" and value {item} to request");
                        Logger.LogInformation(sb.ToString());
                        sans.Add(entry);
                    }

                    request.SANEntries = sans.ToArray();
                }

            var options = new List<Option>();

            if (request.SANEntries == null)
            {
                throw new Exception("Please provide at least one SAN entry.");
            }
            if (request.SANEntries.Count() > 0)
            {
                var opt = new Option();
                opt.OptionName = "SAN";
                opt.OptionValue = "True";
                options.Add(opt);
            }

            var validityPeriod = new ValidityPeriod();
            validityPeriod.Months = Months;
            if (PrivateDomain)
            {
                request.OrderRequestParameter = new OrderRequestParameter
                {
                    BaseOption = "private",
                    ProductCode = ProductCode,
                    OrderKind = "new",
                    Licenses = Licenses,
                    CSR = CSR,
                    ValidityPeriod = validityPeriod,
                    Options = options.ToArray()
                };
            }
            else
            {
                request.OrderRequestParameter = new OrderRequestParameter
                {
                    ProductCode = ProductCode,
                    OrderKind = OrderKind,
                    Licenses = Licenses,
                    CSR = CSR,
                    ValidityPeriod = validityPeriod,
                    Options = options.ToArray()
                };
                if (!string.IsNullOrEmpty(BaseOption)) request.OrderRequestParameter.BaseOption = BaseOption;
            }

            return request;
        }
    }

    public static bool IsIPAddress(string input)
    {
        return IPAddress.TryParse(input, out _);
    }
}