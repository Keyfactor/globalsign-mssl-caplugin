// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using Order;

namespace Keyfactor.Extensions.CAPlugin.GlobalSign.Api;

public class GlobalSignVerifyDomainRequest
{
    private readonly GlobalSignCAConfig Config;

    public GlobalSignVerifyDomainRequest(GlobalSignCAConfig config)
    {
        Config = config;
    }

    public string DomainName { get; set; } = string.Empty;
    public string DomainId { get; set; } = string.Empty;

    public BmV1VerifyMsslDomainRequest Request
    {
        get
        {
            var request = new BmV1VerifyMsslDomainRequest();
            var header = new OrderRequestHeader
            {
                AuthToken = Config.GetOrderAuthToken()
            };
			request.OrderRequestHeader = header;
			request.DomainID = DomainId;
			request.TagLocation = DomainName;
			request.VettingType = "DNS";
			return request;
        }
    }
}