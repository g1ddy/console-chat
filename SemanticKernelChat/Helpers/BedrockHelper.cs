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
        public static Task<AmazonBedrockRuntimeClient> GetBedrockRuntimeAsync(IConfiguration config)
        {
            // Load AWS credentials using the default provider chain
            var baseCredentials = Amazon.Runtime.Credentials.DefaultAWSCredentialsIdentityResolver.GetCredentials();

            // Assume the specified role using automatic credential refreshing
            var assumeRoleCredentials = new Amazon.Runtime.AssumeRoleAWSCredentials(
                baseCredentials,
                config["RoleArn"],
                $"{Environment.MachineName}-{DateTime.UtcNow:yyyyMMdd'T'HHmmss'Z'}"
            );

            // Create Bedrock runtime client with the assumed role credentials
            var bedrockRuntime = new AmazonBedrockRuntimeClient(assumeRoleCredentials, RegionEndpoint.USEast1);

            return Task.FromResult(bedrockRuntime);
        }
    }
}
