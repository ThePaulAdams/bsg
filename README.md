The Plan
Architecture: We will use a Monolithic Repository structure containing three projects:

Supermarket.Core: A Class Library containing your Checkout logic and rules. (Pure C#, no dependencies).

Supermarket.Api: An ASP.NET Core Web API that references Core. It will expose endpoints like POST /scan and GET /total.

Supermarket.Web: A Blazor WebAssembly (WASM) app. This keeps the stack entirely in C# and allows the frontend to be served as static files by the API, meaning you only pay for one service on Railway.

Testing Strategy:

We will keep the NUnit tests in Supermarket.Tests.

"Showing" the tests: To make the tests visible in the live demo without running them on the production server (which is slow and insecure), we will run the tests during the Docker Build process. We will generate an HTML report (using ReportGenerator) and copy it into the website's wwwroot. This way, you can navigate to /test-report on the live site to prove the build passed.

Deployment: A single Dockerfile will build the whole solution, run the tests, generate the report, and publish the API (hosting the Blazor frontend).

1. Folder Structure
Create a folder named SupermarketCheckout. Inside, structure it exactly like this:

Plaintext

SupermarketCheckout/
├── src/
│   ├── Supermarket.Core/       # The logic class library
│   │   ├── Checkout.cs
│   │   ├── PricingRule.cs
│   │   └── Supermarket.Core.csproj
│   ├── Supermarket.Api/        # The backend API
│   │   ├── Controllers/
│   │   ├── Program.cs
│   │   └── Supermarket.Api.csproj
│   └── Supermarket.Web/        # The Blazor Frontend
│       ├── Pages/
│       ├── wwwroot/
│       └── Supermarket.Web.csproj
├── tests/
│   └── Supermarket.Tests/      # NUnit Project
│       ├── CheckoutTests.cs
│       └── Supermarket.Tests.csproj
├── Supermarket.sln
└── Dockerfile                  # The magic file for Railway
2. The Implementation Details
A. The API (Supermarket.Api)
We need a Controller to handle the state. Since HTTP is stateless, we will use a Session or a Singleton Cache (mapped by a Session ID) to store the Checkout instance for the user. For this demo, a MemoryCache using a simplistic "Cart ID" is easiest.

Controllers/CheckoutController.cs

C#

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    // In a real app, use Redis. Here, we use a static dictionary for the demo.
    private static readonly Dictionary<string, Checkout> _carts = new();

    [HttpPost("start")]
    public IActionResult StartCart()
    {
        var cartId = Guid.NewGuid().ToString();
        // Initialize with standard rules
        var rules = new List<PricingRule> 
        { 
            new PricingRule("A", 50, new SpecialOffer(3, 130)),
            new PricingRule("B", 30, new SpecialOffer(2, 45)),
            new PricingRule("C", 20),
            new PricingRule("D", 15)
        };
        _carts[cartId] = new Checkout(rules);
        return Ok(new { CartId = cartId });
    }

    [HttpPost("{cartId}/scan/{item}")]
    public IActionResult Scan(string cartId, string item)
    {
        if (!_carts.ContainsKey(cartId)) return NotFound("Cart not found");
        try 
        {
            _carts[cartId].Scan(item.ToUpper());
            return Ok(new { Total = _carts[cartId].GetTotalPrice() });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{cartId}/total")]
    public IActionResult GetTotal(string cartId)
    {
        if (!_carts.ContainsKey(cartId)) return NotFound("Cart not found");
        return Ok(new { Total = _carts[cartId].GetTotalPrice() });
    }
}
B. The Routing (Program.cs)
We need to tell the API to serve the Blazor files and the Test Report.

C#

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Configure Pipeline
app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles(); // Serves the Blazor WASM files
app.UseStaticFiles();          // Serves wwwroot content
app.UseRouting();
app.UseCors("AllowAll");

app.MapControllers();
// If the request is not an API call, serve the Blazor app (fallback)
app.MapFallbackToFile("index.html"); 

app.Run();
3. The README.md
Place this file in the root of your repository.

Markdown

# Supermarket Checkout Challenge

A full-stack .NET 8 solution demonstrating a flexible supermarket pricing engine. 

## 🚀 Live Demo
**[Insert your Railway URL here]**
* **Checkout UI**: Main page
* **Test Report**: `/reports/index.html` (Generated automatically during deployment)

## 🏗 Architecture
This solution follows **Clean Architecture** principles:
* `Supermarket.Core`: Pure C# Domain logic. Zero external dependencies.
* `Supermarket.Api`: ASP.NET Core Web API. Handles cart state and HTTP requests.
* `Supermarket.Web`: Blazor WebAssembly frontend. Interacts with the API.
* `Supermarket.Tests`: NUnit test suite covering standard and edge cases.

## 🛠 Tech Stack
* **Language**: C# / .NET 8
* **Framework**: ASP.NET Core Web API + Blazor WASM
* **Testing**: NUnit + Coverlet (Coverage) + ReportGenerator
* **Deployment**: Docker (Railway.com)

## 📦 How to Run Locally

1.  **Clone the repo**
    ```bash
    git clone [https://github.com/your-username/supermarket-checkout.git](https://github.com/your-username/supermarket-checkout.git)
    ```
2.  **Run with Docker (Recommended)**
    ```bash
    docker build -t checkout-app .
    docker run -p 8080:8080 checkout-app
    ```
    Visit `http://localhost:8080`.

3.  **Run Manually**
    * Open `Supermarket.sln` in Visual Studio.
    * Right-click `Supermarket.Api` -> "Set as Startup Project".
    * Run (F5).

## ✅ Testing Strategy
The solution includes a comprehensive unit test suite in `Supermarket.Tests`.
To ensure quality, **tests are run inside the Docker build pipeline**. If tests fail, the deployment fails.
We also generate a visible HTML test report accessible via the web interface to prove the build's integrity.

## 📝 API Endpoints
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/api/checkout/start` | Creates a new cart session. Returns `cartId`. |
| `POST` | `/api/checkout/{id}/scan/{item}` | Scans an item (e.g., 'A'). Returns updated total. |
| `GET` | `/api/checkout/{id}/total` | Gets the current cart total. |
4. The Dockerfile (Railway Ready)
This Dockerfile is the critical piece. It performs a "Multi-stage build". It builds the app, runs the tests, generates the HTML report, and then packages everything into a small runtime image.

File: Dockerfile

Dockerfile

# STAGE 1: Build and Test
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install report generator tool locally
RUN dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.1.10
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy csproj files first (caching layer)
COPY ["src/Supermarket.Api/Supermarket.Api.csproj", "src/Supermarket.Api/"]
COPY ["src/Supermarket.Core/Supermarket.Core.csproj", "src/Supermarket.Core/"]
COPY ["src/Supermarket.Web/Supermarket.Web.csproj", "src/Supermarket.Web/"]
COPY ["tests/Supermarket.Tests/Supermarket.Tests.csproj", "tests/Supermarket.Tests/"]

RUN dotnet restore "src/Supermarket.Api/Supermarket.Api.csproj"

# Copy all source code
COPY . .

# Run Tests and Collect Coverage
# If this fails, the Docker build fails (preventing bad code deploy)
WORKDIR /src/tests/Supermarket.Tests
RUN dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Generate HTML Report from coverage
WORKDIR /src
RUN reportgenerator -reports:tests/Supermarket.Tests/TestResults/**/coverage.cobertura.xml -targetdir:TestReport -reporttypes:Html

# Publish the API (which includes the Blazor WASM assets)
WORKDIR /src/src/Supermarket.Api
RUN dotnet publish -c Release -o /app/publish

# STAGE 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Move the Test Report into the web root so it's viewable in the browser
# We create a specific folder for it
RUN mkdir -p wwwroot/reports
COPY --from=build /src/TestReport ./wwwroot/reports

# Railway specific port setup
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Supermarket.Api.dll"]
5. Deployment to Railway
Push this code to a GitHub Repository.

Go to Railway.com -> "New Project" -> "Deploy from GitHub repo".

Select your repo.

Railway will detect the Dockerfile automatically.

Wait for the build.

Note: If your tests in Supermarket.Tests fail, Railway will show a "Build Failed" error, effectively acting as a CI gate.

Once deployed, open the provided URL.

Append /reports/index.html to see your beautiful unit test coverage report.

Next Step
