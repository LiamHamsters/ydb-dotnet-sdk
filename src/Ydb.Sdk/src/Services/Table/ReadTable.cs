﻿using Ydb.Sdk.Client;
using Ydb.Sdk.Value;
using Ydb.Table;
using Ydb.Table.V1;

namespace Ydb.Sdk.Services.Table;

public class ReadTableSettings : GrpcRequestSettings
{
    public List<string> Columns { get; set; } = new();

    public ulong RowLimit { get; set; } = 0;

    public bool Ordered { get; set; } = false;
}

public class ReadTablePart : ResponseWithResultBase<ReadTablePart.ResultData>
{
    internal ReadTablePart(Status status, ResultData? result = null)
        : base(status, result)
    {
    }

    public class ResultData
    {
        internal ResultData(Value.ResultSet resultSet)
        {
            ResultSet = resultSet;
        }

        public Value.ResultSet ResultSet { get; }

        internal static ResultData FromProto(ReadTableResult resultProto) => new(resultProto.ResultSet.FromProto());
    }
}

public class ReadTableStream : StreamResponse<ReadTableResponse, ReadTablePart>
{
    internal ReadTableStream(IServerStream<ReadTableResponse> iterator)
        : base(iterator)
    {
    }

    protected override ReadTablePart MakeResponse(Status status) => new(status);

    protected override ReadTablePart MakeResponse(ReadTableResponse protoResponse)
    {
        var status = Status.FromProto(protoResponse.Status, protoResponse.Issues);
        var result = status.IsSuccess && protoResponse.Result != null
            ? ReadTablePart.ResultData.FromProto(protoResponse.Result)
            : null;

        return new ReadTablePart(status, result);
    }
}

public partial class TableClient
{
    public async ValueTask<ReadTableStream> ReadTable(string tablePath, ReadTableSettings? settings = null)
    {
        settings ??= new ReadTableSettings();

        var request = new ReadTableRequest
        {
            Path = tablePath,
            Columns = { settings.Columns },
            RowLimit = settings.RowLimit,
            Ordered = settings.Ordered
        };

        var streamIterator = await _driver.ServerStreamCall(
            method: TableService.StreamReadTableMethod,
            request: request,
            settings: settings
        );

        return new ReadTableStream(streamIterator);
    }
}
