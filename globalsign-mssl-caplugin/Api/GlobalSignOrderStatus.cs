// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using Keyfactor.PKI.Enums.EJBCA;

namespace Keyfactor.Extensions.CAPlugin.GlobalSign.Api;

public enum GlobalSignOrderStatus
{
    Initial = 1,
    Waiting = 2,
    Canceled = 3,
    Issued = 4,
    Cancelled = 5,
    Revoking = 6,
    Revoked = 7,
    PendingApproval = 8,
    Locked = 9,
    Denied = 10
}

public static class OrderStatus
{
    public static int ConvertToKeyfactorStatus(GlobalSignOrderStatus status)
    {
        switch (status)
        {
            case GlobalSignOrderStatus.Issued:
                return (int)EndEntityStatus.GENERATED;
            case GlobalSignOrderStatus.Revoked:
                return (int)EndEntityStatus.REVOKED;
            case GlobalSignOrderStatus.Revoking:
                return (int)EndEntityStatus.REVOKED;
            case GlobalSignOrderStatus.PendingApproval:
                return (int)EndEntityStatus.WAITINGFORADDAPPROVAL;
            case GlobalSignOrderStatus.Waiting:
                return (int)EndEntityStatus.INPROCESS;
            case GlobalSignOrderStatus.Initial:
                return (int)EndEntityStatus.INITIALIZED;
            case GlobalSignOrderStatus.Denied:
                return (int)EndEntityStatus.FAILED;
            case GlobalSignOrderStatus.Canceled:
                return (int)EndEntityStatus.CANCELLED;
            case GlobalSignOrderStatus.Cancelled:
                return (int)EndEntityStatus.CANCELLED;
            default:
                return (int)EndEntityStatus.FAILED;
        }
    }
}