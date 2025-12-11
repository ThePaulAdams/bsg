# Supermarket Checkout Challenge

A full-stack .NET 8 solution demonstrating a flexible supermarket pricing engine with automatic special offer application.

## 🚀 Live Demo

**[Insert your Railway URL here after deployment]**

- **Checkout UI**: Main page - Interactive shopping cart
- **Pricing Rules**: `/pricing-rules` - Manage pricing rules dynamically
- **Examples**: `/examples` - 8 detailed scenario examples
- **Test Reports**: `/reports/index.html` - Test execution results (generated during build)
- **API Documentation**: Swagger UI available in development mode

## 🏗 Architecture

This solution follows **Clean Architecture** and **Domain-Driven Design** principles:

### Project Structure

```
bsg/
├── src/
│   ├── Supermarket.Core/          # Pure domain logic (no dependencies)
│   │   ├── Checkout.cs             # Aggregate root
│   │   ├── PricingRule.cs          # Pricing calculation engine
│   │   └── SpecialOffer.cs         # Value object
│   ├── Supermarket.Api/            # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   │   ├── CheckoutController.cs
│   │   │   └── PricingRulesController.cs
│   │   ├── Services/               # Business services
│   │   ├── Models/                 # DTOs
│   │   └── Program.cs
│   └── Supermarket.Web/            # Blazor WebAssembly
│       ├── Pages/
│       │   ├── Index.razor         # Checkout page
│       │   ├── PricingRules.razor  # CRUD for pricing rules
│       │   └── Examples.razor      # Scenario documentation
│       ├── Shared/
│       └── Models/
├── tests/
│   └── Supermarket.Tests/          # NUnit test suite
│       ├── CheckoutTests.cs        # 27 tests
│       ├── PricingRuleTests.cs     # 18 tests
│       ├── SpecialOfferTests.cs    # 7 tests
│       ├── CheckoutControllerTests.cs  # 11 tests
│       └── PricingRulesControllerTests.cs # 18 tests
├── Dockerfile                      # Multi-stage build with test execution
└── Supermarket.sln
```

## 🛠 Tech Stack

- **Language**: C# / .NET 8
- **Backend**: ASP.NET Core Web API 8.0
- **Frontend**: Blazor WebAssembly (WASM)
- **UI Framework**: Bootstrap 5.3.0
- **Testing**: NUnit 4.0
- **Test Reports**: LiquidTestReports (HTML test results)
- **Deployment**: Docker + Railway.com
- **CI/CD**: Integrated in Dockerfile (tests must pass to deploy)

## ✨ Features

### Core Functionality
- ✅ Shopping cart with automatic special offer application
- ✅ Support for items A, B, C, D with configurable pricing
- ✅ Special offers: 3 A's for £1.30, 2 B's for £0.45
- ✅ Case-insensitive item scanning
- ✅ Real-time total calculation

### Extended Features (Beyond Specification)
- ✅ **Dynamic Pricing Rules Management**: Full CRUD operations via GUI
- ✅ **Interactive Web Interface**: Modern Bootstrap 5 design
- ✅ **Comprehensive Test Suite**: 76 tests covering all functionality
- ✅ **Live Test Results**: Viewable in production at `/reports/index.html`
- ✅ **API Documentation**: Swagger/OpenAPI in development
- ✅ **Example Scenarios**: 8 detailed examples with calculations

## 📦 How to Run Locally

### Option 1: Docker (Recommended)

```bash
# Clone the repository
git clone <your-repo-url>
cd bsg

# Build and run with Docker
docker build -t supermarket-checkout .
docker run -p 8080:8080 supermarket-checkout

# Open browser
http://localhost:8080
```

### Option 2: .NET CLI

```bash
# Restore dependencies
dotnet restore

# Run tests
dotnet test

# Run the API (hosts Blazor WASM)
cd src/Supermarket.Api
dotnet run

# Open browser
http://localhost:5000  # or https://localhost:5001
```

### Option 3: Visual Studio / VS Code

1. Open `Supermarket.sln` in Visual Studio
2. Set `Supermarket.Api` as startup project
3. Press F5 to run
4. Browser opens automatically

## 📝 API Endpoints

### Checkout Endpoints

| Method | Endpoint | Description | Request | Response |
|--------|----------|-------------|---------|----------|
| POST | `/api/checkout/start` | Create new cart | None | `{ "cartId": "guid" }` |
| POST | `/api/checkout/{cartId}/scan/{item}` | Scan item | Item code (A-D) | `{ "total": int, "items": {} }` |
| GET | `/api/checkout/{cartId}/total` | Get cart total | None | `{ "total": int, "items": {} }` |
| POST | `/api/checkout/{cartId}/clear` | Clear cart | None | `{ "total": 0, "items": {} }` |
| DELETE | `/api/checkout/{cartId}` | Delete cart | None | 204 No Content |

### Pricing Rules Endpoints (Extended Feature)

| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| GET | `/api/pricingrules` | Get all rules | None | `PricingRuleDto[]` |
| GET | `/api/pricingrules/{itemCode}` | Get rule by code | None | `PricingRuleDto` |
| POST | `/api/pricingrules` | Create new rule | `CreatePricingRuleRequest` | `PricingRuleDto` (201) |
| PUT | `/api/pricingrules/{itemCode}` | Update rule | `UpdatePricingRuleRequest` | `PricingRuleDto` |
| DELETE | `/api/pricingrules/{itemCode}` | Delete rule | None | 204 No Content |
| POST | `/api/pricingrules/reset` | Reset to defaults | None | `{ "message": "..." }` |

## ✅ Testing Strategy

### Test Suite

