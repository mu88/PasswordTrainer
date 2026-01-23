# PasswordTrainer

![Combined CI / Release](https://github.com/mu88/PasswordTrainer/actions/workflows/CI_CD.yml/badge.svg)
[![GitHub Tag](https://img.shields.io/github/v/tag/mu88/passwordtrainer?sort=semver&logo=docker&label=release)](https://github.com/mu88/PasswordTrainer/pkgs/container/passwordtrainer)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=mu88_PasswordTrainer&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=mu88_PasswordTrainer)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=mu88_PasswordTrainer&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=mu88_PasswordTrainer)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=mu88_PasswordTrainer&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=mu88_PasswordTrainer)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=mu88_PasswordTrainer&metric=bugs)](https://sonarcloud.io/summary/new_code?id=mu88_PasswordTrainer)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=mu88_PasswordTrainer&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=mu88_PasswordTrainer)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=mu88_PasswordTrainer&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=mu88_PasswordTrainer)

A self-hosted web application for securely training and verifying your memory of passwords, PINs, or passphrases. PasswordTrainer helps you practice recalling sensitive credentials without storing them in plaintext, using strong cryptography and a simple, privacy-focused workflow.


## Features

- **Secure password/PIN training**: Practice recalling passwords or PINs without exposing them in plaintext.
- **Zero plaintext storage**: All secrets are stored encrypted and only checked in-memory.
- **Argon2 hashing**: Uses Argon2 for secure password and PIN verification.
- **Data protection**: Utilizes .NET Data Protection API for encrypting stored secrets.
- **Rate limiting**: Built-in protection against brute-force attacks on the `/check` endpoint using ASP.NET Core Rate Limiting Middleware.
- **Simple web UI**: Minimal, responsive interface for quick password checks.
- **Health check endpoint**: For easy monitoring in production.

## Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download) or later
- [Docker](https://www.docker.com/) (optional, for containerized deployment)

### 1. Clone the repository

```sh
git clone https://github.com/mu88/PasswordTrainer.git
cd PasswordTrainer
```

### 2. Initialize Secrets (First Run)

Before running the app, you must initialize the secret files. This step creates the encrypted password store and required secret files.

#### Using .NET CLI

```sh
dotnet run --project src/PasswordTrainer -- initialize-secrets
```

You will be prompted to enter:
- A new App-PIN (used to unlock the password store)
- The number of passwords to store
- Each password's ID/label and value

This creates the following files in the directories specified by the options:
- `pepper_secret` (random secret)
- `app_pin_hash` (hashed PIN)
- `secrets.json` (encrypted password store)

#### Using Docker

```sh
docker run --rm -it \
  -v $(pwd)/data:/data \
  -v $(pwd)/secrets:/secrets \
  -e Trainer__DataPath=/data \
  -e Trainer__SecretsPath=/secrets \
  mu88/passwordtrainer:latest-chiseled initialize-secrets
```

### 3. Run the Application

#### Using .NET CLI

```sh
dotnet run --project src/PasswordTrainer
```

#### Using Docker

```sh
docker run --rm -p 8080:8080 \
  -v $(pwd)/data:/data \
  -v $(pwd)/secrets:/secrets \
  -e Trainer__DataPath=/data \
  -e Trainer__SecretsPath=/secrets \
  mu88/passwordtrainer:latest-chiseled
```

The web UI will be available at [http://localhost:8080](http://localhost:8080).


### Configuration

PasswordTrainer uses the [Options pattern](src/PasswordTrainer/PasswordTrainerOptions.cs) for configuration. The following environment variables (or appsettings) are required:

- `Trainer__DataPath`: Directory for storing encrypted secrets and data protection keys
- `Trainer__SecretsPath`: Directory for storing secret files (`pepper_secret`, `app_pin_hash`)
- `Trainer__PathBase`: *(Optional)* Sets a custom base path for all endpoints (e.g., `/trainer`). Must start with `/` and contain only alphanumeric characters, dashes, or slashes. If set, the web UI and API endpoints will be available under the specified path (e.g., `/trainer`).

#### Rate Limiting

To protect against brute-force attacks, PasswordTrainer uses ASP.NET Core's Rate Limiting Middleware for the `/check` endpoint. You can configure the following parameters:

- `Trainer__RateLimitingPermitLimit`: Maximum number of allowed requests per window (default: 5)
- `Trainer__RateLimitingWindowMinutes`: Time window in minutes for rate limiting (default: 10)

Example (in `appsettings.json` or as environment variables):

```json
{
  "Trainer": {
    "RateLimitingPermitLimit": 5,
    "RateLimitingWindowMinutes": 10
  }
}
```

This means a single IP address can only attempt to check credentials 5 times within a 10-minute window. Further requests will be blocked until the window resets.

#### Example `appsettings.json` configuration:

```json
{
  "Trainer": {
    "DataPath": "/data",
    "SecretsPath": "/secrets",
    "PathBase": "/trainer",
    "RateLimitingPermitLimit": 5,
    "RateLimitingWindowMinutes": 10
  }
}
```

You can also configure these via Docker `-e` flags.

### Usage Example

1. Open the web UI at `/trainer`.
2. Enter your App-PIN, password ID/label, and password.
3. The app will verify if the password matches the stored (encrypted) value for that ID.

## Support & Documentation

- For issues, use the [GitHub Issues](../../issues) page.
- See [src/PasswordTrainer/PasswordTrainerOptions.cs](src/PasswordTrainer/PasswordTrainerOptions.cs) for configuration details.
- Health check endpoint: `/trainer/healthz`

## Contributing

Contributions are welcome! Please open issues before filing pull requests. See [CONTRIBUTING.md](CONTRIBUTING.md) if available. There is a Dev Container, too, see `.devcontainer/devcontainer.json`.

## License

This project is licensed under the terms of the [LICENSE.md](LICENSE.md) file in this repository.
