// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

namespace Keyfactor.Extensions.CAPlugin.GlobalSign.Api;

public class GlobalSignErrorIndex
{
    private static readonly Dictionary<int, GlobalSignError> ErrorDictionary = new()
    {
        {
            101,
            new GlobalSignError
            {
                HResult = 0xA0060000, SuccessCode = -1, ErrorCode = -101, ErrorMessage = "Invalid parameter entered.",
                ErrorDetails =
                    "Invalid parameter entered. Please check that the parameters match the API specification. Parameter: {0}"
            }
        },
        {
            102,
            new GlobalSignError
            {
                HResult = 0xA0060001, SuccessCode = -1, ErrorCode = -102, ErrorMessage = "Mandatory parameter missing",
                ErrorDetails =
                    "Mandatory parameter missing. Please check that the parameters match the API specification. Parameter: {0}"
            }
        },
        {
            103,
            new GlobalSignError
            {
                HResult = 0xA0060002, SuccessCode = -1, ErrorCode = -103, ErrorMessage = "Parameter length check error",
                ErrorDetails =
                    "Parameter length check error. Please check that the parameters match the API specification. Parameter: {0}"
            }
        },
        {
            104,
            new GlobalSignError
            {
                HResult = 0xA0060003, SuccessCode = -1, ErrorCode = -104,
                ErrorMessage = "Parameter format check error.",
                ErrorDetails =
                    "Parameter format check error. Please check that the parameters match the API specification. Parameter: {0}"
            }
        },
        {
            105,
            new GlobalSignError
            {
                HResult = 0xA0060004, SuccessCode = -1, ErrorCode = -105,
                ErrorMessage = "Invalid parameter combination",
                ErrorDetails =
                    "Invalid parameter combination. Please that check the parameters match the API specification."
            }
        },
        {
            300,
            new GlobalSignError
            {
                HResult = 0xA0060005, SuccessCode = -1, ErrorCode = -300,
                ErrorMessage =
                    "Database Error. Please retry and if the issue persists contact support with detailed information concerning the issue.",
                ErrorDetails =
                    "Database Error. Please retry and if the issue persists contact support with detailed information concerning the issue."
            }
        },
        {
            4001,
            new GlobalSignError
            {
                HResult = 0xA0060006, SuccessCode = -1, ErrorCode = -4001,
                ErrorMessage = "Login failure invalid user ID Login failure.",
                ErrorDetails =
                    "UserName or Password is incorrect. Please make sure that you have specified the correct UserName and Password."
            }
        },
        {
            4008,
            new GlobalSignError
            {
                HResult = 0xA0060007, SuccessCode = -1, ErrorCode = -4008,
                ErrorMessage =
                    "The certificate is either expired, does not meet the requirements of transfer, or is inaccessible on the CN by the GlobalSign system. Please ensure that the certificate is correct and try again",
                ErrorDetails =
                    "Unable to process this request. It could be that the Common Name in the TargetCERT specified does not match the Common Name specified for this request or the TargetCERT is inaccessible on the Common Name by the GlobalSign system. Please review the contents and accessibility of the Common Name in the TargetCERT before proceeding with this request."
            }
        },
        {
            6101,
            new GlobalSignError
            {
                HResult = 0xA0060008, SuccessCode = -1, ErrorCode = -6101,
                ErrorMessage = "The account used does not have enough balance to order a certificate",
                ErrorDetails =
                    "Your account does not have enough remaining balance to process this request. Please make sure you have enough remaining balance in your account before proceeding with this request."
            }
        },
        {
            6102,
            new GlobalSignError
            {
                HResult = 0xA0060009, SuccessCode = -1, ErrorCode = -6102,
                ErrorMessage =
                    "The renewal of the certificate failed. There may be lacking or incorrect information that is required for the renewal of the certificate",
                ErrorDetails =
                    "The renewal of the certificate failed. Please note that when renewing a certificate, the Common Name of the original certificate and this request must be the same. Please also check that the status of the original order is ISSUED and that the order has not been previously renewed."
            }
        },
        {
            9401,
            new GlobalSignError
            {
                HResult = 0xA006000A, SuccessCode = -1, ErrorCode = -9401,
                ErrorMessage =
                    "No profile was found using the supplied MSSLProfileID. Please make sure that the supplied MSSLProfileID is correct.",
                ErrorDetails =
                    "Unable to process this request because you do not have permission to access the MSSLProfileID or we were unable to find the MSSLProfileID specified. Please make sure that the supplied MSSLProfileID is correct."
            }
        },
        {
            9403,
            new GlobalSignError
            {
                HResult = 0xA006000B, SuccessCode = -1, ErrorCode = -9403,
                ErrorMessage =
                    "The account used does not have MSSL rights. Please make sure you are using. Please make sure you are using an account with MSSL rights.",
                ErrorDetails =
                    "MSSL is not activated for this user. Please make sure that your UserName is correctly entered."
            }
        },
        {
            9440,
            new GlobalSignError
            {
                HResult = 0xA006000C, SuccessCode = -1, ErrorCode = -9440,
                ErrorMessage =
                    "No domain was found using the supplied MSSLDomainID. Please make sure that the supplied MSSLDomainID is correct.",
                ErrorDetails =
                    "We were unable to find the MSSLDomainID specified. Please make sure that the supplied MSSLDomainID is correct."
            }
        },
        {
            9443,
            new GlobalSignError
            {
                HResult = 0xA006000D, SuccessCode = -1, ErrorCode = -9443,
                ErrorMessage =
                    "The account used does not have access to the domain associated with the supplied MSSLDomainID",
                ErrorDetails =
                    "You do not have permission to use the specified MSSLDomainID. Please make sure that the MSSLDomainID is correctly specified."
            }
        },
        {
            9450,
            new GlobalSignError
            {
                HResult = 0xA006000E, SuccessCode = -1, ErrorCode = -9450,
                ErrorMessage = "Cannot request a certificate order. Please try again.",
                ErrorDetails =
                    "Unable to process this request. Please note that when requesting for EV orders, an MSSLProfileID with an EV Vetting level must be used. Also make sure that the ProductCode of your request is supported in MSSL."
            }
        },
        {
            9913,
            new GlobalSignError
            {
                HResult = 0xA006000F, SuccessCode = -1, ErrorCode = -9913,
                ErrorMessage = "No valid coupons were found. Please recheck the supplied coupon.",
                ErrorDetails =
                    "We were unable to find the Coupon specified. Please make sure that it is correctly entered."
            }
        },
        {
            9914,
            new GlobalSignError
            {
                HResult = 0xA0060010, SuccessCode = -1, ErrorCode = -9914,
                ErrorMessage = "No valid campaigns were found, Please recheck the supplied campaign.",
                ErrorDetails =
                    "We were unable to find the Campaign specified. Please make sure that it is correctly entered."
            }
        },
        {
            9915,
            new GlobalSignError
            {
                HResult = 0xA0060011, SuccessCode = -1, ErrorCode = -9915,
                ErrorMessage = "Certificate was already canceled",
                ErrorDetails =
                    "The OrderID you are trying to modify has been cancelled previously. Please make sure that the OrderID is correctly entered."
            }
        },
        {
            9916,
            new GlobalSignError
            {
                HResult = 0xA0060012, SuccessCode = -1, ErrorCode = -9916,
                ErrorMessage = "Cannot find the certificate that is associated with the order id you have supplied",
                ErrorDetails =
                    "We were not able to find the OrderID specified. Please make sure that the OrderID is correctly entered."
            }
        },
        {
            9918,
            new GlobalSignError
            {
                HResult = 0xA0060013, SuccessCode = -1, ErrorCode = -9918,
                ErrorMessage = "The coupon or campaign you supplied is invalid",
                ErrorDetails =
                    "The coupon or campaign you specified is already expired. Please make sure that the coupon or campaign is correctly entered."
            }
        },
        {
            9919,
            new GlobalSignError
            {
                HResult = 0xA0060014, SuccessCode = -1, ErrorCode = -9919,
                ErrorMessage = "The coupon or campaign you supplied is already used",
                ErrorDetails =
                    "The coupon you specified has been used previously. Please make sure that the coupon is correctly entered."
            }
        },
        {
            9920,
            new GlobalSignError
            {
                HResult = 0xA0060015, SuccessCode = -1, ErrorCode = -9920,
                ErrorMessage = "The coupon or campaign you supplied is not allowed to be used",
                ErrorDetails =
                    "The coupon or campaign you specified is not yet activated. Please make sure that the coupon or campaign is correctly entered."
            }
        },
        {
            9922,
            new GlobalSignError
            {
                HResult = 0xA0060016, SuccessCode = -1, ErrorCode = -9922,
                ErrorMessage = "The coupon or campaign's currency is not the same with the currency of your user",
                ErrorDetails =
                    "The currency of the specified Coupon or Campaign is not the same with the currency of your user. Please make sure that the coupon or campaign is correctly entered."
            }
        },
        {
            9933,
            new GlobalSignError
            {
                HResult = 0xA0060017, SuccessCode = -1, ErrorCode = -9933,
                ErrorMessage =
                    "The expiration date you have entered is not compatible with the product you have selected",
                ErrorDetails =
                    "The calculated months of the NotBefore and NotAfter specified is beyond the specified Months. Please make sure that the NotBefore and NotAfter has been entered correctly."
            }
        },
        {
            9936,
            new GlobalSignError
            {
                HResult = 0xA0060018, SuccessCode = -1, ErrorCode = -9936,
                ErrorMessage =
                    "GlobalSign operates a security and vulnerability scan of the public key component of the CSR you have just submitted.",
                ErrorDetails =
                    "The key you used in your CSR is either too short (RSA minimum 2048, ECC minimum 256), or the key failed the Debian weak key check as well as key length. Please generate a new keypair and try again"
            }
        },
        {
            9938,
            new GlobalSignError
            {
                HResult = 0xA0060019, SuccessCode = -1, ErrorCode = -9938,
                ErrorMessage = "The status of the certificate has already been changed",
                ErrorDetails =
                    "The certificate you are trying to modify has already been modified. Please make sure that the OrderID is correctly entered."
            }
        },
        {
            9942,
            new GlobalSignError
            {
                HResult = 0xA006001A, SuccessCode = -1, ErrorCode = -9942,
                ErrorMessage = "A problem was encountered when trying to request the certificate in the RA System",
                ErrorDetails =
                    "An internal server problem has been encountered. Please try again and if the issue persists contact GlobalSign support with detailed information concerning the issue."
            }
        },
        {
            9943,
            new GlobalSignError
            {
                HResult = 0xA006001B, SuccessCode = -1, ErrorCode = -9943,
                ErrorMessage = "A problem was was encountered when trying to issue the certificate in the RA System",
                ErrorDetails =
                    "We were unable to issue this certificate request. It could be that your certificate has been modified previously. Please make sure that Data is correctly entered."
            }
        },
        {
            4201,
            new GlobalSignError
            {
                HResult = 0xA006001C, SuccessCode = -1, ErrorCode = -4201, ErrorMessage = "",
                ErrorDetails =
                    "Your IP Address {0} is not within the range of IP addresses that is allowed for API use. Please contact GlobalSign support to have this address added for API access"
            }
        },
        {
            6002,
            new GlobalSignError
            {
                HResult = 0xA006001D, SuccessCode = -1, ErrorCode = -6002, ErrorMessage = "",
                ErrorDetails =
                    "There was an error when trying to parse the TargetCERT specified. Please make sure that the TargetCERT specified is correct."
            }
        },
        {
            6007,
            new GlobalSignError
            {
                HResult = 0xA006001E, SuccessCode = -1, ErrorCode = -6007, ErrorMessage = "",
                ErrorDetails =
                    "The Public Key of the CSR has been used previously. For security reasons we allow the keys to be used if they have the same CN. Please recheck the CSR specified and try again."
            }
        },
        {
            6017,
            new GlobalSignError
            {
                HResult = 0xA006001F, SuccessCode = -1, ErrorCode = -6017, ErrorMessage = "",
                ErrorDetails =
                    "The number of SANEntry has exceeded the maximum allowed number of SANEntry. Please do not exceed the maximum allowed number of SANEntry."
            }
        },
        {
            6021,
            new GlobalSignError
            {
                HResult = 0xA0060020, SuccessCode = -1, ErrorCode = -6021, ErrorMessage = "",
                ErrorDetails =
                    "Common Name in CSR and FQDN for check do not match. Please make sure that the CSR has been entered correctly."
            }
        },
        {
            9200,
            new GlobalSignError
            {
                HResult = 0xA0060021, SuccessCode = -1, ErrorCode = -9200, ErrorMessage = "",
                ErrorDetails =
                    "The type of your user is not allowed to use this API. Please check your permission and retry."
            }
        },
        {
            9404,
            new GlobalSignError
            {
                HResult = 0xA0060022, SuccessCode = -1, ErrorCode = -9404, ErrorMessage = "",
                ErrorDetails =
                    "You do not have permission to add a domain to this MSSLProfileID. Please make sure that the MSSLProfileID is correctly entered."
            }
        },
        {
            9405,
            new GlobalSignError
            {
                HResult = 0xA0060023, SuccessCode = -1, ErrorCode = -9405, ErrorMessage = "",
                ErrorDetails =
                    "Unable to process this request. You need to upgrade your account to MSSL Pro before you can add another profile. Please contact Globalsign Support to request for an upgrade to MSSL Pro."
            }
        },
        {
            9406,
            new GlobalSignError
            {
                HResult = 0xA0060024, SuccessCode = -1, ErrorCode = -9406, ErrorMessage = "",
                ErrorDetails =
                    "The DomainName already exists for the MSSLProfileID. Please make sure that the DomainName you are adding is unique or make sure that the MSSLProfileID specified is correctly entered."
            }
        },
        {
            9407,
            new GlobalSignError
            {
                HResult = 0xA0060025, SuccessCode = -1, ErrorCode = -9407, ErrorMessage = "",
                ErrorDetails =
                    "A Profile with that OrganizationName, StateOrProvince, Locality and Country already exists. Please make sure that the details mentioned above are correctly entered."
            }
        },
        {
            9430,
            new GlobalSignError
            {
                HResult = 0xA0060026, SuccessCode = -1, ErrorCode = -9430, ErrorMessage = "",
                ErrorDetails =
                    "You do not have permission to edit the specified MSSLProfileID. Please make sure the the MSSLProfileID is correctly entered."
            }
        },
        {
            9442,
            new GlobalSignError
            {
                HResult = 0xA0060027, SuccessCode = -1, ErrorCode = -9442, ErrorMessage = "",
                ErrorDetails =
                    "You do not have permission to delete the specified MSSLDomainID. Please make sure that the MSSLDomainID is correctly entered."
            }
        },
        {
            9444,
            new GlobalSignError
            {
                HResult = 0xA0060028, SuccessCode = -1, ErrorCode = -9444, ErrorMessage = "",
                ErrorDetails =
                    "The specified DomainName or SubjectAltName is not supported. Note that wildcard gTLDs are not supported. Please make sure that the DomainName or SubjecctAltName specified is correctly entered."
            }
        },
        {
            9445,
            new GlobalSignError
            {
                HResult = 0xA0060029, SuccessCode = -1, ErrorCode = -9445, ErrorMessage = "",
                ErrorDetails =
                    "Unable to process this request because the vetting level of the MSSLProfileID and the specified VettingLevel does not match. Note that when adding an EV Domain, the vetting level of the specified MSSLProfile should also be EV. Please make sure that the MSSLProfileID or the VettingLevel is correctly entered."
            }
        },
        {
            4083,
            new GlobalSignError
            {
                HResult = 0xA006002A, SuccessCode = -1, ErrorCode = -4083, ErrorMessage = "",
                ErrorDetails =
                    "The CommonName specified is not the same or is not a subdomain of the specified MSSLDomainID. Please make sure that the CommonName or the MSSLDomainID is correctly entered."
            }
        },
        {
            9901,
            new GlobalSignError
            {
                HResult = 0xA006002B, SuccessCode = -1, ErrorCode = -9901, ErrorMessage = "",
                ErrorDetails =
                    "The Product Group of this user does not allow ordering of the specified ProductCode. Please contact Globalsign Support if you wish to order using this ProductCode."
            }
        },
        {
            9902,
            new GlobalSignError
            {
                HResult = 0xA006002C, SuccessCode = -1, ErrorCode = -9902, ErrorMessage = "",
                ErrorDetails =
                    "Unable to process this request. You do not have permission to access the OrderID. Please make sure that the OrderID is correctly entered."
            }
        },
        {
            9934,
            new GlobalSignError
            {
                HResult = 0xA006002D, SuccessCode = -1, ErrorCode = -9934, ErrorMessage = "",
                ErrorDetails =
                    "The Top Level Domain used belongs to Globalsign's Banned List. Therefore, a certificate cannot be issued. Please make sure that Common Name is correctly entered."
            }
        },
        {
            9939,
            new GlobalSignError
            {
                HResult = 0xA006002E, SuccessCode = -1, ErrorCode = -9939, ErrorMessage = "",
                ErrorDetails =
                    "The state of this account is either invalid, stopped or locked. Please make sure that the account is correctly configured. Contact customer support for assistance."
            }
        },
        {
            9940,
            new GlobalSignError
            {
                HResult = 0xA006002F, SuccessCode = -1, ErrorCode = -9940, ErrorMessage = "",
                ErrorDetails =
                    "The specified NotBefore or NotAfter should not be before the current date. Please recheck these parameters before continuing with this request. OR The public key used in the CSR specified has been previously revoked. Please confirm you CSR and try again."
            }
        },
        {
            9949,
            new GlobalSignError
            {
                HResult = 0xA0060030, SuccessCode = -1, ErrorCode = -9949, ErrorMessage = "",
                ErrorDetails =
                    "The NotAfter specified is after the calculated BaseLine Validity Limit. Please take note that validity should not exceed 39 months."
            }
        },
        {
            9952,
            new GlobalSignError
            {
                HResult = 0xA0060031, SuccessCode = -1, ErrorCode = -9952, ErrorMessage = "",
                ErrorDetails =
                    "The Top Level Domain you specified belongs to the list of TLDs that is not allowed for ordering. Please make sure that Common Name is correctly entered."
            }
        },
        {
            9953,
            new GlobalSignError
            {
                HResult = 0xA0060032, SuccessCode = -1, ErrorCode = -9953, ErrorMessage = "",
                ErrorDetails =
                    "Cannot complete this request because the region or country of your Domain is not allowed for this partner.Please make sure that Common Name is correctly entered."
            }
        },
        {
            9961,
            new GlobalSignError
            {
                HResult = 0xA0060033, SuccessCode = -1, ErrorCode = -9961, ErrorMessage = "",
                ErrorDetails =
                    "The ECC CSR you specified is is not allowed. Please enter an ECC CSR using either P-256 or P-384 curves."
            }
        },
        {
            9962,
            new GlobalSignError
            {
                HResult = 0xA0060034, SuccessCode = -1, ErrorCode = -9962, ErrorMessage = "",
                ErrorDetails = "Key Compression is not allowed. Please make sure that CSR is correctly entered."
            }
        },
        {
            9964,
            new GlobalSignError
            {
                HResult = 0xA0060035, SuccessCode = -1, ErrorCode = -9964, ErrorMessage = "",
                ErrorDetails =
                    "Unable to process this request. It could be that the HashAlgorithm of this order is ECC but the key Algorithm of the CSR is RSA. Please make sure that the CSR or the ProductCode are correctly entered."
            }
        },
        {
            9971,
            new GlobalSignError
            {
                HResult = 0xA0060036, SuccessCode = -1, ErrorCode = -9971, ErrorMessage = "",
                ErrorDetails =
                    "Due to industry requirements, you can no longer issue certificates with internal server names in Common Name. Please specify a non-internal Common Name."
            }
        },
        {
            201,
            new GlobalSignError
            {
                HResult = 0xA0060037, SuccessCode = -1, ErrorCode = -201,
                ErrorMessage = "Internal system error - Failed database operation",
                ErrorDetails =
                    "System Error. (Database error - database operation). Please retry and if the issue persists contact support with detailed information concerning the issue."
            }
        },
        {
            301,
            new GlobalSignError
            {
                HResult = 0xA0060039, SuccessCode = -1, ErrorCode = -301,
                ErrorMessage = "Internal system error - Failed database operation",
                ErrorDetails =
                    "System Error. (Database error - inconsistent data). Please retry and if the issue persists contact support with detailed information concerning the issue."
            }
        },
        {
            2001,
            new GlobalSignError
            {
                HResult = 0xA006003A, SuccessCode = -1, ErrorCode = -2001,
                ErrorMessage = "Internal system error - Email sending warning",
                ErrorDetails =
                    "Internal system error - Email sending warning. Unable to send email with the details of your order to your email address. Please contact support with detailed information concerning this issue."
            }
        },
        {
            8001,
            new GlobalSignError
            {
                HResult = 0xA006003B, SuccessCode = -1, ErrorCode = -8001, ErrorMessage = "",
                ErrorDetails = "Unknown GlobalSign error occured"
            }
        },
        {
            2220,
            new GlobalSignError
            {
                HResult = 0xA006003C, SuccessCode = -1, ErrorCode = -2220, ErrorMessage = "",
                ErrorDetails = "The GlobalSign webservice responded with a null object."
            }
        },
        {
            2221,
            new GlobalSignError
            {
                HResult = 0xA006003D, SuccessCode = -1, ErrorCode = -2221, ErrorMessage = "",
                ErrorDetails = "The CSR is missing a common name"
            }
        },
        {
            2222,
            new GlobalSignError
            {
                HResult = 0xA006003E, SuccessCode = -1, ErrorCode = -2222, ErrorMessage = "",
                ErrorDetails = "The domain id for the provided common name could not be found"
            }
        },
        {
            2223,
            new GlobalSignError
            {
                HResult = 0xA006003F, SuccessCode = -1, ErrorCode = -2223, ErrorMessage = "",
                ErrorDetails = "The profile id for the provided common name and organization could not be found"
            }
        },
        {
            2224,
            new GlobalSignError
            {
                HResult = 0xA0060040, SuccessCode = -1, ErrorCode = -2224, ErrorMessage = "",
                ErrorDetails =
                    "Not a valid term for a globalsign product. Valid terms lengths are 6 months, 1 year, or 2 years."
            }
        },
        {
            2225,
            new GlobalSignError
            {
                HResult = 0xA0060041, SuccessCode = -1, ErrorCode = -2225, ErrorMessage = "",
                ErrorDetails = "Could not connect to GlobalSign web service"
            }
        },
        {
            2226,
            new GlobalSignError
            {
                HResult = 0xA0050042, SuccessCode = -1, ErrorCode = -2226, ErrorMessage = "",
                ErrorDetails =
                    "This domain is not enabled. To order a certificate for this domain please enabled it in the configuration wizard."
            }
        }
    };

    public static GlobalSignError GetGlobalSignError(int errorCode)
    {
        GlobalSignError? value = null;
        var code = errorCode;
        if (code < 0) code = code * -1;
        ErrorDictionary.TryGetValue(code, out value);
        if (value != null) return value;

        ErrorDictionary.TryGetValue(8001, out value);
        return value ?? new GlobalSignError
            { ErrorMessage = "Unknown error", ErrorDetails = "No error details available." };
    }
}

public class GlobalSignError
{
    public int SuccessCode { get; set; }
    public int ErrorCode { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorDetails { get; set; } = string.Empty;
    public uint HResult { get; set; }

    public string Message
    {
        get
        {
            if (string.IsNullOrEmpty(ErrorMessage)) return ErrorDetails;

            return ErrorMessage;
        }
    }

    public string DetailedMessage => (ErrorMessage + " " + ErrorDetails).Trim();
}