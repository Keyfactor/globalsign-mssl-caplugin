using System.Globalization;
using System.ServiceModel;
using Keyfactor.AnyGateway.Extensions;
using Keyfactor.Extensions.CAPlugin.GlobalSign.Api;
using Keyfactor.Logging;
using Keyfactor.PKI;
using Keyfactor.PKI.Enums.EJBCA;
using Keyfactor.PKI.X509;
using Microsoft.Extensions.Logging;
using Order;
using Query;
using OrderRequestHeader = Order.OrderRequestHeader;
using QueryRequestHeader = Query.QueryRequestHeader;

namespace Keyfactor.Extensions.CAPlugin.GlobalSign.Client;

public class GlobalSignApiClient
{
    private readonly GlobalSignCAConfig Config;
    private ILogger Logger;
    public ManagedSSLV2 OrderService;
    public GASV1 QueryService;

    public GlobalSignApiClient(GlobalSignCAConfig config, ILogger logger)
    {
        Logger = logger;
        Config = config;
        // Logger = LogHandler.GetClassLogger(this.GetType());
        QueryService = new GASV1Client
        {
            Endpoint = { Address = new EndpointAddress(config.GetUrl(GlobalSignServiceType.QUERY)), Name = "QUERY" }
        };
        OrderService = new ManagedSSLV2Client
        {
            Endpoint = { Address = new EndpointAddress(config.GetUrl(GlobalSignServiceType.ORDER)), Name = "ORDER" }
        };
    }

    public async Task<List<OrderDetail>> GetCertificatesForSyncAsync(
        bool fullSync,
        DateTime? lastSync,
        DateTime startDate,
        int intervalDays)
    {
        Logger.LogDebug("Getting certificates for sync (async)");

        var results = new List<OrderDetail>();
        if (fullSync)
        {
            // If startDate is before year 2000, treat it as “since the dawn of time”
            var from = startDate > new DateTime(2000, 1, 1)
                ? startDate
                : DateTime.MinValue;
            var finalStop = DateTime.UtcNow;

            // first window
            var end = from.AddDays(intervalDays);
            if (end > finalStop) end = finalStop;
            results.AddRange(
                await GetCertificatesByDateRange(from, end));

            // subsequent windows
            while (end < finalStop)
            {
                from = end.AddSeconds(1);
                end = from.AddDays(intervalDays);
                if (end > finalStop) end = finalStop;

                results.AddRange(
                    await GetCertificatesByDateRange(from, end));
            }
        }
        else
        {
            // incremental sync since lastSync
            var from = lastSync;
            var to = DateTime.UtcNow;

            results.AddRange(
                await GetCertificatesByDateRange(from, to)
            );
        }

        return results;
    }


    private async Task<List<OrderDetail>> GetCertificatesByDateRange(DateTime? fromDate, DateTime? toDate)
    {
        var tmpFromDate = fromDate ?? DateTime.MinValue;
        var tmpToDate = toDate ?? DateTime.UtcNow;
        QbV1GetOrderByDateRangeRequest req = new QbV1GetOrderByDateRangeRequest
        {
            QueryRequestHeader = new Query.QueryRequestHeader
            {
                AuthToken = Config.GetQueryAuthToken()
            },
            FromDate = tmpFromDate.ToString(Config.DateFormatString, DateTimeFormatInfo.InvariantInfo),
            ToDate = tmpToDate.ToString(Config.DateFormatString, DateTimeFormatInfo.InvariantInfo),
            OrderQueryOption = new OrderQueryOption
            {
                ReturnOrderOption = "true",
                ReturnCertificateInfo = "true",
                ReturnFulfillment = "true",
                ReturnOriginalCSR = "true"
            }
        };
        Logger.LogDebug($"Retrieving all orders between {tmpFromDate} and {tmpToDate}");
        var allOrdersResponse = await QueryService.GetOrderByDateRangeAsync(new GetOrderByDateRange(req));

        if (allOrdersResponse.Response.QueryResponseHeader.SuccessCode == 0)
        {
            var retVal = allOrdersResponse.Response.OrderDetails?.ToList() ?? new List<OrderDetail>();
            Logger.LogDebug($"Retrieved {retVal.Count} orders from GlobalSign");
            return retVal;
        }
        else
        {
            int errCode = int.Parse(allOrdersResponse.Response.QueryResponseHeader.Errors[0].ErrorCode);
            Logger.LogError($"Unable to retrieve certificates:");
            foreach (var e in allOrdersResponse.Response.QueryResponseHeader.Errors)
            {
                Logger.LogError($"{e.ErrorCode} | {e.ErrorField} | {e.ErrorMessage}");
            }
            var gsError = GlobalSignErrorIndex.GetGlobalSignError(errCode);
            Logger.LogError(gsError.DetailedMessage);
            throw new Exception(gsError.Message);
        }
    }

