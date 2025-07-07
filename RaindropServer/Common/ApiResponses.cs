namespace RaindropServer.Common;

public record ItemResponse<T>(bool Result, T Item);

public record ItemsResponse<T>(bool Result, List<T> Items);

public record SuccessResponse(bool Result);
