using WildNatureExplorer.Application.DTOs.Admin;
using WildNatureExplorer.Application.DTOs.AI;
using WildNatureExplorer.Application.DTOs.Auth;
using WildNatureExplorer.Application.DTOs.Library;
using WildNatureExplorer.Application.DTOs.Species;
using WildNatureExplorer.Application.DTOs.Users;
using WildNatureExplorer.Application.Interfaces.Services;
using WildNatureExplorer.Application.Validators;
using Moq;
using Xunit;

namespace WildNatureExplorer.Tests.Unit;

public class FluentValidatorsTests
{
    private static Mock<IUserService> MockUserService() => new();

    [Fact]
    [Trait("Category", "Unit")]
    public void LoginUserDtoValidator_InvalidEmail_Fails()
    {
        var v = new LoginUserDtoValidator();
        var r = v.Validate(new LoginUserDto("not-an-email", "Aa11!!aaaa"));
        Assert.False(r.IsValid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void LoginUserDtoValidator_Valid_Passes()
    {
        var v = new LoginUserDtoValidator();
        var r = v.Validate(new LoginUserDto("a@b.co", "Aa11!!aaaa"));
        Assert.True(r.IsValid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RegisterUserDtoValidator_Valid_Passes()
    {
        var v = new RegisterUserDtoValidator(MockUserService().Object);
        var r = v.Validate(new RegisterUserDto("me@here.co", "Aa11!!aaaa", "Joe", "Doe"));
        Assert.True(r.IsValid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RegisterUserDtoValidator_ShortPassword_Fails()
    {
        var v = new RegisterUserDtoValidator(MockUserService().Object);
        var r = v.Validate(new RegisterUserDto("me@here.co", "Aa1!", "Joe", "Doe"));
        Assert.False(r.IsValid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void UpdateUserDtoValidator_Valid_Passes()
    {
        var v = new UpdateUserDtoValidator(MockUserService().Object);
        var r = v.Validate(new UpdateUserDto("Jane", "Roe", "j@r.co"));
        Assert.True(r.IsValid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CreateSightingRequestValidator_InvalidLatitude_Fails()
    {
        var v = new CreateSightingRequestValidator();
        var r = v.Validate(new CreateSightingRequest { CommonName = "Bear", Latitude = 91, Longitude = 0 });
        Assert.False(r.IsValid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CreateSightingRequestValidator_Valid_Passes()
    {
        var v = new CreateSightingRequestValidator();
        var r = v.Validate(new CreateSightingRequest
        {
            CommonName = "Bear",
            Latitude = 45,
            Longitude = 10,
            Notes = null,
            ImageUrl = null
        });
        Assert.True(r.IsValid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void NearbySightingsRequestValidator_InvalidRadius_Fails()
    {
        var v = new NearbySightingsRequestValidator();
        var r = v.Validate(new NearbySightingsRequest { Latitude = 0, Longitude = 0, RadiusKm = 0 });
        Assert.False(r.IsValid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void NearbySightingsRequestValidator_Valid_Passes()
    {
        var v = new NearbySightingsRequestValidator();
        var r = v.Validate(new NearbySightingsRequest { Latitude = -45, Longitude = 120, RadiusKm = 10 });
        Assert.True(r.IsValid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SpeciesSearchDtoValidator_NoFilters_Fails()
    {
        var v = new SpeciesSearchDtoValidator();
        var r = v.Validate(new SpeciesSearchDto(null, null, null, null, null, null));
        Assert.False(r.IsValid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SpeciesSearchDtoValidator_WithCountryIds_Passes()
    {
        var v = new SpeciesSearchDtoValidator();
        var r = v.Validate(new SpeciesSearchDto(null, null, new List<Guid> { Guid.NewGuid() }, null, null, null));
        Assert.True(r.IsValid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SpeciesAutocompleteDtoValidator_Valid_Passes()
    {
        var v = new SpeciesAutocompleteDtoValidator();
        var r = v.Validate(new SpeciesAutocompleteDto("Li"));
        Assert.True(r.IsValid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AiQuestionDtoValidator_MustEndWithQuestionMark()
    {
        var v = new AiQuestionDtoValidator();
        var bad = v.Validate(new AiQuestionDto { QuestionAboutNature = "What is an elk" });
        Assert.False(bad.IsValid);

        var ok = v.Validate(new AiQuestionDto { QuestionAboutNature = "What is an elk?" });
        Assert.True(ok.IsValid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AiFeedbackDtoValidator_RatingBounds()
    {
        var v = new AiFeedbackDtoValidator();
        Assert.False(v.Validate(new AiFeedbackDto { Rating = 0, Comment = null }).IsValid);
        Assert.True(v.Validate(new AiFeedbackDto { Rating = 50, Comment = "ok" }).IsValid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AdminSpeciesImportDtoValidator_TooManyCountries_Fails()
    {
        var v = new AdminSpeciesImportDtoValidator();
        var dto = new AdminSpeciesImportDto(
            "A",
            "B sci",
            null,
            false,
            false,
            "c1,c2,c3,c4,c5,c6",
            "red",
            "forest",
            "Large");
        Assert.False(v.Validate(dto).IsValid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AdminSpeciesImportDtoValidator_Valid_Passes()
    {
        var v = new AdminSpeciesImportDtoValidator();
        var dto = new AdminSpeciesImportDto(
            "Fox",
            "Vulpes",
            null,
            false,
            false,
            "Italy",
            "Orange",
            "Forest",
            "Medium");
        Assert.True(v.Validate(dto).IsValid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AdminSpeciesCsvDtoValidator_MustBeCsv()
    {
        var v = new AdminSpeciesCsvDtoValidator();
        using var ms = new MemoryStream();
        Assert.False(v.Validate(new AdminSpeciesCsvDto { FileStream = ms, FileName = "x.txt" }).IsValid);

        ms.WriteByte((byte)'a');
        ms.Position = 0;
        Assert.True(v.Validate(new AdminSpeciesCsvDto { FileStream = ms, FileName = "data.csv" }).IsValid);
    }
}