    public async Task<AnyCAPluginCertificate> PickupCertificateById(string caRequestId)
    {
        Logger.MethodEntry();
        Logger.LogDebug($"Attempting to pick up order with order ID {caRequestId}");

        var request = new QbV1GetOrderByOrderIdRequest
        {
            QueryRequestHeader = new QueryRequestHeader
            {
                AuthToken = Config.GetQueryAuthToken()
            },
            OrderID = caRequestId,
            OrderQueryOption = new OrderQueryOption
            {
                ReturnCertificateInfo = "true",
                ReturnOriginalCSR = "true",
                ReturnFulfillment = "true"
            }
        };

        var retryCounter = 0;
        while (retryCounter <= Config.PickupRetries)
        {
            var wrapper = new GetOrderByOrderID(request);
            var responseWrapper = await QueryService.GetOrderByOrderIDAsync(wrapper);
            var response = responseWrapper.Response;

            if (response.OrderResponseHeader.SuccessCode == 0)
            {
                Logger.LogDebug($"Order with order ID {caRequestId} successfully picked up");
                var orderStatus = (GlobalSignOrderStatus)Enum.Parse(
                    typeof(GlobalSignOrderStatus),
                    response.OrderDetail.CertificateInfo.CertificateStatus);

                if (orderStatus == GlobalSignOrderStatus.Issued)
                {
                    var orderDate = DateTime.TryParse(
                        response.OrderDetail.OrderInfo.OrderDate,
                        out var od)
                        ? od
                        : (DateTime?)null;
                    var completeDate = DateTime.TryParse(
                        response.OrderDetail.OrderInfo.OrderCompleteDate,
                        out var cd)
                        ? cd
                        : (DateTime?)null;
                    var deactivateDate = DateTime.TryParse(
                        response.OrderDetail.OrderInfo.OrderDeactivatedDate,
                        out var de)
                        ? de
                        : (DateTime?)null;

                    Logger.MethodExit();
                    return new AnyCAPluginCertificate
                    {
                        CARequestID = caRequestId,
                        ProductID = response.OrderDetail.OrderInfo.ProductCode,
                        Status = OrderStatus.ConvertToKeyfactorStatus(orderStatus),
                        CSR = response.OrderDetail.Fulfillment.OriginalCSR,
                        Certificate = response.OrderDetail.Fulfillment.ServerCertificate.X509Cert,
                        RevocationReason = 0,
                        RevocationDate = orderStatus == GlobalSignOrderStatus.Revoked ? deactivateDate : null
                    };
                }
            }

            retryCounter++;
            Logger.LogDebug(
                $"Pickup certificate failed for order ID {caRequestId}. Attempt {retryCounter} of {Config.PickupRetries}.{(retryCounter < Config.PickupRetries ? " Retrying..." : string.Empty)}");
            await Task.Delay(TimeSpan.FromSeconds(Config.PickupDelay));
        }

        var gsError = GlobalSignErrorIndex.GetGlobalSignError(-9916);
        var errorMsg =
            "Unable to pickup certificate during configured pickup window. Check for required approvals in GlobalSign portal. This can also be caused by a delay with GlobalSign, in which case the certificate will get picked up by a future sync";
        Logger.LogError(errorMsg);
        Logger.LogError(gsError.DetailedMessage);
        throw new Exception(errorMsg);
    }

