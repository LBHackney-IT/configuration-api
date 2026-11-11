# Contributing to the Configuration API

This document covers local setup, build and run, testing, and the development/deployment workflow for the
Configuration API. For an overview of the service, its endpoints and dependencies, see the
[README](README.md).

## Prerequisites

1. [Docker][docker-download] with the **`docker compose`** v2 plugin.
2. A **`LBHPACKAGESTOKEN`** with `read:packages` access to the `LBHackney-IT` GitHub Packages feed — the
   `Hackney.*` packages are not published to nuget.org:
   ```sh
   export LBHPACKAGESTOKEN=<token>
   ```
3. Optionally copy [`.env.sample`](.env.sample) to `.env` and set `LBHPACKAGESTOKEN` there instead of
   exporting it — `docker compose` reads `.env` for the values referenced in `docker-compose.yml`. The file
   also lists every environment variable the API reads at runtime; those are **not** loaded from `.env`, so
   export them (or add them to the compose service) if you need to override the defaults. In particular
   `CONFIGURATION_S3_BUCKETNAME` must be exported (or added to the `configuration-api` compose service,
   which has no `environment` block) for `make serve` to read configuration documents.

## Build and run

```sh
git clone git@github.com:LBHackney-IT/configuration-api.git
cd configuration-api
make setup     # docker compose build
make build     # build the API image
make serve     # start the API (port 3000) and LocalStack
make shell     # bash inside the API container
```

`make serve` starts the API on <http://localhost:3000> together with the `localstack-configuration-api`
service (`localstack/localstack` with the `s3` service, port 4566). Swagger UI is available at `/swagger`;
the health check is at `/api/v1/healthcheck/ping`.

Point the API at LocalStack by exporting `CONFIGURATION_S3_URL=http://localhost:4566`; when it is unset the
client talks to real AWS S3 and needs `AWS_REGION`, `AWS_ACCESS_KEY_ID` and `AWS_SECRET_ACCESS_KEY`.

The API image build context is the `ConfigurationApi/` directory; the test image builds from the repository
root.

## Testing

```sh
make test
```

`make test` starts the LocalStack S3 service, builds the test image
([`ConfigurationApi.Tests/Dockerfile`](ConfigurationApi.Tests/Dockerfile)) and runs the suite inside it
against LocalStack. The image also runs the SonarCloud scanner (requires `SONAR_TOKEN` when run in CI).

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

## Development workflow

- **Formatting** — code must pass `dotnet-format --check`; run `make lint` (or `dotnet format`) to fix
  formatting locally before pushing. CI fails on formatting drift.
- **Branches and review** — work on a feature branch and open a pull request; changes are reviewed and
  approved before merging.
- **Static analysis** — SonarCloud (`sonar-project.properties`) runs as part of the build-and-test job.
- **Infrastructure changes** — environment configuration lives under [`terraform/`](terraform) (one folder
  per environment) and is applied by CI.
- **`make restart-db`** — only relevant to the stale `test-database` / `database/` template leftovers; the
  application itself has no SQL database.

### CI/CD and deployment

CircleCI ([`.circleci/config.yml`](.circleci/config.yml)) gates deployment on a code-formatting check
(`dotnet-format --check`), the Docker test suite and a `sonarcloud/scan` step, then deploys infrastructure
with **Terraform** and the Lambda with the **Serverless Framework**.

**Stages and triggers:**

- **Feature branches** (anything except `master` and `release`) — formatting, tests and Terraform **plan
  previews** against development, staging and production. Nothing is applied or deployed.
- **`master`** — formatting, tests, Terraform apply to development, then deployment to **development**.
- **`release`** — formatting, tests, Terraform apply to staging and deployment to **staging**. Production
  follows only after the `permit-production-terraform-release` and `permit-production-release` manual
  approvals.
- **`release`** — the separate `deploy-terraform-pre-production` and `deploy-code-pre-production` workflows
  deploy **pre-production**, each behind its own manual approval.

**Packaging and runtime:**

- [`ConfigurationApi/build.sh`](ConfigurationApi/build.sh) installs `Amazon.Lambda.Tools` and runs
  `dotnet lambda package --configuration release --framework net10.0 --output-package ./bin/release/net10.0/configuration-api.zip`.
  (`build.cmd` is the Windows equivalent for local use.)
- The deploy job runs `sls deploy --stage <stage> --conceal`.
- [`ConfigurationApi/serverless.yml`](ConfigurationApi/serverless.yml) declares `runtime: dotnet10`,
  consumes that artifact, and exposes `ANY /{proxy+}` plus `GET /swagger/{proxy+}` behind the request
  authorizer, with the `Platform_APIs_Web_ACL` WAF attached.

[docker-download]: https://www.docker.com/products/docker-desktop
[AWS-CLI]: https://aws.amazon.com/cli/
