// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Query;

namespace Keyfactor.Extensions.CAPlugin.GlobalSign;

public class GlobalSignCAConfig
{
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool IsTest { get; set; } = true;

    public int PickupRetries { get; set; } = 5;
    public int PickupDelay { get; set; } = 150;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";

    public string DateFormatString { get; set; } = "";

    public string ORDER_PROD_URL { get; set; } = "";
    public string ORDER_TEST_URL { get; set; } = "";
    public string QUERY_PROD_URL { get; set; } = "";
    public string QUERY_TEST_URL { get; set; } = "";


    public string SyncStartDate { get; set; } = "";
    public int SyncIntervalDays { get; set; } = 0;
    public bool Enabled { get; set; } = true;

    public string GetUrl(GlobalSignServiceType queryType)
    {
        switch (queryType)
        {
            case GlobalSignServiceType.ORDER:
                return IsTest ? ORDER_TEST_URL : ORDER_PROD_URL;

            case GlobalSignServiceType.QUERY:
                return IsTest ? QUERY_TEST_URL : QUERY_PROD_URL;
            default:
                throw new ArgumentException($"Invalid value ({queryType}) for queryType argument");
        }
    }

    public AuthToken GetQueryAuthToken()
    {
        return new AuthToken { UserName = Username, Password = Password };
    }

    public Order.AuthToken GetOrderAuthToken()
    {
        return new Order.AuthToken { UserName = Username, Password = Password };
    }
}

public enum GlobalSignServiceType
{
    ORDER,
    QUERY
}

public class ClientCertificate
{
    [JsonConverter(typeof(StringEnumConverter))]
    public StoreLocation StoreLocation { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public StoreName StoreName { get; set; }

    public string Thumbprint { get; set; } = string.Empty;
}