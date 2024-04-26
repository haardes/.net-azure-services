using Azure.Security.KeyVault.Secrets;
using AzureServices.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServices.Delta;

public interface IDeltaService
{
    public string? WarehouseId { get; }

    /// <summary>
    /// The <see cref="GetDeltaTableContent"/> operation queries a databricks SQL warehouse using the specified
    /// schema, statement and catalog, and returns the result as a CSV-formatted string.
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="statement"></param>
    /// <param name="catalog"></param>
    /// <param name="disposition"></param>
    /// <param name="format"></param>
    /// <remarks>
    /// <para>A <see cref="KeyNotFoundException"/> will be thrown if the external link does not exists in the response.</para>
    /// <para>An <see cref="ArgumentException"/> will be thrown if the response does not contain any columns in the schema.</para>
    /// <para>An <see cref="ArgumentNullException"/> will be thrown if the response does not contain any content.</para>
    /// <para>A <see cref="JsonException"/> will be thrown if the response cannot be deserialized as a <see cref="SqlWarehouseResponse"/>.</para>
    /// <para>An <see cref="Exception"/> will be thrown if deserialization of the response results in a <see cref="null"/>-value.</para>
    /// </remarks>
    string GetDeltaTableContent(string schema, string statement, string catalog = "hive_metastore", string disposition = "EXTERNAL_LINKS", string format = "csv");

    /// <summary>
    /// The <see cref="GetDeltaTableContent"/> operation queries a databricks SQL warehouse using the specified
    /// schema, statement and catalog, and returns the result as a CSV-formatted string.
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="statement"></param>
    /// <param name="parameters"></param>
    /// <param name="catalog"></param>
    /// <param name="disposition"></param>
    /// <param name="format"></param>
    /// <remarks>
    /// <para>A <see cref="KeyNotFoundException"/> will be thrown if the external link does not exists in the response.</para>
    /// <para>An <see cref="ArgumentException"/> will be thrown if the response does not contain any columns in the schema.</para>
    /// <para>An <see cref="ArgumentNullException"/> will be thrown if the response does not contain any content.</para>
    /// <para>A <see cref="JsonException"/> will be thrown if the response cannot be deserialized as a <see cref="SqlWarehouseResponse"/>.</para>
    /// <para>An <see cref="Exception"/> will be thrown if deserialization of the response results in a <see cref="null"/>-value.</para>
    /// </remarks>
    string GetDeltaTableContent(string schema, string statement, IEnumerable<QueryParameters> parameters, string catalog = "hive_metastore", string disposition = "EXTERNAL_LINKS", string format = "csv");

    /// <summary>
    /// The <see cref="GetDeltaTableContent"/> operation queries a databricks SQL warehouse using the specified
    /// schema, statement and catalog, and returns the result as a CSV-formatted string.
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="statement"></param>
    /// <param name="parameters"></param>
    /// <param name="catalog"></param>
    /// <param name="disposition"></param>
    /// <remarks>
    /// <para>A <see cref="KeyNotFoundException"/> will be thrown if the external link does not exists in the response.</para>
    /// <para>An <see cref="ArgumentException"/> will be thrown if the response does not contain any columns in the schema.</para>
    /// <para>An <see cref="ArgumentNullException"/> will be thrown if the response does not contain any content.</para>
    /// <para>A <see cref="JsonException"/> will be thrown if the response cannot be deserialized as a <see cref="SqlWarehouseResponse"/>.</para>
    /// <para>An <see cref="Exception"/> will be thrown if deserialization of the response results in a <see cref="null"/>-value.</para>
    /// </remarks>
    string GetDeltaTableContent(string schema, string statement, IEnumerable<QueryParameters> parameters, string catalog = "hive_metastore", string disposition = "EXTERNAL_LINKS");

    /// <summary>
    /// The <see cref="FetchMetadataAndAwaitSuccess"/> operation sends a POST-request to databricks with a 
    /// <see cref="SqlWarehouseQuery"/> and returns the resulting <see cref="SqlWarehouseResponse"/>.
    /// </summary>
    /// <param name="query"></param>
    /// <remarks>
    /// <para>An <see cref="ArgumentNullException"/> will be thrown if the response does not contain any content.</para>
    /// <para>A <see cref="JsonException"/> will be thrown if the response cannot be deserialized as a <see cref="SqlWarehouseResponse"/>.</para>
    /// <para>An <see cref="Exception"/> will be thrown if deserialization of the response results in a <see cref="null"/>-value.</para>
    /// </remarks>
    SqlWarehouseResponse FetchMetadataAndAwaitSuccess(SqlWarehouseQuery query);

    Result? FetchNextResult(Result currentResult);

    string FetchCsvFromResult(Result currentResult, SqlWarehouseResponse metadata, ref bool hasHeaders);

    /// <summary>
    /// The <see cref="AddAzureKeyVault(string, KeyOptions)"/> operation creates a <see cref="SecretClient"/> and extracts secrets 
    /// using the secret-names from the given <see cref="KeyOptions"/>, or the default secret names if not given. 
    /// Uses the underlying operation <see cref="AddAzureKeyVault(SecretClient, KeyOptions)"/>.
    /// </summary>
    /// <param name="keyVaultUri"></param>
    /// <param name="options"></param>
    /// <remarks>
    /// <para>An <see cref="Azure.RequestFailedException"/> will be thrown if the key-vault does not contain the listed secrets.</para>
    /// </remarks>
    DeltaService AddAzureKeyVault(string keyVaultUri, KeyOptions? options);

    /// <summary>
    /// The <see cref="AddAzureKeyVault(SecretClient, KeyOptions)"/> operation extracts secrets from a <see cref="SecretClient"/>, using the secret-names 
    /// from the given <see cref="KeyOptions"/>, or the default secret names if not given.
    /// </summary>
    /// <param name="secretClient"></param>
    /// <param name="options"></param>
    /// <remarks>
    /// <para>An <see cref="Azure.RequestFailedException"/> will be thrown if the key-vault does not contain the listed secrets.</para>
    /// </remarks>
    DeltaService AddAzureKeyVault(SecretClient secretClient, KeyOptions? options);

    /// <summary>
    /// The <see cref="IsInitialized"/> operation verifies that all necessary attributes has values.
    /// </summary>
    (bool IsInitialized, string Message) IsInitialized();
}