- **Total Tests**: 76 comprehensive tests
- **Core Domain Tests**: 47 tests
  - CheckoutTests: 27 tests - Cart operations and calculations
  - PricingRuleTests: 18 tests - Pricing logic and special offers
  - SpecialOfferTests: 7 tests - Offer validation
- **API Controller Tests**: 29 tests
  - CheckoutControllerTests: 11 tests - Cart API endpoints
  - PricingRulesControllerTests: 18 tests - CRUD operations

### Key Test Scenarios

```csharp
// Empty basket
[] => £0.00

// Single items
[A] => £0.50
[B] => £0.30
[C] => £0.20
[D] => £0.15

// Special offers
[A, A, A] => £1.30  // 3 for £1.30 (saves £0.20)
[B, B] => £0.45     // 2 for £0.45 (saves £0.15)

// Partial offers
[A, A, A, A] => £1.80  // 3 + 1
[A, A, A, B, B] => £1.75  // Multiple offers

// Large orders
[A × 6, B × 4] => £3.50  // Multiple special offer sets
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with TRX output (for HTML reports)
dotnet test --logger "trx;LogFileName=test-results.trx"

# Generate HTML report (requires LiquidTestReports)
dotnet tool install --global dotnet-liquidtestReports.cli
liquid --inputs TestResults/*.trx --output-directory ./test-report
```

### Build-Time Testing

The Dockerfile runs tests during build:
- ✅ Tests execute in Release configuration
- ✅ Test results captured in TRX format
- ✅ HTML report generated with LiquidTestReports
- ✅ Build fails if any test fails (CI gate)
- ✅ Report served at `/reports/index.html` in production

## 🎨 User Interface

### Pages

1. **Checkout (`/`)**
   - Interactive shopping cart
   - 4 scan buttons (A, B, C, D) with prices
   - Live basket display showing item counts
   - Running total in large green text
   - Clear basket and new cart buttons
   - Special offers sidebar

2. **Pricing Rules (`/pricing-rules`)**
   - Card grid displaying all pricing rules
   - Create, edit, delete operations
   - Modal forms for data entry
   - Reset to defaults functionality
   - Shows unit prices and special offers
   - Calculates savings for each offer

3. **Examples (`/examples`)**
   - 8 detailed scenario examples
   - Bootstrap accordion interface
   - Calculation breakdowns
   - Expected totals
   - Savings highlighted
   - Pricing rules summary table

4. **Test Reports (`/reports/index.html`)**
   - Auto-generated during Docker build
   - Shows all 76 test results
   - Pass/fail status for each test
   - Test execution times and details
   - Proves all tests passed before deployment

## 🚢 Deployment to Railway

### Prerequisites
- GitHub account
- Railway.com account (free tier available)

### Steps

1. **Push to GitHub**
   ```bash
   git remote add origin <your-github-repo>
   git push -u origin main
   ```

2. **Deploy to Railway**
   - Go to [railway.com](https://railway.com)
   - Click "New Project"
   - Select "Deploy from GitHub repo"
   - Choose your repository
   - Railway auto-detects the Dockerfile

3. **Build Process**
   - Railway runs `docker build`
   - Tests execute (build fails if tests fail)
   - Coverage report generates
   - Application deploys

4. **Access Your App**
   - Railway provides a URL (e.g., `your-app.up.railway.app`)
   - Main app: `https://your-app.up.railway.app/`
   - Test report: `https://your-app.up.railway.app/reports/index.html`

### Environment Variables

Railway automatically sets:
- `PORT`: Assigned dynamically (we override with 8080)
- `ASPNETCORE_ENVIRONMENT`: Set to `Production`

No additional configuration required!

## 🏛 Architecture Decisions

### State Management
- **In-memory dictionaries** with static storage
- Thread-safe with lock-based synchronization
- **Trade-off**: Simple but not scalable
- **Production alternative**: Redis or distributed cache

### Pricing Engine
- **Pure domain logic** in Core project
- Zero external dependencies
- Special offers apply automatically
- Calculates remainder at unit price

### Testing Approach
- **Build-time execution** (not runtime)
- HTML test results reports served statically
- Acts as CI/CD gate (build fails if tests fail)
- Visible proof of quality with detailed test outcomes

### Frontend Architecture
- **Blazor WASM** for full C# stack
- Hosted by API (single deployment)
- Bootstrap 5 for responsive UI
- No JavaScript frameworks needed

## 📊 Default Pricing Rules

| Item | Unit Price | Special Offer | Savings |
|------|------------|---------------|---------|
| A | £0.50 | 3 for £1.30 | £0.20 |
| B | £0.30 | 2 for £0.45 | £0.15 |
| C | £0.20 | None | - |
| D | £0.15 | None | - |

*Pricing rules can be modified dynamically via the Pricing Rules page.*

## 🔧 Development

### Prerequisites
- .NET 8 SDK
- Docker (optional)
- Visual Studio 2022 / VS Code / Rider

### Project Commands

```bash
# Build
dotnet build

# Test
dotnet test

# Run API
dotnet run --project src/Supermarket.Api

# Publish
dotnet publish -c Release

# Docker build
docker build -t supermarket-checkout .

# Docker run
docker run -p 8080:8080 supermarket-checkout
```

### Code Structure

- **Core**: Pure C# domain models
- **API**: Controllers, services, DTOs
- **Web**: Razor components, models
- **Tests**: NUnit test fixtures

## 📄 License

This project was created as a coding challenge demonstration.

## 🤖 Credits

Generated with [Claude Code](https://claude.com/claude-code)

---

**Total Implementation**: 12 commits covering full-stack development from architecture to deployment
