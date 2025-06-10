using Amazon;
using Amazon.BedrockRuntime;

using Microsoft.Extensions.Configuration;

namespace SemanticKernelChat.Helpers
{
    /// <summary>
    /// Helper methods for creating and configuring the AWS Bedrock runtime client.
    /// </summary>
    public static class BedrockHelper
    {
        /// <summary>
        /// Assumes an AWS role and returns an authenticated AmazonBedrockRuntimeClient.
        /// </summary>
        /// <param name="config">Configuration section containing RoleArn and credentials profile.</param>
        /// <returns>AmazonBedrockRuntimeClient with temporary credentials.</returns>
        public static async Task<AmazonBedrockRuntimeClient> GetBedrockRuntimeAsync(IConfiguration config)
        {
            // Get AWS credentials from the shared credentials file (default profile)
            var awsCredentials = new Amazon.Runtime.CredentialManagement.SharedCredentialsFile();
            var chain = new Amazon.Runtime.CredentialManagement.CredentialProfileStoreChain();
            chain.TryGetAWSCredentials("default", out var dev);

            // Prepare to assume the specified role
            var roleArn = config["RoleArn"];
            var roleSessionName = $"{Environment.MachineName}-{DateTime.UtcNow:yyyyMMdd'T'HHmmss'Z'}";

            // Create STS client and assume the role
            var stsClient = new Amazon.SecurityToken.AmazonSecurityTokenServiceClient(dev, RegionEndpoint.USEast1);
            var assumeRoleRequest = new Amazon.SecurityToken.Model.AssumeRoleRequest
            {
                RoleArn = roleArn,
                RoleSessionName = roleSessionName
            };
            var assumeResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest).ConfigureAwait(false);
            var credentials = assumeResponse.Credentials;

            // Create Bedrock runtime client with temporary credentials
            var bedrockRuntime = new AmazonBedrockRuntimeClient(new Amazon.Runtime.SessionAWSCredentials(
                credentials.AccessKeyId,
                credentials.SecretAccessKey,
                credentials.SessionToken
            ), RegionEndpoint.USEast1);

            return bedrockRuntime;
        }
    }
}
