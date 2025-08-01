// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

namespace Keyfactor.Extensions.CAPlugin.GlobalSign;

public class GlobalSignCertType
{
    public bool HasWildCard { get; private set; }
    public GSCertificateType Type { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public string ShortName { get; private set; } = string.Empty;

    public static List<GlobalSignCertType> AllTypes =>
        new()
        {
            new GlobalSignCertType
            {
                DisplayName = "ExtendedSSL SHA256", ProductCode = "PEV_SHA2", ShortName = "ExtendedSSL",
                HasWildCard = false, Type = GSCertificateType.EV
            },
            new GlobalSignCertType
            {
                DisplayName = "ExtendedSSL SHA1", ProductCode = "PEV", ShortName = "ExtendedSSL-Deprecated",
                HasWildCard = false, Type = GSCertificateType.EV
            },

            new GlobalSignCertType
            {
                DisplayName = "OrganizationSSL SHA1", ProductCode = "PV", ShortName = "OrganizationSSL-Deprecated",
                HasWildCard = true, Type = GSCertificateType.OV
            },
            new GlobalSignCertType
            {
                DisplayName = "OrganizationSSL SHA256", ProductCode = "PV_SHA2", ShortName = "OrganizationSSL",
                HasWildCard = true, Type = GSCertificateType.OV
            },

            new GlobalSignCertType
            {
                DisplayName = "IntranetSSL SHA1", ProductCode = "PV_INTRA", ShortName = "IntranetSSL",
                HasWildCard = true, Type = GSCertificateType.Intranet
            },
            new GlobalSignCertType
            {
                DisplayName = "IntranetSSL SHA2", ProductCode = "PV_INTRA_SHA2", ShortName = "IntranetSSL",
                HasWildCard = true, Type = GSCertificateType.Intranet
            },
            new GlobalSignCertType
            {
                DisplayName = "IntranetSSL SHA256ECDSA", ProductCode = "PV_INTRA_ECCP256", ShortName = "IntranetSSL",
                HasWildCard = true, Type = GSCertificateType.Intranet
            },

            new GlobalSignCertType
            {
                DisplayName = "CloudSSL SHA256", ProductCode = "PV_CLOUD", ShortName = "IntranetSSL",
                HasWildCard = true, Type = GSCertificateType.Cloud
            },
            new GlobalSignCertType
            {
                DisplayName = "CloudSSL SHA256ECDSA", ProductCode = "PV_CLOUD_ECC2", ShortName = "IntranetSSL",
                HasWildCard = true, Type = GSCertificateType.Cloud
            }
        };
}

public enum GSCertificateType
{
    Intranet = 1,
    OV = 2,
    EV = 3,
    Cloud = 4
}