    public async Task<List<GetDomainsDomainDetail>> GetDomains()
    {
        Logger.MethodEntry();
        var requestWrapper = new GetDomains(
            new BmV1GetDomainsRequest
            {
                QueryRequestHeader = new Order.QueryRequestHeader
                {
                    AuthToken = Config.GetOrderAuthToken()
                }
            });

        var responseWrapper = await OrderService.GetDomainsAsync(requestWrapper
            )
            ;
        var response = responseWrapper.Response;

        if (response.QueryResponseHeader.SuccessCode == 0)
        {
            var retVal = response.DomainDetails?.ToList() ?? new List<GetDomainsDomainDetail>();
            Logger.LogDebug($"Successfully retrieved {retVal.Count} domains");
            return retVal;
        }

        var errCode = int.Parse(response.QueryResponseHeader.Errors[0].ErrorCode);
        var gsError = GlobalSignErrorIndex.GetGlobalSignError(errCode);
        Logger.LogError(gsError.DetailedMessage);
        throw new Exception(gsError.Message);
    }

    public async Task<List<SearchMsslProfileDetail>> GetProfiles()
    {
        Logger.MethodEntry();
        var requestWrapper = new GetMSSLProfiles(
            new BmV1GetMsslProfilesRequest
            {
                QueryRequestHeader = new Order.QueryRequestHeader
                {
                    AuthToken = Config.GetOrderAuthToken()
                }
            });

        var responseWrapper = await OrderService.GetMSSLProfilesAsync(requestWrapper)
            ;
        var response = responseWrapper.Response;

        if (response.QueryResponseHeader.SuccessCode == 0)
        {
            var retVal = response.SearchMSSLProfileDetails.ToList();
            Logger.LogDebug($"Successfully retrieved {retVal.Count} profiles");
            return retVal;
        }

        var errCode = int.Parse(response.QueryResponseHeader.Errors[0].ErrorCode);
        var gsError = GlobalSignErrorIndex.GetGlobalSignError(errCode);
        Logger.LogError(gsError.DetailedMessage);
        throw new Exception(gsError.Message);
    }

    public async Task<EnrollmentResult> Enroll(GlobalSignEnrollRequest enrollRequest)
    {
        Logger.MethodEntry();
        var rawRequest = enrollRequest.Request;
        Logger.LogTrace("Request details:");
        Logger.LogTrace($"Profile ID: {enrollRequest.MsslProfileId}");
        Logger.LogTrace($"Domain ID: {enrollRequest.MsslDomainId}");
        Logger.LogTrace(
            $"Contact Info: {enrollRequest.FirstName}, {enrollRequest.LastName}, {enrollRequest.Email}, {enrollRequest.Phone}");
        Logger.LogTrace($"SAN Count: {enrollRequest.SANs.Count()}");
        if (rawRequest.SANEntries.Count() > 0)
            Logger.LogTrace($"SANs: {string.Join(",", rawRequest.SANEntries.Select(s => s.SubjectAltName))}");
        Logger.LogTrace($"Product Code: {rawRequest.OrderRequestParameter.ProductCode}");
        Logger.LogTrace($"Order Kind: {rawRequest.OrderRequestParameter.OrderKind}");
        if (!string.IsNullOrEmpty(rawRequest.OrderRequestParameter.BaseOption))
            Logger.LogTrace($"Order Base Option: {rawRequest.OrderRequestParameter.BaseOption}");

        var requestwrapper = new PVOrder(enrollRequest.Request);
        var responsewrapper = await OrderService.PVOrderAsync(requestwrapper);
        ;
        var response = responsewrapper.Response;
        if (response.OrderResponseHeader.SuccessCode == 0)
        {
            Logger.LogDebug("Enrollment request successfully submitted");
            var certStatus = (GlobalSignOrderStatus)Enum.Parse(typeof(GlobalSignOrderStatus),
                response.PVOrderDetail.CertificateInfo.CertificateStatus);

            switch (certStatus)
            {
                case GlobalSignOrderStatus.Issued:
                    return new EnrollmentResult
                    {
                        CARequestID = response.OrderID,
                        Certificate = response.PVOrderDetail.Fulfillment.ServerCertificate.X509Cert,
                        Status = (int)EndEntityStatus.GENERATED
                    };

                case GlobalSignOrderStatus.PendingApproval:
                case GlobalSignOrderStatus.Waiting:
                    return new EnrollmentResult
                    {
                        CARequestID = response.OrderID,
                        Status = (int)EndEntityStatus.WAITINGFORADDAPPROVAL,
                        StatusMessage = "Enrollment is pending review.  Check GlobalSign Portal for more detail."
                    };
            }
        }

        var errorCode = int.Parse(response.OrderResponseHeader.Errors[0].ErrorCode);
        var err = GlobalSignErrorIndex.GetGlobalSignError(errorCode);
        foreach (var e in response.OrderResponseHeader.Errors)
            Logger.LogError($"{e.ErrorCode}|{e.ErrorField}|{e.ErrorMessage}");
        return new EnrollmentResult
        {
            Status = (int)PKIConstants.Microsoft.RequestDisposition.FAILED,
            StatusMessage = $"Enrollment failed. {err.DetailedMessage}"
        };
    }

