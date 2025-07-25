using Ydb.Sdk.Client;
using Ydb.Sdk.Services.Operations;
using Ydb.Table;
using Ydb.Table.V1;

namespace Ydb.Sdk.Services.Table;

public class RollbackTransactionSettings : OperationSettings
{
}

public class RollbackTransactionResponse : ResponseBase
{
    internal RollbackTransactionResponse(Status status) : base(status)
    {
    }
}

public partial class Session
{
    public async Task<RollbackTransactionResponse> RollbackTransaction(
        string txId,
        RollbackTransactionSettings? settings = null)
    {
        CheckSession();
        settings ??= new RollbackTransactionSettings();

        var request = new RollbackTransactionRequest
        {
            TxId = txId,
            SessionId = Id,
            OperationParams = settings.MakeOperationParams()
        };

        var response = await UnaryCall(TableService.RollbackTransactionMethod, request, settings);

        var status = response.Operation.Unpack();
        OnResponseStatus(status);

        return new RollbackTransactionResponse(status);
    }
}
