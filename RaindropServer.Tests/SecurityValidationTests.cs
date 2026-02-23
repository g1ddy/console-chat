using System.ComponentModel.DataAnnotations;
using RaindropServer.Raindrops;
using Xunit;
using System.Collections.Generic;

namespace RaindropServer.Tests;

public class SecurityValidationTests
{
    [Theory]
    [InlineData("not-a-url")]
    [InlineData("javascript:alert(1)")]
    [InlineData("file:///etc/passwd")]
    public void RaindropCreateRequest_InvalidLink_ShouldBeInvalid(string invalidLink)
    {
        var request = new RaindropCreateRequest { Link = invalidLink };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid, $"Link '{invalidLink}' should be invalid");
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://example.com/path?query=1")]
    public void RaindropCreateRequest_ValidLink_ShouldBeValid(string validLink)
    {
        var request = new RaindropCreateRequest { Link = validLink };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.True(isValid, $"Link '{validLink}' should be valid");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("javascript:alert(1)")]
    [InlineData("file:///etc/passwd")]
    public void RaindropUpdateRequest_InvalidLink_ShouldBeInvalid(string invalidLink)
    {
        var request = new RaindropUpdateRequest { Link = invalidLink };
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(request, context, results, true);

        Assert.False(isValid, $"Link '{invalidLink}' should be invalid for update");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("javascript:alert(1)")]
    [InlineData("file:///etc/passwd")]
    public void Raindrop_InvalidLink_ShouldBeInvalid(string invalidLink)
    {
        var item = new Raindrop { Link = invalidLink };
        var context = new ValidationContext(item);
        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(item, context, results, true);

        Assert.False(isValid, $"Link '{invalidLink}' should be invalid for Raindrop record");
    }
}
