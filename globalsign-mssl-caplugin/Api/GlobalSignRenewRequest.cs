// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System.Text;
using Keyfactor.Logging;
using Microsoft.Extensions.Logging;
using Order;

namespace Keyfactor.Extensions.CAPlugin.GlobalSign.Api;

public class GlobalSignRenewRequest : GlobalSignEnrollRequest
{
    private readonly ILogger Logger;
    internal new bool InternalIP;

    public GlobalSignRenewRequest(GlobalSignCAConfig config, bool privateDomain, bool InternalIP) : base(config, privateDomain, InternalIP)
    {
        this.InternalIP = InternalIP;
        Logger = LogHandler.GetClassLogger<GlobalSignCAPlugin>();
    }

    public string RenewalTargetOrderId { get; set; } = "0";

    public new BmV2PvOrderRequest Request
    {
        get
        {
            var request = new BmV2PvOrderRequest
            {
                OrderRequestHeader = new OrderRequestHeader { AuthToken = Config.GetOrderAuthToken() },
                MSSLProfileID = MsslProfileId,
                MSSLDomainID = MsslDomainId,
                ContactInfo = new ContactInfo
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Phone = Phone,
                    Email = Email
                }
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

                        var entry = new SANEntry();
                        entry.SubjectAltName = item;
                        var sb = new StringBuilder();
                        sb.Append("Adding SAN entry of type ");
                        if (item.StartsWith("*"))
                        {
                            entry.SubjectAltName = "13";
                            sb.Append("WILDCARD");
                        }
                        else
                        {
                            entry.SubjectAltName = "7";
                            sb.Append("FQDN");
                        }

                        sb.Append($" and value {item} to request");
                        Logger.LogInformation(sb.ToString());
                        sans.Add(entry);
                    }

                    /*
                    foreach (var item in SANs)
                    {
                        var entry = new SANEntry();
                        entry.SubjectAltName = item;

                        //Determine whether SAN has an IP address in it or not:
                        if (IsIPAddress(item))
                        {
                            //If toggle for intranet use for IPs was toggled, use this:
                            if (InternalIP)
                            {
                                entry.SubjectAltName = "4";
                            }
                            else
                            {
                                entry.SubjectAltName = "3";

                            }
                        }
                        else
                        {
                            if (item.StartsWith("*"))
                            {
                                entry.SubjectAltName = "13";
                            }
                            else
                            {
                                entry.SubjectAltName = "7";
                            }
                        }
                        /*
                        if (item.StartsWith("*"))
                            entry.SubjectAltName = "13";
                        else
                            entry.SubjectAltName = "7";

                    }
                    */
                    request.SANEntries = sans.ToArray();
                }

            var options = new List<Option>();
            if (request.SANEntries.Count() > 0)
            {
                var opt = new Option();
                opt.OptionName = "SAN";
                opt.OptionValue = "True";
                options.Add(opt);
            }

            var validityPeriod = new ValidityPeriod
            {
                Months = Months
            };
            request.OrderRequestParameter = new OrderRequestParameter
            {
                ProductCode = ProductCode,
                OrderKind = OrderKind,
                Licenses = Licenses,
                CSR = CSR,
                RenewalTargetOrderID = RenewalTargetOrderId,
                ValidityPeriod = validityPeriod,
                Options = options.ToArray()
            };
            if (!string.IsNullOrEmpty(BaseOption)) request.OrderRequestParameter.BaseOption = BaseOption;
            return request;
        }
    }
}