    public async Task<EnrollmentResult> Renew(GlobalSignRenewRequest renewRequest)
    {
        Logger.MethodEntry(LogLevel.Debug);
        var requestWrapper = new PVOrder(renewRequest.Request);
        Logger.LogTrace("Request details:");
        Logger.LogTrace($"Profile ID: {requestWrapper.Request.MSSLProfileID}");
        Logger.LogTrace($"Domain ID: {requestWrapper.Request.MSSLDomainID}");
        Logger.LogTrace(
            $"Contact Info: {requestWrapper.Request.ContactInfo.FirstName}, {requestWrapper.Request.ContactInfo.LastName}, {requestWrapper.Request.ContactInfo.Email}, {requestWrapper.Request.ContactInfo.Phone}");
        Logger.LogTrace($"SAN Count: {requestWrapper.Request.SANEntries.Count()}");
        if (requestWrapper.Request.SANEntries.Count() > 0)
            Logger.LogTrace(
                $"SANs: {string.Join(",", requestWrapper.Request.SANEntries.Select(s => s.SubjectAltName))}");

        Logger.LogTrace($"Product Code: {requestWrapper.Request.OrderRequestParameter.ProductCode}");
        Logger.LogTrace($"Order Kind: {requestWrapper.Request.OrderRequestParameter.OrderKind}");
        if (!string.IsNullOrEmpty(requestWrapper.Request.OrderRequestParameter.BaseOption))
            Logger.LogTrace($"Order Base Option: {requestWrapper.Request.OrderRequestParameter.BaseOption}");

        Logger.LogTrace($"Renewal Target: {requestWrapper.Request.OrderRequestParameter.RenewalTargetOrderID}");
        var responseWrapper = await OrderService.PVOrderAsync(requestWrapper);
        ;
        var response = responseWrapper.Response;
        if (response.OrderResponseHeader.SuccessCode == 0)
        {
            Logger.LogDebug("Renewal request successfully submitted");
            var certStatus = (GlobalSignOrderStatus)Enum.Parse(typeof(GlobalSignOrderStatus),
                response.PVOrderDetail.CertificateInfo.CertificateStatus);

            switch (certStatus)
            {
                case GlobalSignOrderStatus.Issued:
                    return new EnrollmentResult
                    {
                        CARequestID = response.OrderID,
                        Certificate = response.PVOrderDetail.Fulfillment.ServerCertificate.X509Cert,
                        Status = (int)EndEntityStatus.GENERATED
                    };

                case GlobalSignOrderStatus.PendingApproval:
                case GlobalSignOrderStatus.Waiting:
                    return new EnrollmentResult
                    {
                        CARequestID = response.OrderID,
                        Status = (int)EndEntityStatus.WAITINGFORADDAPPROVAL,
                        StatusMessage = "Enrollment is pending review.  Check GlobalSign Portal for more detail."
                    };
            }
        }

        var errorCode = int.Parse(response.OrderResponseHeader.Errors[0].ErrorCode);
        var err = GlobalSignErrorIndex.GetGlobalSignError(errorCode);
        foreach (var e in response.OrderResponseHeader.Errors)
            Logger.LogError($"{e.ErrorCode}|{e.ErrorField}|{e.ErrorMessage}");
        if (errorCode <= -101 && errorCode >= -104) // Invalid parameter errors, provide more information
            err.ErrorDetails = string.Format(err.ErrorDetails, response.OrderResponseHeader.Errors[0].ErrorField);

        foreach (var e in response.OrderResponseHeader.Errors)
            Logger.LogError($"{e.ErrorCode}|{e.ErrorField}|{e.ErrorMessage}");

        return new EnrollmentResult
        {
            Status = (int)PKIConstants.Microsoft.RequestDisposition.FAILED,
            StatusMessage = $"Enrollment failed. {err.DetailedMessage}"
        };
    }


