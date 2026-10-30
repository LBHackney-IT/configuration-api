# Configuration API

The Configuration API is the London Borough of Hackney (LBH) **MTFH** service that serves **per-application
configuration and feature toggles**. Each application's configuration is stored as a JSON document in S3,
keyed by the configuration type, and this API returns the requested types to the calling application.

It is a read-only, S3-backed API.

## Stack

- **.NET 10** (`net10.0`) — ASP.NET Core Web API hosted on **AWS Lambda** via `Amazon.Lambda.AspNetCoreServer`
  (the managed `dotnet10` runtime).
- **Amazon S3** as the configuration store (`AWSSDK.S3`).
- **Swashbuckle** for Swagger/OpenAPI, **FluentValidation** for request validation.
- **MVC API versioning** (URL segment reader).
- **AWS X-Ray** for tracing.
- **NUnit** and **xUnit** tests, with FluentAssertions, Moq, AutoFixture and Bogus.

## Endpoints

Versioning uses `UrlSegmentApiVersionReader` with `DefaultApiVersion = 1.0`; the controller is decorated
`[ApiVersion("1.0")]`.

| Verb | Route | Request | Response |
| --- | --- | --- | --- |
| GET | `/api/v1/configuration` | Query: `types` — a repeated query parameter (`?types=First&types=Second`), bound to `string[]` | `200` → `List<ApiConfiguration>`, one entry per configuration key found. Keys that do not exist in S3 are silently omitted. Also declares `500`. |
| GET | `/api/v1/healthcheck/ping` | — | `200` → `{ "success": true }`. Unversioned (the controller carries no `[ApiVersion]`, so it is absent from the Swagger documents) and does not touch S3. |
| GET | `/swagger` | — | Swagger UI / OpenAPI document, one document per API version. |

`ApiConfiguration` contains `Type`, `Configuration` (a dictionary of settings) and `FeatureToggles`
(a dictionary of toggle names to enabled/disabled).

All endpoints sit behind the API Gateway request authorizer and API key.

## Data sources and external dependencies

| Dependency | Detail |
| --- | --- |
| S3 bucket | `configuration-api-configurations-{development\|staging\|production\|pre-production}` (Terraform). The bucket name is read from `CONFIGURATION_S3_BUCKETNAME`. |
| S3 object layout | One JSON object per configuration type — the object **key is the requested configuration type** (`?types=X` reads object `X`). |
| S3 IAM | `s3:GetObject` and `s3:ListBucket` on the bucket and `bucket/*`. Pre-production additionally enables SSE, versioning, public-access-block and a bucket policy. |
| AWS region | `eu-west-2`. |
| Local S3 | **LocalStack** (`localstack/localstack`, service `s3`, port 4566) from `docker-compose.yml`. Tests point at it with `CONFIGURATION_S3_URL` (default `http://localhost:4566`) and bucket `configuration-api-configurations`, using path-style addressing and dummy credentials. |
| AWS X-Ray | Distributed tracing; `AWSXRayDaemonWriteAccess` on the Lambda role. |
| AWS SSM Parameter Store | `/configuration-api/{stage}/bucket-name`; `/housing-tl/{stage}/aspnetcore-environment`. |
| Database | **None.** There is no DynamoDB or SQL database. The `DynamoDb` settings in `appsettings*.json`, `DynamoDb_LocalMode` in the test Dockerfile, `ConfigurationApi.Tests/ConnectionString.cs` and `database/` (`schema.sql` with an `example_table`, a `postgres:12` Dockerfile, `database.env`) are stale template leftovers and are not used by the application. |

## Setup

### Prerequisites

1. [Docker][docker-download] with the **`docker compose`** v2 plugin.
2. A **`LBHPACKAGESTOKEN`** with `read:packages` access to the `LBHackney-IT` GitHub Packages feed — the
   `Hackney.*` packages are not published to nuget.org:
   ```sh
   export LBHPACKAGESTOKEN=<token>
   ```
3. Optionally copy [`.env.sample`](.env.sample) to `.env` and set `LBHPACKAGESTOKEN` there instead of
   exporting it — `docker compose` reads `.env` for the values referenced in `docker-compose.yml`. The file
   also lists every environment variable the API reads at runtime; those are **not** loaded from `.env`, so
   export them (or add them to the compose service) if you need to override the defaults.

### Build and run

```sh
git clone git@github.com:LBHackney-IT/configuration-api.git
cd configuration-api
make setup     # docker compose build
make build     # build the API image
make serve     # start the API (port 3000) and LocalStack
make shell     # bash inside the API container
```

The API image build context is the `ConfigurationApi/` directory; the test image builds from the repository
root.

## Testing

```sh
make test
```

`make test` starts the LocalStack S3 service, builds the test image
(`ConfigurationApi.Tests/Dockerfile`) and runs the suite inside it against LocalStack.

The suite contains both **NUnit** and **xUnit** tests — both frameworks are genuinely in use, so the project
references both `nunit`/`NUnit3TestAdapter` and `xunit`/`xunit.runner.visualstudio`, and both sets of tests
are discovered by `dotnet test`.

Coverage is collected with `coverlet.collector` (`--collect "XPlat Code Coverage;Format=opencover"`).

Note: the E2E test that exercises the configuration endpoint end-to-end
(`GetConfigurationByName.GivenAConfigurationTypeWhenQueryingControllerReturnsJsonFile`) is currently marked
`[Fact(Skip = ...)]` because of a pre-existing test-host dependency-file issue. This is unrelated to the
application itself.

### Agreed testing approach

- xUnit and NUnit, with FluentAssertions, Moq, AutoFixture and Bogus.
- Tests should be independent of each other.
- E2E tests run against a real (containerised) LocalStack S3 instance.
- Unit and E2E tests run in CI.
- Test coverage should never go down.

## Deployment

CircleCI (`.circleci/config.yml`) gates deployment on a code-formatting check (`dotnet-format --check`), the
Docker test suite and a `sonarcloud/scan` step, then deploys infrastructure with **Terraform** and the Lambda
with the **Serverless Framework**. Changes are reviewed and approved on a branch before merging.

**Stages and triggers:**

- **Feature branches** (anything except `master` and `release`) — formatting, tests and Terraform **plan
  previews** against development, staging and production. Nothing is applied or deployed.
- **`master`** — formatting, tests, Terraform apply to development, then deployment to **development**.
- **`release`** — formatting, tests, Terraform apply to staging and deployment to **staging**. Production
  follows only after the `permit-production-terraform-release` and `permit-production-release` manual approvals.
- **`release`** — the separate `deploy-terraform-pre-production` and `deploy-code-pre-production` workflows
  deploy **pre-production**, each behind its own manual approval.

**Packaging and runtime:**

- `build.sh` installs `Amazon.Lambda.Tools` and runs
  `dotnet lambda package --configuration release --framework net10.0 --output-package ./bin/release/net10.0/configuration-api.zip`.
  (`build.cmd` is the Windows equivalent for local use.)
- The deploy job runs `sls deploy --stage <stage> --conceal`.
- `serverless.yml` declares `runtime: dotnet10`, consumes that artifact, and exposes `ANY /{proxy+}` plus
  `GET /swagger/{proxy+}` behind the request authorizer, with the `Platform_APIs_Web_ACL` WAF attached.

## Contacts

**Housing product team** — [mmh-project-team@hackney.gov.uk](mailto:mmh-project-team@hackney.gov.uk)

[docker-download]: https://www.docker.com/products/docker-desktop
[AWS-CLI]: https://aws.amazon.com/cli/
