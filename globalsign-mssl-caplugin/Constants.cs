// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

namespace Keyfactor.Extensions.CAPlugin.GlobalSign;

internal class Constants
{
    public static string ORDERTESTURL = "OrderAPITestURL";
    public static string ORDERPRODURL = "OrderAPIProdURL";
    public static string QUERYTESTURL = "QueryAPITestURL";
    public static string QUERYPRODURL = "QueryAPIProdURL";
    public static string DATEFORMAT = "DateFormatString";
    public static string GLOBALSIGNUSER = "GlobalSignUsername";
    public static string GLOBALSIGNPASS = "GlobalSignPassword";
    public static string ISTEST = "TestAPI";
    public static string PICKUPRETRIES = "RetryCount";
    public static string PICKUPDELAY = "DelayTime";
    public static string SYNCSTARTDATE = "SyncStartDate";
    public static string SYNCINTERNVALDAYS = "SyncIntervalDays";
}

public static class EnrollmentConfigConstants
{
    public const string RootCAType = "RootCAType";
    public const string SlotSize = "SlotSize";
    public const string CertificateValidityInYears = "CertificateValidityInYears";
}