    public async Task<EnrollmentResult> Reissue(GlobalSignReissueRequest reissueRequest, string priorSn)
    {
        Logger.MethodEntry();
        var requestwrapper = new ReIssue(reissueRequest.Request);

        // Synchronous reissue call on QueryService
        var responsewrapper = await QueryService.ReIssueAsync(requestwrapper);
        ;
        var response = responsewrapper.Response;
        if (response.OrderResponseHeader.SuccessCode == 0)
        {
            Logger.LogDebug("Reissue request successfully submitted");

            // Pick up the certificate after reissue
            var pickupResponse = await PickupCertificateById(response.OrderID);
            var cert = CertificateConverterFactory.FromPEM(pickupResponse.Certificate).ToX509Certificate2();

            // If newly generated or serial differs, return success
            if (pickupResponse.Status == (int)EndEntityStatus.GENERATED || cert.SerialNumber != priorSn)
                return new EnrollmentResult
                {
                    CARequestID = response.OrderID,
                    Status = (int)EndEntityStatus.GENERATED,
                    Certificate = pickupResponse.Certificate
                };
        }

        // On failure, log all errors and return a failed result
        var err = GlobalSignErrorIndex.GetGlobalSignError(
            int.Parse(response.OrderResponseHeader.Errors[0].ErrorCode));
        foreach (var e in response.OrderResponseHeader.Errors)
            Logger.LogError($"{e.ErrorCode}|{e.ErrorField}|{e.ErrorMessage}");

        return new EnrollmentResult
        {
            Status = (int)EndEntityStatus.FAILED,
            StatusMessage = $"Enrollment failed. {err.DetailedMessage}"
        };
    }

    public async Task<int> RevokeCertificateById(string caRequestId)
    {
        Logger.MethodEntry();
        var request = new BmV1ModifyMsslOrderRequest
        {
            OrderRequestHeader = new OrderRequestHeader { AuthToken = Config.GetOrderAuthToken() },
            OrderID = caRequestId,
            ModifyOrderOperation = "Revoke"
        };

        Logger.LogDebug($"Attempting to revoke certificate with request ID {caRequestId}");
        var wrapper = new ModifyMSSLOrder(request);

        // Call the async revoke operation
        var responseWrapper = await OrderService
                .ModifyMSSLOrderAsync(wrapper)
            ;
        var response = responseWrapper.Response;

        if (response.OrderResponseHeader.SuccessCode == 0)
        {
            Logger.LogDebug($"Certificate with request ID {caRequestId} successfully revoked");
            return (int)EndEntityStatus.REVOKED;
        }

        // Log all errors and throw
        var errCode = int.Parse(response.OrderResponseHeader.Errors[0].ErrorCode);
        foreach (var e in response.OrderResponseHeader.Errors)
            Logger.LogError($"{e.ErrorCode}|{e.ErrorField}|{e.ErrorMessage}");

        var gsError = GlobalSignErrorIndex.GetGlobalSignError(errCode);
        Logger.LogError(gsError.DetailedMessage);
        throw new Exception(gsError.Message);
    }
}