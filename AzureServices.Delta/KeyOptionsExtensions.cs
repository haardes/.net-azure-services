using AzureServices.Core;

namespace AzureServices.Delta;

public static class KeyOptionsExtensions
{
    private static string _workspaceId = "DatabricksWorkspaceId";
    private static string _warehouseId = "DatabricksWarehouseId";
    private static string _warehouseToken = "DatabricksApiToken";

    public static ref readonly string DatabricksWorkspaceId(this KeyOptions keyOptions)
    {
        return ref _workspaceId;
    }

    public static KeyOptions DatabricksWorkspaceId(this KeyOptions keyOptions, string keyName)
    {
        _workspaceId = keyName;
        return keyOptions;
    }

    public static ref readonly string DatabricksWarehouseId(this KeyOptions keyOptions)
    {
        return ref _warehouseId;
    }

    public static KeyOptions DatabricksWarehouseId(this KeyOptions keyOptions, string keyName)
    {
        _warehouseId = keyName;
        return keyOptions;
    }

    public static ref readonly string DatabricksApiToken(this KeyOptions keyOptions)
    {
        return ref _warehouseToken;
    }

    public static KeyOptions DatabricksApiToken(this KeyOptions keyOptions, string keyName)
    {
        _warehouseToken = keyName;
        return keyOptions;
    }
}