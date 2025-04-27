using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using CopilotTester.Models;
using CopilotTester.Services;
using NSubstitute;
using Shouldly;
using System;

namespace CopilotTester.UnitTests.Services;

public class LoanEvaluationServiceTests
{
    #region Custom Attributes
    
    public class AutoNSubstituteDataAttribute : AutoDataAttribute
    {
        public AutoNSubstituteDataAttribute()
            : base(() => new Fixture()
                .Customize(new AutoNSubstituteCustomization()))
        {
        }
    }
    
    public class InlineAutoDataAttribute : CompositeDataAttribute
    {
        public InlineAutoDataAttribute(params object[] values)
            : base(new InlineDataAttribute(values), new AutoNSubstituteDataAttribute())
        {
        }
    }
    
    #endregion

    [Theory]
    [AutoNSubstituteData]
    public void Evaluate_ShouldThrowArgumentNullException_WhenApplicationIsNull(
        LoanEvaluationService sut)
    {
        // Arrange
        LoanApplication? application = null;

        // Act + Assert
        Should.Throw<ArgumentNullException>(() => 
            this.InvokeEvaluate(sut, application!));
    }

    [Theory]
    [InlineAutoData(0)]
    [InlineAutoData(-100)]
    public void Evaluate_ShouldDenyLoan_WhenAmountIsZeroOrNegative(
        decimal invalidAmount,
        LoanApplication application,
        LoanEvaluationService sut)
    {
        // Arrange
        application = application with { Amount = invalidAmount };

        // Act
        var result = this.InvokeEvaluate(sut, application);

        // Assert
        result.Approved.ShouldBeFalse();
        result.Reason.ShouldBe("Invalid loan amount.");
    }

    [Theory]
    [InlineAutoData(5)]
    [InlineAutoData(0)]
    [InlineAutoData(-1)]
    [InlineAutoData(85)]
    [InlineAutoData(100)]
    public void Evaluate_ShouldDenyLoan_WhenLoanTermIsOutOfRange(
        int invalidTermMonths,
        LoanApplication application,
        LoanEvaluationService sut)
    {
        // Arrange
        application = application with { Amount = 1000, LoanTermMonths = invalidTermMonths };

        // Act
        var result = this.InvokeEvaluate(sut, application);

        // Assert
        result.Approved.ShouldBeFalse();
        result.Reason.ShouldBe("Loan term must be between 6 and 84 months.");
    }

