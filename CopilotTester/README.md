# CopilotTester

![.NET Badge](https://img.shields.io/badge/.NET-9-512BD4)
![GitHub Copilot](https://img.shields.io/badge/GitHub-Copilot-blue)

This repository provides a practical example to demonstrate how to use GitHub Copilot and Copilot Agents for writing professional unit tests in .NET 9 projects.

## 📋 Content

The example includes:

* **Minimal API (CopilotTester)** for evaluating loan applications.
* **Unit Test Project (CopilotTester.Unittest)** using:
  * ✅ **xUnit** - Testing framework
  * ✅ **Shouldly** - Expressive assertions
  * ✅ **NSubstitute** - Mocking framework
  * ✅ **AutoFixture** - Automatic test data generation

## 🔥 Quick Start

### Option 1: Clone the repository

```powershell
git clone <your-repo-url>
cd CopilotTester
dotnet restore
dotnet build
```

### Option 2: Create from scratch

1. **Create the solution**

```powershell
dotnet new sln -n CopilotTester
```

2. **Create the API project**

```powershell
dotnet new web -n CopilotTester
```

3. **Create the test project**

```powershell
dotnet new xunit -n CopilotTester.Unittest
```

4. **Add projects to solution**

```powershell
dotnet sln add CopilotTester/CopilotTester.csproj
dotnet sln add CopilotTester.Unittest/CopilotTester.Unittest.csproj
```

5. **Add project reference**

```powershell
dotnet add CopilotTester.Unittest/CopilotTester.Unittest.csproj reference CopilotTester/CopilotTester.csproj
```

6. **Add necessary packages to test project**

```powershell
cd CopilotTester.Unittest
dotnet add package AutoFixture.AutoNSubstitute --version 4.18.1
dotnet add package AutoFixture.Xunit2 --version 4.18.1
dotnet add package NSubstitute --version 5.3.0
dotnet add package Shouldly --version 4.3.0
```

## 🚀 About the API

The CopilotTester API exposes the following endpoints:

### `POST /evaluate`

Evaluates a loan application based on the following criteria:

| Criterion | Description | Rule |
|----------|-------------|-------|
| Loan amount | Requested amount | Must be between €1,000 and €50,000 |
| Applicant age | Age in years | Must be over 18 years old |
| Employment status | Employed or self-employed | Must be employed or self-employed |
| Credit score | Applicant's credit score | Must be above 650 |
| Debt-to-income ratio | Monthly debt / Income | Must be below 40% |
| Loan term | Period in months | Between 12 and 60 months |
| Income verification | Statement verification | Must be verifiable |

If the application meets all criteria, the loan is approved.

### Request example

```json
{
  "customerId": "cust123",
  "age": 30,
  "amount": 15000,
  "monthlyDebt": 400,
  "employmentStatus": "Employed",
  "loanTermMonths": 36,
  "hasPreviousDefaults": false
}
```

### Response example

```json
{
  "isApproved": true,
  "reason": "Loan application approved"
}
```

## ✅ Unit Testing Goals

The project includes 10-15 professional unit tests covering:

### Happy paths:
- ✅ Loan approval with valid data
- ✅ Successful income verification

### Edge cases:
- ⚠️ Boundary age values (exactly 18 years old)
- ⚠️ Boundary amount values (€1,000 and €50,000)
- ⚠️ Maximum and minimum loan terms

### Failure scenarios:
- ❌ Invalid age
- ❌ Poor credit history
- ❌ Excessive debt-to-income ratio
- ❌ Failed income verification

### With GitHub Copilot you can:
1. **Write** initial unit tests following .NET best practices
2. **Run** tests automatically using Copilot Agents
3. **Fix and iterate** until all tests pass
4. **Refactor** to improve code quality and maintainability

### Test structure

```csharp
// Example of a basic test structure
public class LoanEvaluationServiceTests
{
    [Theory]
    [InlineData(30, 15000, 36, true)]  // valid case
    [InlineData(17, 15000, 36, false)] // invalid age
    public void EvaluateLoan_ShouldReturnExpectedResult(
        int age, decimal amount, int term, bool expectedResult)
    {
        // Arrange
        var application = new LoanApplication(...);
        var service = new LoanEvaluationService(...);
        
        // Act
        var result = service.EvaluateLoan(application);
        
        // Assert
        result.IsApproved.ShouldBe(expectedResult);
    }
}
```

## 📚 References and Resources

| Tool | Description | Link |
|-------------|-------------|--------|
| xUnit | Unit testing framework for .NET | [Documentation](https://xunit.net/) |
| Shouldly | More readable assertions library | [GitHub](https://github.com/shouldly/shouldly) |
| NSubstitute | Mocking framework for .NET | [Documentation](https://nsubstitute.github.io/) |
| AutoFixture | Automatic test data generation | [GitHub](https://github.com/AutoFixture/AutoFixture) |
| GitHub Copilot | AI coding assistant | [Official page](https://github.com/features/copilot) |

## 🔧 Technologies used

- **.NET 9**: Development framework
- **Minimal APIs**: For creating REST APIs with minimal code
- **System.Text.Json**: For JSON serialization/deserialization
- **xUnit, Shouldly, NSubstitute, and AutoFixture**: For professional unit testing

## 📝 License

This project is under the [MIT](LICENSE) license.

## ✉️ Contact

For any inquiries about this project, you can [open an issue](https://github.com/juanluelguerre/ai-labs/issues) or contact the main author.