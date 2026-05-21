## Getting Started

Make sure the `CarvedRock-Aspire.AppHost` project is set as the startup
project.

Run it!

> **N O T E:** This code is based on the [using-csharp-mcp-sdk](https://github.com/dahlsailrunner/using-csharp-mcp-sdk) course repo. The first time you run the app, it may take a little longer to start
if you don't already have the Postgres and Ollama container images downloaded.

Additionally, since there will be a LLM running locally on your computer, the Ollama runtime will start downloading the [Llama3.1](https://ollama.com/library/llama3.1) model which is yet about an additional 3GB. The download progress of the model will be visible on the Aspire dashboard. Next, you might also want to configure where the model will be running, preferrably on the GPU, where you can specify the manufacturer (either Nvidea or AMD). For the Llama 3.1 model, a graphics card with a minimum of 8GB of VRam is recommended. Check the 'builder.AddOllama(...)' section in the app host project for other configuration options.

The Aspire Dashboard will be launched and that will have links for the different
projects.  Start by clicking the link for the WebApp project!

## Features

This is a simple e-commerce application.

Here are the features:

- **API**
  - `GET` based on category (or "all") and by id allow anonymous requests
  - `POST`, `PUT`, and `DELETE` require authentication and an `admin` role (available with the `bob` login, but not `alice`)
  - Validation will be done with [FluentValidation](https://docs.fluentvalidation.net/en/latest/index.html) and errors returned as a `400 Bad Request` with `ProblemDetails`
  - A `GET` with a category of something other than "all", "boots", "equip", or "kayak" will return a `500 internal server error` with `ProblemDetails`
  - Data is seeded by the `SeedData.json` contents in the `Data` project
- Authentication provided by OIDC via a [demo instance of Duende IdentityServer](https://demo.duendesoftware.com) (`bob` is an admin, `alice` is not)

- **WebApp**
  - The home page and listing pages will show a subset of products
  - There is a page at `/Admin` that will show a list of products that can be edited / deleted and new ones added
  - If you navigate to `/Admin` without the admin role, you should see an `AccessDenied` page
  - Any validation errors from the API should be displayed on the admin section add page
  - Can add items to cart and see a summary of the cart (shows when empty too)
  - Can submit an order or cancel the order and clear the cart
  - A submitted order will send a fake email

## VS Code Setup

Running in VS Code is a totally legitimate use-case for this solution and
repo.

The same instructions above (Getting Started) apply here, but the following
extension should probably be installed (it includes some other extensions):

- [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

Then just hit `F5` to run the app.

## Data and EF Core Migrations

The `dotnet ef` tool is used to manage EF Core migrations.  The following command was used to create migrations (from the `CarvedRock.Data` folder).

```bash
dotnet ef migrations add Initial -s ../CarvedRock.Api
```

The application uses PostgreSQL.

## MailKit Client

Added `MailKit.Client` project based on the tutorial here:
<https://learn.microsoft.com/en-us/dotnet/aspire/extensibility/custom-integration>

## Verifying Emails

The very simple email functionality is done using a template
from [this GitHub repo](https://github.com/leemunroe/responsive-html-email-template)
and the [MailPit](https://mailpit.axllent.org/)
service that can easily run in a Docker container.

To see the emails just hit the link for the `smtp` service in the Aspire Dashboard.
