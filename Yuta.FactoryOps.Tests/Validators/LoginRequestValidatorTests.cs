using FluentValidation.TestHelper;
using Yuta.FactoryOps.Application.Validators;
using Yuta.FactoryOps.Domain.DTOs;

namespace Yuta.FactoryOps.Tests.Validators
{
    public class LoginRequestValidatorTests
    {
        private readonly LoginRequestValidator _validator;

        public LoginRequestValidatorTests()
        {
            _validator = new LoginRequestValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Empty()
        {
            var model = new LoginRequestDto { Email = "", Senha = "123456" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Invalid()
        {
            var model = new LoginRequestDto { Email = "invalid-email", Senha = "123456" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Have_Error_When_Senha_Is_Empty()
        {
            var model = new LoginRequestDto { Email = "test@example.com", Senha = "" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Senha);
        }

        [Fact]
        public void Should_Have_Error_When_Senha_Is_Too_Short()
        {
            var model = new LoginRequestDto { Email = "test@example.com", Senha = "123" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Senha);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Model_Is_Valid()
        {
            var model = new LoginRequestDto { Email = "test@example.com", Senha = "123456" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}