    [Theory]
    [InlineAutoData(17)]
    [InlineAutoData(0)]
    [InlineAutoData(71)]
    [InlineAutoData(100)]
    public void Evaluate_ShouldDenyLoan_WhenAgeIsOutOfRange(
        int invalidAge,
        LoanApplication application,
        LoanEvaluationService sut)
    {
        // Arrange
        application = application with { 
            Amount = 1000, 
            LoanTermMonths = 12,
            Age = invalidAge 
        };

        // Act
        var result = this.InvokeEvaluate(sut, application);

        // Assert
        result.Approved.ShouldBeFalse();
        result.Reason.ShouldBe("Applicant must be between 18 and 70 years old.");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Evaluate_ShouldDenyLoan_WhenEmploymentStatusIsUnacceptable(
        LoanApplication application,
        LoanEvaluationService sut)
    {
        // Arrange - Setting an invalid employment status value using reflection 
        // since the enum only has valid values (Employed, SelfEmployed)
        var invalidStatus = (EmploymentStatus)99;
        application = application with { 
            Amount = 1000, 
            LoanTermMonths = 12,
            Age = 30,
            EmploymentStatus = invalidStatus
        };

        // Act
        var result = this.InvokeEvaluate(sut, application);

        // Assert
        result.Approved.ShouldBeFalse();
        result.Reason.ShouldBe("Unacceptable employment status.");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Evaluate_ShouldDenyLoan_WhenApplicantHasPreviousDefaults(
        LoanApplication application,
        LoanEvaluationService sut)
    {
        // Arrange
        application = application with { 
            Amount = 1000, 
            LoanTermMonths = 12,
            Age = 30,
            EmploymentStatus = EmploymentStatus.Employed,
            HasPreviousDefaults = true
        };

        // Act
        var result = this.InvokeEvaluate(sut, application);

        // Assert
        result.Approved.ShouldBeFalse();
        result.Reason.ShouldBe("Applicant has a history of defaults.");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Evaluate_ShouldDenyLoan_WhenCreditScoreIsTooLow(
        LoanApplication application,
        [Frozen] ICreditScoreService creditScoreService,
        LoanEvaluationService sut)
    {
        // Arrange
        application = application with { 
            Amount = 1000, 
            LoanTermMonths = 12,
            Age = 30,
            EmploymentStatus = EmploymentStatus.Employed,
            HasPreviousDefaults = false
        };

        creditScoreService.GetScore(application.CustomerId).Returns(599); // Below minimum 600

        // Act
        var result = this.InvokeEvaluate(sut, application);

        // Assert
        result.Approved.ShouldBeFalse();
        result.Reason.ShouldBe("Low credit score.");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Evaluate_ShouldDenyLoan_WhenIncomeCannotBeVerified(
        LoanApplication application,
        [Frozen] ICreditScoreService creditScoreService,
        [Frozen] IIncomeVerificationService incomeService,
        LoanEvaluationService sut)
    {
        // Arrange
        application = application with { 
            Amount = 1000, 
            LoanTermMonths = 12,
            Age = 30,
            EmploymentStatus = EmploymentStatus.Employed,
            HasPreviousDefaults = false
        };

        creditScoreService.GetScore(application.CustomerId).Returns(700);
        incomeService.GetMonthlyIncome(application.CustomerId).Returns(0); // Zero income

        // Act
        var result = this.InvokeEvaluate(sut, application);

        // Assert
        result.Approved.ShouldBeFalse();
        result.Reason.ShouldBe("Income could not be verified.");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Evaluate_ShouldDenyLoan_WhenDebtToIncomeRatioIsTooHigh(
        LoanApplication application,
        [Frozen] ICreditScoreService creditScoreService,
        [Frozen] IIncomeVerificationService incomeService,
        LoanEvaluationService sut)
    {
        // Arrange
        var monthlyIncome = 1000m;
        application = application with { 
            Amount = 12000, 
            LoanTermMonths = 12, // Monthly installment = 1000
            Age = 30,
            EmploymentStatus = EmploymentStatus.Employed,
            HasPreviousDefaults = false,
            MonthlyDebt = 0
        };

        // Make the debt-to-income ratio exactly 0.5 (exceeds 0.4 max)
        var monthlyInstallment = application.Amount / application.LoanTermMonths;
        var totalMonthlyDebt = application.MonthlyDebt + monthlyInstallment;
        
        // Required income to make DTI = 0.5
        var requiredIncome = totalMonthlyDebt / 0.5m;
        
        creditScoreService.GetScore(application.CustomerId).Returns(700);
        incomeService.GetMonthlyIncome(application.CustomerId).Returns(monthlyIncome);

        // Act
        var result = this.InvokeEvaluate(sut, application);

        // Assert
        result.Approved.ShouldBeFalse();
        result.Reason.ShouldBe("High debt-to-income ratio.");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Evaluate_ShouldDenyLoan_WhenAmountExceedsAnnualIncome(
        LoanApplication application,
        [Frozen] ICreditScoreService creditScoreService,
        [Frozen] IIncomeVerificationService incomeService,
        LoanEvaluationService sut)
    {
        // Arrange
        var monthlyIncome = 1000m;
        var annualIncome = monthlyIncome * 12;
        
        application = application with { 
            Amount = annualIncome + 1, // Just above annual income
            LoanTermMonths = 60,       // Low monthly payment
            Age = 30,
            EmploymentStatus = EmploymentStatus.Employed,
            HasPreviousDefaults = false,
            MonthlyDebt = 0
        };

        creditScoreService.GetScore(application.CustomerId).Returns(700);
        incomeService.GetMonthlyIncome(application.CustomerId).Returns(monthlyIncome);

        // Act
        var result = this.InvokeEvaluate(sut, application);

        // Assert
        result.Approved.ShouldBeFalse();
        result.Reason.ShouldBe("Loan amount exceeds annual income.");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Evaluate_ShouldApproveLoan_WhenAllCriteriaAreMet(
        LoanApplication application,
        [Frozen] ICreditScoreService creditScoreService,
        [Frozen] IIncomeVerificationService incomeService,
        LoanEvaluationService sut)
    {
        // Arrange
        var monthlyIncome = 5000m; // High income
        
        application = application with { 
            Amount = 10000,      // Below annual income
            LoanTermMonths = 36, // Reasonable term
            Age = 30,
            EmploymentStatus = EmploymentStatus.Employed,
            HasPreviousDefaults = false,
            MonthlyDebt = 100    // Low existing debt
        };

        creditScoreService.GetScore(application.CustomerId).Returns(700);
        incomeService.GetMonthlyIncome(application.CustomerId).Returns(monthlyIncome);

        // Act
        var result = this.InvokeEvaluate(sut, application);

        // Assert
        result.Approved.ShouldBeTrue();
        result.Reason.ShouldBe("Approved");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Evaluate_ShouldCallDependencies_WhenEvaluatingValidApplication(
        LoanApplication application,
        [Frozen] ICreditScoreService creditScoreService,
        [Frozen] IIncomeVerificationService incomeService,
        LoanEvaluationService sut)
    {
        // Arrange
        application = application with { 
            Amount = 1000, 
            LoanTermMonths = 12,
            Age = 30,
            EmploymentStatus = EmploymentStatus.Employed,
            HasPreviousDefaults = false
        };

        creditScoreService.GetScore(application.CustomerId).Returns(700);
        incomeService.GetMonthlyIncome(application.CustomerId).Returns(5000m);

        // Act
        _ = this.InvokeEvaluate(sut, application);

        // Assert
        creditScoreService.Received(1).GetScore(application.CustomerId);
        incomeService.Received(1).GetMonthlyIncome(application.CustomerId);
    }

    #region Helper Methods
    
    private LoanEvaluationResult InvokeEvaluate(
        LoanEvaluationService sut, 
        LoanApplication application)
    {
        return sut.Evaluate(application);
    }
    
    #endregion
}
